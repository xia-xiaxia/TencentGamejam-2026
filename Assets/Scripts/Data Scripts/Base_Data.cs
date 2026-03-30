using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class StoryDatabase
{
    public List<StoryEventData> events;
    public List<CharacterData> characters;

}

[System.Serializable]
public class StoryEventData
{
    public string id;
    public string Text;
    public List<EffectData> effectDatas;
    public string image; // 存放在 Resources 文件夹下的图片路径
    public List<OptionData> options;

}

[System.Serializable]
public class OptionData
{
    public string Text;
    public string nextNodeId;
    public string requiredItemId;
    public string requiredUnlockedNodeId;
    public string unlockConditionText;
    public string consumeItemId;
    public List<EffectData> effects;

}

[System.Serializable]
public class EffectData
{
    public string targetKey;
    public string type;
    public float floatValue;
    public string stringValue;
}

[System.Serializable]
public enum EffectType
{
    Add,
    Subtract,
    Set,
    Reset,
    UnlockNode,
    ApplyStatus,
    RemoveStatus
}

[System.Serializable]
public class CharacterData
{
    public string id;
    public string name;
    public string description;
    public string image; // 存放在 Resources 文件夹下的图片路径
    public float health;

    public List<ItemData> bag;
}

[System.Serializable]
public class ItemData
{
    public string id;
    public string name;
    public string description;
    public string image; // 存放在 Resources 文件夹下的图片路径
    public List<OptionData> useOption; // 使用物品时的选项数据
}
