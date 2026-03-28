using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 负责维护剧情节点索引与当前节点状态。
/// </summary>
public sealed class BackBoardStoryService
{
    private const string UnlockProgressSaveKey = "BackBoard.UnlockProgress";

    private readonly Dictionary<string, StoryEventData> eventMap = new Dictionary<string, StoryEventData>();
    private readonly HashSet<string> unlockedNodeIds = new HashSet<string>();
    private readonly HashSet<string> unlockedOptionKeys = new HashSet<string>();

    [Serializable]
    private sealed class UnlockProgressData
    {
        public List<string> unlockedNodeIds = new List<string>();
        public List<string> unlockedOptionKeys = new List<string>();
    }

    /// <summary>
    /// 当前节点 id。
    /// </summary>
    public string CurrentNodeId { get; private set; }

    /// <summary>
    /// 当前节点数据。
    /// </summary>
    public StoryEventData CurrentNode
    {
        get
        {
            if (string.IsNullOrEmpty(CurrentNodeId))
            {
                return null;
            }

            StoryEventData node;
            if (eventMap.TryGetValue(CurrentNodeId, out node))
            {
                return node;
            }

            return null;
        }
    }

    /// <summary>
    /// 使用事件列表重建节点索引。
    /// </summary>
    public void BuildEventMap(List<StoryEventData> events, UnityEngine.Object logContext)
    {
        eventMap.Clear();
        unlockedNodeIds.Clear();
        unlockedOptionKeys.Clear();
        CurrentNodeId = null;

        if (events == null)
        {
            return;
        }

        for (int i = 0; i < events.Count; i++)
        {
            StoryEventData evt = events[i];
            if (evt == null || string.IsNullOrEmpty(evt.id))
            {
                continue;
            }

            if (eventMap.ContainsKey(evt.id))
            {
                Debug.LogWarning("BackBoard 检测到重复节点 id: " + evt.id, logContext);
                continue;
            }

            eventMap.Add(evt.id, evt);
        }
    }

    /// <summary>
    /// 尝试切换到指定节点。
    /// </summary>
    public bool TryEnterNode(string nodeId, UnityEngine.Object logContext, out StoryEventData node)
    {
        node = null;
        if (string.IsNullOrEmpty(nodeId))
        {
            Debug.LogWarning("EnterNode 失败：nodeId 为空。", logContext);
            return false;
        }

        if (!eventMap.TryGetValue(nodeId, out node))
        {
            Debug.LogWarning("EnterNode 失败：未找到节点 " + nodeId, logContext);
            return false;
        }

        CurrentNodeId = nodeId;
        unlockedNodeIds.Add(nodeId);
        return true;
    }

    /// <summary>
    /// 清空当前节点，表示剧情运行时已退出节点态。
    /// </summary>
    public void ClearCurrentNode()
    {
        CurrentNodeId = null;
    }

    /// <summary>
    /// 判断某节点是否已解锁（已访问或被显式解锁）。
    /// </summary>
    public bool IsNodeUnlocked(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            return false;
        }

        return unlockedNodeIds.Contains(nodeId);
    }

    /// <summary>
    /// 显式解锁某个节点。
    /// </summary>
    public void UnlockNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId))
        {
            return;
        }

        unlockedNodeIds.Add(nodeId);
    }

    /// <summary>
    /// 获取已解锁节点 id 列表（用于调试展示）。
    /// </summary>
    public List<string> GetUnlockedNodeIds()
    {
        List<string> result = new List<string>(unlockedNodeIds);
        result.Sort(StringComparer.Ordinal);
        return result;
    }

    public string BuildOptionKey(string nodeId, int optionIndex)
    {
        if (string.IsNullOrEmpty(nodeId) || optionIndex < 0)
        {
            return string.Empty;
        }

        return nodeId + "#" + optionIndex;
    }

    public bool IsOptionUnlocked(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return false;
        }

        return unlockedOptionKeys.Contains(optionKey);
    }

    public void UnlockOption(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return;
        }

        unlockedOptionKeys.Add(optionKey);
    }

    public List<string> GetUnlockedOptionKeys()
    {
        List<string> result = new List<string>(unlockedOptionKeys);
        result.Sort(StringComparer.Ordinal);
        return result;
    }

    public void SaveUnlockProgress()
    {
        UnlockProgressData data = new UnlockProgressData();
        data.unlockedNodeIds.AddRange(unlockedNodeIds);
        data.unlockedOptionKeys.AddRange(unlockedOptionKeys);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(UnlockProgressSaveKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 清空所有解锁进度，并删除本地存档键。
    /// </summary>
    public void ResetUnlockProgress()
    {
        unlockedNodeIds.Clear();
        unlockedOptionKeys.Clear();
        CurrentNodeId = null;

        if (PlayerPrefs.HasKey(UnlockProgressSaveKey))
        {
            PlayerPrefs.DeleteKey(UnlockProgressSaveKey);
            PlayerPrefs.Save();
        }
    }

    public void LoadUnlockProgress(UnityEngine.Object logContext)
    {
        if (!PlayerPrefs.HasKey(UnlockProgressSaveKey))
        {
            return;
        }

        string json = PlayerPrefs.GetString(UnlockProgressSaveKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        UnlockProgressData data;
        try
        {
            data = JsonUtility.FromJson<UnlockProgressData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("读取剧情解锁存档失败: " + ex.Message, logContext);
            return;
        }

        if (data == null)
        {
            return;
        }

        if (data.unlockedNodeIds != null)
        {
            for (int i = 0; i < data.unlockedNodeIds.Count; i++)
            {
                string nodeId = data.unlockedNodeIds[i];
                if (!string.IsNullOrEmpty(nodeId) && eventMap.ContainsKey(nodeId))
                {
                    unlockedNodeIds.Add(nodeId);
                }
            }
        }

        if (data.unlockedOptionKeys != null)
        {
            for (int i = 0; i < data.unlockedOptionKeys.Count; i++)
            {
                string optionKey = data.unlockedOptionKeys[i];
                if (IsValidOptionKey(optionKey))
                {
                    unlockedOptionKeys.Add(optionKey);
                }
            }
        }
    }

    private bool IsValidOptionKey(string optionKey)
    {
        if (string.IsNullOrEmpty(optionKey))
        {
            return false;
        }

        int sep = optionKey.LastIndexOf('#');
        if (sep <= 0 || sep >= optionKey.Length - 1)
        {
            return false;
        }

        string nodeId = optionKey.Substring(0, sep);
        string optionIndexText = optionKey.Substring(sep + 1);

        int optionIndex;
        if (!int.TryParse(optionIndexText, out optionIndex) || optionIndex < 0)
        {
            return false;
        }

        StoryEventData node;
        if (!eventMap.TryGetValue(nodeId, out node) || node == null || node.options == null)
        {
            return false;
        }

        return optionIndex < node.options.Count;
    }

    /// <summary>
    /// 根据配置或数据库首节点解析开始节点 id。
    /// </summary>
    public bool TryGetStartNodeId(StoryDatabase database, string configuredStartNodeId, out string nodeId)
    {
        if (!string.IsNullOrEmpty(configuredStartNodeId))
        {
            nodeId = configuredStartNodeId;
            return true;
        }

        if (database != null && database.events != null && database.events.Count > 0)
        {
            StoryEventData firstNode = database.events[0];
            if (firstNode != null && !string.IsNullOrEmpty(firstNode.id))
            {
                nodeId = firstNode.id;
                return true;
            }
        }

        nodeId = null;
        return false;
    }
}
