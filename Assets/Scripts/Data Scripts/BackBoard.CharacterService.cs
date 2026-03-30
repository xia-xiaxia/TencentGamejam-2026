using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 负责角色索引、当前角色解析与背包操作。
/// </summary>
public sealed class BackBoardCharacterService
{
    private readonly Dictionary<string, CharacterData> characterMap = new Dictionary<string, CharacterData>();

    /// <summary>
    /// 使用角色列表重建角色索引。
    /// </summary>
    public void BuildCharacterMap(List<CharacterData> characters, Object logContext)
    {
        characterMap.Clear();
        if (characters == null)
        {
            return;
        }

        for (int i = 0; i < characters.Count; i++)
        {
            CharacterData character = characters[i];
            if (character == null || string.IsNullOrEmpty(character.id))
            {
                continue;
            }

            if (characterMap.ContainsKey(character.id))
            {
                Debug.LogWarning("BackBoard 检测到重复角色 id: " + character.id, logContext);
                continue;
            }

            if (character.bag == null)
            {
                character.bag = new List<ItemData>();
            }

            characterMap.Add(character.id, character);
        }
    }

    /// <summary>
    /// 判断角色 id 是否存在。
    /// </summary>
    public bool ContainsCharacter(string characterId)
    {
        return !string.IsNullOrEmpty(characterId) && characterMap.ContainsKey(characterId);
    }

    /// <summary>
    /// 解析当前角色 id，若传入无效则回退到首个可用角色。
    /// </summary>
    public string ResolveCurrentCharacterId(string preferredCharacterId)
    {
        if (ContainsCharacter(preferredCharacterId))
        {
            return preferredCharacterId;
        }

        foreach (KeyValuePair<string, CharacterData> pair in characterMap)
        {
            return pair.Key;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取指定角色数据。
    /// </summary>
    public CharacterData GetCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return null;
        }

        CharacterData character;
        if (characterMap.TryGetValue(characterId, out character))
        {
            return character;
        }

        return null;
    }

    /// <summary>
    /// 获取角色背包引用。
    /// </summary>
    public List<ItemData> GetBag(string characterId)
    {
        CharacterData character = GetCharacter(characterId);
        if (character == null)
        {
            return null;
        }

        if (character.bag == null)
        {
            character.bag = new List<ItemData>();
        }

        return character.bag;
    }

    /// <summary>
    /// 向背包追加道具。
    /// </summary>
    public bool AddItemToBag(string characterId, ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return false;
        }

        bag.Add(item);
        return true;
    }

    /// <summary>
    /// 从背包移除指定道具。
    /// </summary>
    public bool RemoveItemFromBag(string characterId, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return false;
        }

        for (int i = 0; i < bag.Count; i++)
        {
            ItemData item = bag[i];
            if (item != null && item.id == itemId)
            {
                bag.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 清空角色背包。
    /// </summary>
    public bool ClearBag(string characterId)
    {
        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return false;
        }

        if (bag.Count == 0)
        {
            return true;
        }

        bag.Clear();
        return true;
    }

    /// <summary>
    /// 清空角色背包，但保留指定 id 的道具。
    /// </summary>
    public bool ClearBagExcept(string characterId, ISet<string> preservedItemIds)
    {
        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return false;
        }

        if (bag.Count == 0)
        {
            return true;
        }

        if (preservedItemIds == null || preservedItemIds.Count == 0)
        {
            bag.Clear();
            return true;
        }

        for (int i = bag.Count - 1; i >= 0; i--)
        {
            ItemData item = bag[i];
            if (item == null || string.IsNullOrEmpty(item.id) || !preservedItemIds.Contains(item.id))
            {
                bag.RemoveAt(i);
            }
        }

        return true;
    }

    /// <summary>
    /// 判断背包中是否包含指定道具。
    /// </summary>
    public bool HasItem(string characterId, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return false;
        }

        for (int i = 0; i < bag.Count; i++)
        {
            ItemData item = bag[i];
            if (item != null && item.id == itemId)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 获取背包中的某个道具。
    /// </summary>
    public ItemData GetItemFromBag(string characterId, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return null;
        }

        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            return null;
        }

        for (int i = 0; i < bag.Count; i++)
        {
            ItemData item = bag[i];
            if (item != null && item.id == itemId)
            {
                return item;
            }
        }

        return null;
    }
}
