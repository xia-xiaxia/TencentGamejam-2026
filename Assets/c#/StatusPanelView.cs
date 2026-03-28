using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatusPanelView : MonoBehaviour
{
    public TextMeshProUGUI buffListText; // 用一个长文本显示所有 Buff

    public void Initialize()
    {
        BindBackBoardEvents();
        RefreshBuffs();
    }

    private void OnEnable()
    {
        BindBackBoardEvents();
        RefreshBuffs();
    }

    private void OnDisable()
    {
        UnbindBackBoardEvents();
    }

    private void RefreshBuffs()
    {
        if (buffListText == null)
        {
            return;
        }

        if (BackBoard.Instance == null)
        {
            buffListText.text = "暂无异常状态";
            return;
        }

        List<string> buffs = BackBoard.Instance.GetActiveStatusSummaries();
        buffListText.text = buffs != null && buffs.Count > 0 ? string.Join("\n", buffs) : "暂无异常状态";
    }

    private void BindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnNodeChanged -= HandleNodeChanged;
        BackBoard.Instance.OnNodeChanged += HandleNodeChanged;
    }

    private void UnbindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnNodeChanged -= HandleNodeChanged;
    }

    private void HandleNodeChanged(StoryEventData _)
    {
        RefreshBuffs();
    }
}