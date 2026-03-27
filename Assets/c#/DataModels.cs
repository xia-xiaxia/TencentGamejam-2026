using System.Collections.Generic;
using UnityEngine;

// 单个选项的数据模型
public class OptionModel
{
    public string Text;           // 选项显示的文字
    public bool IsUnlocked;       // 该选项当前是否满足解锁条件
    public string TargetEventId;  // 点击后要跳转到的下一个事件 ID
}

// 整个回合的数据模型
public class RoundModel
{
    public string Content;              // 回合描述文本
    public Sprite BgSprite;             // 本回合的背景图
    public Sprite WindowSprite;         // 本回合的对话框皮肤
    public List<OptionModel> Options;   // 本回合拥有的所有选项列表
}