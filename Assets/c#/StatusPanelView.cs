using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatusPanelView : MonoBehaviour
{
    public TextMeshProUGUI buffListText; // 用一个长文本显示所有 Buff

    public void Initialize(PlayerModel model)
    {
        model.OnBuffsChanged += RefreshBuffs;
        RefreshBuffs(model.Buffs);
    }

    private void RefreshBuffs(List<string> buffs)
    {
        // 将 Buff 列表转为换行字符串
        buffListText.text = string.Join("\n", buffs);
        if (buffs.Count == 0) buffListText.text = "暂无异常状态";
    }
}