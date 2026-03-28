using System;
using System.Collections.Generic;
using UnityEngine;

// 单个选项的数据模型
public class OptionModel
{
    public string Text;           // 选项显示的文字
    public bool IsUnlocked;       // 该选项当前是否满足解锁条件
    public string TargetEventId;  // 点击后要跳转到的下一个事件 ID

    // 订阅点：当 OptionModel 数据被修改时通知订阅者（传回自身）
    public Action<OptionModel> OnChanged;

    // 通知方法（外部在修改字段后应调用，或者使用下面的 SetX 方法）
    public void NotifyChanged()
    {
        OnChanged?.Invoke(this);
    }
}
    // 整个回合的数据模型
    public class RoundModel
{
    public string Content;              // 回合描述文本
    public Sprite BgSprite;             // 本回合的背景图
    public Sprite WindowSprite;         // 本回合的对话框皮肤
    public List<OptionModel> Options;   // 本回合拥有的所有选项列表
}
public class PlayerModel
{
    public int HP;
    public int MaxHP;
    public List<string> Buffs; // 存储当前所有 Buff 的名称或 ID

    // 观察者模式：当血量或 Buff 改变时触发
    public Action<int> OnHPChanged;
    public Action<List<string>> OnBuffsChanged;
}

// 背包物品数据
public class InventoryModel
{
    public List<string> ItemNames; // 简易版：只存名字
    public Action<List<string>> OnItemsChanged;
}