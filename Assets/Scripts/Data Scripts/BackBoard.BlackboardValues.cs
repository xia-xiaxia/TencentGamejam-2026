using System;
using System.Collections.Generic;
using UnityEngine;

public partial class BackBoard
{
    /// <summary>
    /// 批量应用效果列表。
    /// </summary>
    public void ApplyEffects(List<EffectData> effects)
    {
        if (effects == null)
        {
            return;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i]);
        }
    }

    /// <summary>
    /// 应用单条效果数据到黑板。
    /// </summary>
    public void ApplyEffect(EffectData effect)
    {
        if (effect == null || string.IsNullOrEmpty(effect.targetKey))
        {
            return;
        }

        EffectType effectType;
        if (!TryParseEffectType(effect.type, out effectType))
        {
            effectType = EffectType.Set;
        }

        switch (effectType)
        {
            case EffectType.Add:
                AddFloat(effect.targetKey, effect.floatValue);
                break;
            case EffectType.Subtract:
                AddFloat(effect.targetKey, -effect.floatValue);
                break;
            case EffectType.Set:
                if (!string.IsNullOrEmpty(effect.stringValue))
                {
                    SetString(effect.targetKey, effect.stringValue);
                }
                else
                {
                    SetFloat(effect.targetKey, effect.floatValue);
                }
                break;
            case EffectType.UnlockNode:
                storyService.UnlockNode(effect.targetKey);
                break;
            case EffectType.ApplyStatus:
                ApplyStatusFromEffect(effect);
                break;
            case EffectType.RemoveStatus:
                statusService.RemoveStatus(effect.targetKey);
                break;
        }
    }

    /// <summary>
    /// 处理状态类效果：targetKey 为状态 id，floatValue 为每回合生命改变量，stringValue 为持续回合数（<=0 表示永久）。
    /// </summary>
    private void ApplyStatusFromEffect(EffectData effect)
    {
        if (effect == null || string.IsNullOrEmpty(effect.targetKey))
        {
            return;
        }

        int durationTurns = 0;
        if (!string.IsNullOrEmpty(effect.stringValue))
        {
            int.TryParse(effect.stringValue, out durationTurns);
        }

        statusService.ApplyStatus(effect.targetKey, effect.floatValue, durationTurns);
    }

    /// <summary>
    /// 每回合结算一次全局状态并同步生命值。
    /// </summary>
    private void TickStatusEffects()
    {
        float healthDelta = statusService.TickAndGetHealthDelta();
        if (Mathf.Approximately(healthDelta, 0f))
        {
            return;
        }

        SetCurrentHealth(GetCurrentHealth() + healthDelta);
    }

    /// <summary>
    /// 获取黑板浮点值。
    /// </summary>
    public float GetFloat(string key, float defaultValue = 0f)
    {
        return valueService.GetFloat(key, defaultValue);
    }

    /// <summary>
    /// 获取黑板字符串值。
    /// </summary>
    public string GetString(string key, string defaultValue = "")
    {
        return valueService.GetString(key, defaultValue);
    }

    /// <summary>
    /// 设置黑板浮点值。
    /// </summary>
    public void SetFloat(string key, float value)
    {
        if (valueService.SetFloat(key, value, CurrentHealthKey, LegacyHealthKey))
        {
            NotifyBlackboardChanged(key);
        }
    }

    /// <summary>
    /// 累加黑板浮点值。
    /// </summary>
    public void AddFloat(string key, float delta)
    {
        if (valueService.AddFloat(key, delta, CurrentHealthKey, LegacyHealthKey))
        {
            NotifyBlackboardChanged(key);
        }
    }

    /// <summary>
    /// 设置黑板字符串值。
    /// </summary>
    public void SetString(string key, string value)
    {
        if (valueService.SetString(key, value))
        {
            NotifyBlackboardChanged(key);
        }
    }

    /// <summary>
    /// 清空黑板中的全部值。
    /// </summary>
    public void ClearBoard()
    {
        valueService.Clear();
    }

    private bool TryParseEffectType(string effectTypeString, out EffectType effectType)
    {
        if (string.IsNullOrEmpty(effectTypeString))
        {
            effectType = EffectType.Set;
            return false;
        }

        return Enum.TryParse(effectTypeString, true, out effectType);
    }

    private void NotifyBlackboardChanged(string key)
    {
        if (OnBlackboardValueChanged != null)
        {
            OnBlackboardValueChanged(key);
        }
    }
}
