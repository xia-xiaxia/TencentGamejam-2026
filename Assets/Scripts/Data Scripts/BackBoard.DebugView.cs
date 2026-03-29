using System.Collections.Generic;

public partial class BackBoard
{
    /// <summary>
    /// 获取剧情全量节点 id 列表（调试用途）。
    /// </summary>
    public List<string> GetAllNodeIds()
    {
        return storyService.GetAllNodeIds();
    }

    /// <summary>
    /// 获取整局累计玩过的节点 id 列表（调试用途）。
    /// </summary>
    public List<string> GetPlayedNodeIds()
    {
        return storyService.GetPlayedNodeIds();
    }

    /// <summary>
    /// 获取本条命经过的节点 id 列表（调试用途）。
    /// </summary>
    public List<string> GetCurrentLifeNodeIds()
    {
        return storyService.GetCurrentLifeNodeIds();
    }

    /// <summary>
    /// 获取整局累计玩过的节点 id 列表（兼容旧命名，调试用途）。
    /// </summary>
    public List<string> GetUnlockedNodeIds()
    {
        return storyService.GetPlayedNodeIds();
    }

    /// <summary>
    /// 获取已解锁选项 key 列表（调试用途）。
    /// </summary>
    public List<string> GetUnlockedOptionKeys()
    {
        return storyService.GetUnlockedOptionKeys();
    }

    /// <summary>
    /// 获取当前生效状态摘要（调试用途）。
    /// </summary>
    public List<string> GetActiveStatusSummaries()
    {
        return statusService.GetActiveStatusSummaries();
    }
}
