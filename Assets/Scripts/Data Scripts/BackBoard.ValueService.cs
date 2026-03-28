using System.Collections.Generic;

/// <summary>
/// 负责黑板键值存储，不包含剧情或角色流程规则。
/// </summary>
public sealed class BackBoardValueService
{
    private readonly Dictionary<string, float> floatBoard = new Dictionary<string, float>();
    private readonly Dictionary<string, string> stringBoard = new Dictionary<string, string>();

    /// <summary>
    /// 获取浮点值，不存在时返回默认值。
    /// </summary>
    public float GetFloat(string key, float defaultValue)
    {
        float value;
        if (floatBoard.TryGetValue(key, out value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// 获取字符串值，不存在时返回默认值。
    /// </summary>
    public string GetString(string key, string defaultValue)
    {
        string value;
        if (stringBoard.TryGetValue(key, out value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// 设置浮点值，并可选同步镜像 key。
    /// </summary>
    public bool SetFloat(string key, float value, string mirrorKeyA, string mirrorKeyB)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        floatBoard[key] = value;

        if (key == mirrorKeyA && !string.IsNullOrEmpty(mirrorKeyB))
        {
            floatBoard[mirrorKeyB] = value;
        }
        else if (key == mirrorKeyB && !string.IsNullOrEmpty(mirrorKeyA))
        {
            floatBoard[mirrorKeyA] = value;
        }

        return true;
    }

    /// <summary>
    /// 叠加浮点值，并可选同步镜像 key。
    /// </summary>
    public bool AddFloat(string key, float delta, string mirrorKeyA, string mirrorKeyB)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        float current = GetFloat(key, 0f);
        return SetFloat(key, current + delta, mirrorKeyA, mirrorKeyB);
    }

    /// <summary>
    /// 设置字符串值。
    /// </summary>
    public bool SetString(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        stringBoard[key] = value;
        return true;
    }

    /// <summary>
    /// 清空全部黑板值。
    /// </summary>
    public void Clear()
    {
        floatBoard.Clear();
        stringBoard.Clear();
    }
}
