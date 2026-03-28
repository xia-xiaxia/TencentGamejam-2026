using System;
using System.Collections.Generic;

public partial class BackBoard
{
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
        }
    }

    public float GetFloat(string key, float defaultValue = 0f)
    {
        float value;
        if (floatBoard.TryGetValue(key, out value))
        {
            return value;
        }

        return defaultValue;
    }

    public string GetString(string key, string defaultValue = "")
    {
        string value;
        if (stringBoard.TryGetValue(key, out value))
        {
            return value;
        }

        return defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        floatBoard[key] = value;
        NotifyBlackboardChanged(key);
    }

    public void AddFloat(string key, float delta)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        floatBoard[key] = GetFloat(key) + delta;
        NotifyBlackboardChanged(key);
    }

    public void SetString(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        stringBoard[key] = value;
        NotifyBlackboardChanged(key);
    }

    public void ClearBoard()
    {
        floatBoard.Clear();
        stringBoard.Clear();
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
