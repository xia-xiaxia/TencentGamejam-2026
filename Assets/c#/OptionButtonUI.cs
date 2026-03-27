using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OptionButtonUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    public TextMeshProUGUI btnText;  // 按钮显示的文字
    public Button mainButton;        // 按钮组件，用于控制是否可点击
    public GameObject lockIcon;      // 锁头图标，当选项锁定时显示

    /// <summary>
    /// 【核心方法】由 UIManager 调用，将数据“画”在按钮上
    /// </summary>
    /// <param name="data">包含文字、是否解锁等信息的数据模型</param>
    /// <param name="onSelect">点击按钮后的回调动作，传回该选项的 ID</param>
    public void Render(OptionModel data, Action<string> onSelect)
    {
        // 1. 设置显示的文字
        btnText.text = data.Text;

        // 2. 处理“解锁/锁定”逻辑
        // 如果 IsUnlocked 为 false，按钮将变为不可交互状态（变灰）
        mainButton.interactable = data.IsUnlocked;

        // 如果有锁头图标，根据解锁状态显示或隐藏
        if (lockIcon != null)
            lockIcon.SetActive(!data.IsUnlocked);

        // 3. 处理点击事件
        // 先清除之前的监听器，防止重复触发
        mainButton.onClick.RemoveAllListeners();
        // 添加新的监听器：点击时执行 onSelect 动作，并带上这个选项的目标事件 ID
        mainButton.onClick.AddListener(() => onSelect?.Invoke(data.TargetEventId));

        /* 【美术预留区】
           你可以在这里加点小动画，比如：
           transform.localScale = Vector3.zero;
           transform.DOScale(Vector3.one, 0.3f); // 按钮弹出效果
        */
    }
}