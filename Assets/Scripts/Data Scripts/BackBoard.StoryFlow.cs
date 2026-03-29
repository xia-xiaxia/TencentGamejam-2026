using System.Collections.Generic;
using System.Text;
using UnityEngine;

public partial class BackBoard
{
    /// <summary>
    /// 进入游戏结束状态：清理节点与状态效果，并通知 UI 退出当前事件展示。
    /// </summary>
    public bool EndGame(string reason = "你鼠掉了")
    {
        if (IsGameEnded)
        {
            return false;
        }

        IsGameEnded = true;
        statusService.Clear();

        Debug.Log(string.IsNullOrEmpty(reason) ? "Game Ended." : ("Game Ended: " + reason), this);
        if (ReplayFromStartNodeKeepUnlocked())
        {
            return true;
        }

        storyService.ClearCurrentNode();
        if (OnNodeChanged != null)
        {
            OnNodeChanged(null);
        }

        return false;
    }

    /// <summary>
    /// 完全重新开始：重置玩家状态与全部解锁进度，回到初始节点。
    /// </summary>
    public bool RestartFromScratch()
    {
        storyService.ResetUnlockProgress();
        BuildDatabase();

        string nodeId;
        if (!TryGetStartNodeId(out nodeId))
        {
            Debug.LogWarning("RestartFromScratch 失败：没有可用的开始节点。", this);
            return false;
        }

        return EnterNode(nodeId);
    }

    /// <summary>
    /// 重玩：回到起始节点，但保留已解锁节点与选项解锁状态。
    /// </summary>
    public bool ReplayFromStartNodeKeepUnlocked()
    {
        string nodeId;
        if (!TryGetStartNodeId(out nodeId))
        {
            Debug.LogWarning("Replay 失败：没有可用的开始节点。", this);
            return false;
        }

        // 重玩时重置玩家运行态：清空状态、回满生命、清空当前角色背包。
        statusService.Clear();
        SetCurrentHealth(GetCurrentMaxHealth());

        ClearBag(currentCharacterId);
        storyService.ResetCurrentLifeProgress();

        if (!EnterNode(nodeId))
        {
            return false;
        }

        storyService.SaveUnlockProgress();
        return true;
    }

    /// <summary>
    /// 进入指定剧情节点并触发节点效果与节点切换事件。
    /// </summary>
    public bool EnterNode(string nodeId)
    {
        StoryEventData node;
        if (!storyService.TryEnterNode(nodeId, this, out node))
        {
            return false;
        }

        IsGameEnded = false;

        TickStatusEffects();

        ApplyEffects(node.effectDatas);

        if (OnNodeChanged != null)
        {
            OnNodeChanged(node);
        }

        return true;
    }

    /// <summary>
    /// 按当前可见选项索引执行一次选择流程。
    /// </summary>
    public bool ChooseOption(int optionIndex)
    {
        List<OptionData> visibleOptions = GetVisibleOptions();
        if (optionIndex < 0 || optionIndex >= visibleOptions.Count)
        {
            Debug.LogWarning("ChooseOption 失败：选项索引越界 " + optionIndex, this);
            return false;
        }

        OptionData option = visibleOptions[optionIndex];
        if (option == null)
        {
            Debug.LogWarning("ChooseOption 失败：选项数据为空。", this);
            return false;
        }

        if (!IsOptionUnlocked(optionIndex))
        {
            Debug.LogWarning("ChooseOption 失败：选项尚未解锁。", this);
            return false;
        }

        if (!ConsumeOptionRequiredItem(option))
        {
            Debug.LogWarning("ChooseOption 失败：无法消耗所需道具。", this);
            return false;
        }

        ApplyEffects(option.effects);

        if (string.IsNullOrEmpty(option.nextNodeId))
        {
            return true;
        }

        return EnterNode(option.nextNodeId);
    }

    /// <summary>
    /// 获取当前节点用于展示的选项列表（包含未解锁项）。
    /// </summary>
    public List<OptionData> GetVisibleOptions()
    {
        List<OptionData> visibleOptions = new List<OptionData>();
        StoryEventData node = CurrentNode;
        if (node == null || node.options == null)
        {
            return visibleOptions;
        }

        for (int i = 0; i < node.options.Count; i++)
        {
            OptionData option = node.options[i];
            visibleOptions.Add(option);
        }

        return visibleOptions;
    }

    /// <summary>
    /// 判断当前节点中指定索引的选项是否已解锁可点击。
    /// </summary>
    public bool IsOptionUnlocked(int optionIndex)
    {
        StoryEventData node = CurrentNode;
        if (node == null || node.options == null)
        {
            return false;
        }

        if (optionIndex < 0 || optionIndex >= node.options.Count)
        {
            return false;
        }

        OptionData option = node.options[optionIndex];
        return IsOptionUnlockedInternal(node, optionIndex, option, true);
    }

