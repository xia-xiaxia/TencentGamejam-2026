using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责维护剧情节点索引与当前节点状态。
/// </summary>
public sealed class BackBoardStoryService
{
    private readonly Dictionary<string, StoryEventData> eventMap = new Dictionary<string, StoryEventData>();
    private readonly HashSet<string> unlockedNodeIds = new HashSet<string>();

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
    public void BuildEventMap(List<StoryEventData> events, Object logContext)
    {
        eventMap.Clear();
        unlockedNodeIds.Clear();
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
    public bool TryEnterNode(string nodeId, Object logContext, out StoryEventData node)
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
