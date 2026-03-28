using System.Collections.Generic;
using UnityEngine;

public partial class BackBoard
{
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

        if (!CanShowOption(option))
        {
            Debug.LogWarning("ChooseOption 失败：选项条件未满足。", this);
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
    /// 获取当前节点可显示的选项列表。
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
            if (CanShowOption(option))
            {
                visibleOptions.Add(option);
            }
        }

        return visibleOptions;
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
        
        if (!string.IsNullOrEmpty(option.requiredUnlockedNodeId) && !storyService.IsNodeUnlocked(option.requiredUnlockedNodeId))
        {
            return false;
        }

        if (string.IsNullOrEmpty(option.requiredItemId))
        {
            return true;
        }

        return HasItem(currentCharacterId, option.requiredItemId);
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
        if (node == null || string.IsNullOrEmpty(node.image))
        {
            return null;
        }

        return Resources.Load<Sprite>(node.image);
    }

    /// <summary>
    /// 获取游戏启动时应该进入的首个节点 id。
    /// </summary>
    private bool TryGetStartNodeId(out string nodeId)
    {
        return storyService.TryGetStartNodeId(storyDatabase, startNodeId, out nodeId);
    }
}