    /// <summary>
    /// 判断某个选项是否满足展示条件。
    /// </summary>
    public bool CanShowOption(OptionData option)
    {
        if (option == null)
        {
            return false;
        }

        StoryEventData node = CurrentNode;
        if (node == null || node.options == null)
        {
            return false;
        }

        int optionIndex = node.options.IndexOf(option);
        if (optionIndex < 0)
        {
            return false;
        }

        return IsOptionUnlockedInternal(node, optionIndex, option, true);
    }

    private bool IsOptionUnlockedInternal(StoryEventData node, int optionIndex, OptionData option, bool autoUnlockWhenAvailable)
    {
        if (node == null || option == null || optionIndex < 0)
        {
            return false;
        }

        string optionKey = storyService.BuildOptionKey(node.id, optionIndex);
        if (storyService.IsOptionUnlocked(optionKey))
        {
            // 依赖前置节点的选项，必须按本条命节点记录实时判断，不能被历史解锁状态短路。
            if (!string.IsNullOrEmpty(option.requiredUnlockedNodeId))
            {
                return storyService.IsNodeUnlocked(option.requiredUnlockedNodeId);
            }

            return true;
        }

        if (!IsOptionUnlockConditionMet(option))
        {
            return false;
        }

        if (autoUnlockWhenAvailable)
        {
            storyService.UnlockOption(optionKey);
        }

        return true;
    }

    private bool IsOptionUnlockConditionMet(OptionData option)
    {
        if (option == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(option.requiredUnlockedNodeId) && !storyService.IsNodeUnlocked(option.requiredUnlockedNodeId))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(option.requiredItemId) && !HasItem(currentCharacterId, option.requiredItemId))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 执行选项绑定的道具消耗逻辑。
    /// </summary>
    private bool ConsumeOptionRequiredItem(OptionData option)
    {
        if (option == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(option.consumeItemId))
        {
            return true;
        }

        return RemoveItemFromBag(currentCharacterId, option.consumeItemId);
    }

    /// <summary>
    /// 获取当前节点配置的图片资源。
    /// </summary>
    public Sprite GetCurrentNodeSprite()
    {
        StoryEventData node = CurrentNode;
        if (node == null)
        {
            Debug.LogWarning("GetCurrentNodeSprite: CurrentNode 为空。", this);
            return null;
        }

        if (string.IsNullOrEmpty(node.image))
        {
            Debug.LogWarning("GetCurrentNodeSprite: 节点 " + node.id + " 未配置 image。", this);
            return null;
        }

        string imagePath = NormalizeImagePath(node.image);

        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            return sprite;
        }

        StringBuilder spritePath = new StringBuilder(40);
        spritePath.Append("Event/");
        spritePath.Append(imagePath);
        sprite = Resources.Load<Sprite>(spritePath.ToString());
        if (sprite == null)
        {
            Debug.LogWarning("GetCurrentNodeSprite: 资源未找到，原始值=" + node.image + "，尝试路径=" + imagePath + " 或 " + spritePath, this);
        }

        return sprite;
    }

    private static string NormalizeImagePath(string rawImage)
    {
        if (string.IsNullOrEmpty(rawImage))
        {
            return string.Empty;
        }

        string value = rawImage.Trim();

        // 一些表格导出会把路径夹杂换行/制表，先清洗成单行路径。
        value = value
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty)
            .Trim();

        if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
        {
            value = value.Substring(1, value.Length - 2).Trim();
        }

        if (value.StartsWith("=", System.StringComparison.Ordinal))
        {
            int firstQuote = value.IndexOf('"');
            int secondQuote = firstQuote >= 0 ? value.IndexOf('"', firstQuote + 1) : -1;
            if (firstQuote >= 0 && secondQuote > firstQuote)
            {
                value = value.Substring(firstQuote + 1, secondQuote - firstQuote - 1).Trim();
            }
        }

        value = value
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\t", string.Empty)
            .Trim();

        int lastSlash = value.LastIndexOf('/');
        int lastBackSlash = value.LastIndexOf('\\');
        int dotIndex = value.LastIndexOf('.');
        int lastSeparator = lastSlash > lastBackSlash ? lastSlash : lastBackSlash;
        if (dotIndex > lastSeparator)
        {
            value = value.Substring(0, dotIndex);
        }

        return value;
    }

    /// <summary>
    /// 获取游戏启动时应该进入的首个节点 id。
    /// </summary>
    private bool TryGetStartNodeId(out string nodeId)
    {
        return storyService.TryGetStartNodeId(storyDatabase, startNodeId, out nodeId);
    }
}
