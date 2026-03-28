using System.Collections.Generic;
using UnityEngine;

public partial class BackBoard
{
    public bool SetCurrentCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        if (!characterMap.ContainsKey(characterId))
        {
            Debug.LogWarning("SetCurrentCharacter 失败：未找到角色 " + characterId, this);
            return false;
        }

        currentCharacterId = characterId;
        if (OnCurrentCharacterChanged != null)
        {
            OnCurrentCharacterChanged(characterMap[characterId]);
        }

        return true;
    }

    public CharacterData GetCurrentCharacter()
    {
        return GetCharacter(currentCharacterId);
    }

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

    public bool AddItemToBag(string characterId, ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        List<ItemData> bag = GetBag(characterId);
        if (bag == null)
        {
            Debug.LogWarning("AddItemToBag 失败：未找到角色 " + characterId, this);
            return false;
        }

        bag.Add(item);
        NotifyBlackboardChanged("bag:" + characterId);
        return true;
    }

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
                NotifyBlackboardChanged("bag:" + characterId);
                return true;
            }
        }

        return false;
    }

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
}
