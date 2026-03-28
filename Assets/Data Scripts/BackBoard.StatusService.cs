using System.Collections.Generic;

/// <summary>
/// 负责管理可跨回合生效的全局状态。
/// </summary>
public sealed class BackBoardStatusService
{
    private sealed class StatusRuntime
    {
        public float HealthDeltaPerTurn;
        public int RemainingTurns;
    }

    private readonly Dictionary<string, StatusRuntime> statusMap = new Dictionary<string, StatusRuntime>();

    /// <summary>
    /// 添加或覆盖一个状态。
    /// </summary>
    public void ApplyStatus(string statusId, float healthDeltaPerTurn, int durationTurns)
    {
        if (string.IsNullOrEmpty(statusId))
        {
            return;
        }

        StatusRuntime runtime = new StatusRuntime
        {
            HealthDeltaPerTurn = healthDeltaPerTurn,
            RemainingTurns = durationTurns
        };

        statusMap[statusId] = runtime;
    }

    /// <summary>
    /// 移除指定状态。
    /// </summary>
    public void RemoveStatus(string statusId)
    {
        if (string.IsNullOrEmpty(statusId))
        {
            return;
        }

        statusMap.Remove(statusId);
    }

    /// <summary>
    /// 结算一回合状态效果并返回生命值改变量。
    /// </summary>
    public float TickAndGetHealthDelta()
    {
        if (statusMap.Count == 0)
        {
            return 0f;
        }

        float healthDelta = 0f;
        List<string> expiredStatus = null;

        foreach (KeyValuePair<string, StatusRuntime> pair in statusMap)
        {
            StatusRuntime runtime = pair.Value;
            if (runtime == null)
            {
                continue;
            }

            healthDelta += runtime.HealthDeltaPerTurn;

            if (runtime.RemainingTurns > 0)
            {
                runtime.RemainingTurns--;
                if (runtime.RemainingTurns <= 0)
                {
                    if (expiredStatus == null)
                    {
                        expiredStatus = new List<string>();
                    }

                    expiredStatus.Add(pair.Key);
                }
            }
        }

        if (expiredStatus != null)
        {
            for (int i = 0; i < expiredStatus.Count; i++)
            {
                statusMap.Remove(expiredStatus[i]);
            }
        }

        return healthDelta;
    }

    /// <summary>
    /// 清空全部状态。
    /// </summary>
    public void Clear()
    {
        statusMap.Clear();
    }
}
