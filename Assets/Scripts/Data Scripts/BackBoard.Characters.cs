using System.Collections.Generic;
using UnityEngine;

public partial class BackBoard
{
    public const string CurrentHealthKey = "character.health";
    public const string LegacyHealthKey = "health";

    /// <summary>
    /// 设置当前操作角色，并同步初始化其生命值状态。
    /// </summary>
    public bool SetCurrentCharacter(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return false;
        }

        if (!characterService.ContainsCharacter(characterId))
        {
            Debug.LogWarning("SetCurrentCharacter 失败：未找到角色 " + characterId, this);
            return false;
        }

        currentCharacterId = characterId;
        InitializeCurrentCharacterHealth();
        if (OnCurrentCharacterChanged != null)
        {
            OnCurrentCharacterChanged(characterService.GetCharacter(characterId));
        }

        return true;
    }

    /// <summary>
    /// 获取当前角色数据。
    /// </summary>
    public CharacterData GetCurrentCharacter()
    {
        return GetCharacter(currentCharacterId);
    }

    /// <summary>
    /// 按角色 id 获取角色数据。
    /// </summary>
    public CharacterData GetCharacter(string characterId)
    {
        return characterService.GetCharacter(characterId);
    }

    /// <summary>
    /// 获取当前角色的最大生命值。
    /// </summary>
    public float GetPlayerHealth()
    {
        CharacterData player = GetCurrentCharacter();
        if (player == null)
        {
            return 0;
        }

        return player.health;
    }

    /// <summary>
    /// 获取当前角色的实时生命值（带上下限约束）。
    /// </summary>
    public float GetCurrentHealth()
    {
        float maxHealth = GetPlayerHealth();
        if (maxHealth <= 0f)
        {
            return GetFloat(CurrentHealthKey, GetFloat(LegacyHealthKey, 0f));
        }

        float health = GetFloat(CurrentHealthKey, GetFloat(LegacyHealthKey, maxHealth));
        return Mathf.Clamp(health, 0f, maxHealth);
    }

    /// <summary>
    /// 获取当前角色生命上限。
    /// </summary>
    public float GetCurrentMaxHealth()
    {
        return Mathf.Max(0f, GetPlayerHealth());
    }

    /// <summary>
    /// 设置当前角色实时生命值。
    /// </summary>
    public void SetCurrentHealth(float value)
    {
        float maxHealth = GetCurrentMaxHealth();
        float clamped = maxHealth > 0f ? Mathf.Clamp(value, 0f, maxHealth) : Mathf.Max(0f, value);
        SetFloat(CurrentHealthKey, clamped);
        SetFloat(LegacyHealthKey, clamped);

        if (maxHealth > 0f && clamped <= 0f)
        {
            EndGame("health depleted");
        }
    }

    /// <summary>
    /// 依据当前角色与黑板值初始化实时生命值。
    /// </summary>
    private void InitializeCurrentCharacterHealth()
    {
        CharacterData character = GetCurrentCharacter();
        if (character == null)
        {
            SetFloat(CurrentHealthKey, 0f);
            SetFloat(LegacyHealthKey, 0f);
            return;
        }

        float maxHealth = Mathf.Max(0f, character.health);
        float boardHealth = GetFloat(CurrentHealthKey, GetFloat(LegacyHealthKey, maxHealth));
        float currentHealth = maxHealth > 0f ? Mathf.Clamp(boardHealth, 0f, maxHealth) : Mathf.Max(0f, boardHealth);

        SetFloat(CurrentHealthKey, currentHealth);
        SetFloat(LegacyHealthKey, currentHealth);
    }

    /// <summary>
    /// 获取指定角色的背包数据。
    /// </summary>
    public List<ItemData> GetBag(string characterId)
    {
        return characterService.GetBag(characterId);
    }

    /// <summary>
    /// 向角色背包添加道具。
    /// </summary>
    public bool AddItemToBag(string characterId, ItemData item)
    {
        if (!characterService.AddItemToBag(characterId, item))
        {
            Debug.LogWarning("AddItemToBag 失败：未找到角色 " + characterId, this);
            return false;
        }

        NotifyBlackboardChanged("bag:" + characterId);
        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            NotifyBlackboardChanged("bag.add:" + item.id);
        }
        return true;
    }

    /// <summary>
    /// 从角色背包移除道具。
    /// </summary>
    public bool RemoveItemFromBag(string characterId, string itemId)
    {
        bool removed = characterService.RemoveItemFromBag(characterId, itemId);
        if (removed)
        {
            NotifyBlackboardChanged("bag:" + characterId);
            if (!string.IsNullOrEmpty(itemId))
            {
                NotifyBlackboardChanged("bag.remove:" + itemId);
            }
        }

        return removed;
    }

    /// <summary>
    /// 清空角色背包。
    /// </summary>
    public bool ClearBag(string characterId)
    {
        if (!characterService.ClearBag(characterId))
        {
            return false;
        }

        NotifyBlackboardChanged("bag:" + characterId);
        return true;
    }

    /// <summary>
    /// 判断角色背包中是否存在道具。
    /// </summary>
    public bool HasItem(string characterId, string itemId)
    {
        return characterService.HasItem(characterId, itemId);
    }

    /// <summary>
    /// 从角色背包中按 id 获取道具。
    /// </summary>
    public ItemData GetItemFromBag(string characterId, string itemId)
    {
        return characterService.GetItemFromBag(characterId, itemId);
    }

    /// <summary>
    /// 获取背包道具描述文本。
    /// </summary>
    public string GetItemDescription(string characterId, string itemId)
    {
        ItemData item = GetItemFromBag(characterId, itemId);
        if (item == null)
        {
            return string.Empty;
        }

        return item.description ?? string.Empty;
    }
}
