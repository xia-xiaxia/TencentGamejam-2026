using System.Collections.Generic;

public partial class BackBoard
{
    /// <summary>
    /// 获取已解锁节点 id 列表（调试用途）。
    /// </summary>
    public List<string> GetUnlockedNodeIds()
    {
        return storyService.GetUnlockedNodeIds();
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
