using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽象剧情运行时能力，供 UI 与系统通过接口访问而非依赖具体实现。
/// </summary>
public interface IStoryRuntime
{
    /// <summary>
    /// 节点切换时触发。
    /// </summary>
    event Action<StoryEventData> OnNodeChanged;

    /// <summary>
    /// 当前节点数据。
    /// </summary>
    StoryEventData CurrentNode { get; }

    /// <summary>
    /// 获取当前节点用于展示的选项列表。
    /// </summary>
    List<OptionData> GetVisibleOptions();

    /// <summary>
    /// 判断当前节点中指定索引的选项是否已解锁可点击。
    /// </summary>
    bool IsOptionUnlocked(int optionIndex);

    /// <summary>
    /// 获取整局累计玩过的节点 id 列表。
    /// </summary>
    List<string> GetPlayedNodeIds();

    /// <summary>
    /// 获取本条命经过的节点 id 列表。
    /// </summary>
    List<string> GetCurrentLifeNodeIds();

    /// <summary>
    /// 通过节点 id 获取节点数据。
    /// </summary>
    StoryEventData GetstoryBaseDatabyId(string nodeId);

    /// <summary>
    /// 获取当前节点图片资源。
    /// </summary>
    Sprite GetCurrentNodeSprite();

    /// <summary>
    /// 按可见选项索引执行一次选择。
    /// </summary>
    bool ChooseOption(int optionIndex);

    /// <summary>
    /// 进入游戏结束状态。
    /// </summary>
    bool EndGame(string reason = "");
}
