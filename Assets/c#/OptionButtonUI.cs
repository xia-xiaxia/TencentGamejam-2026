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

    // 缓存以便订阅/取消订阅
    private OptionModel boundModel;
    private Action<string> boundOnSelect;

    /// <summary>
    /// 【核心方法】由 UI 层调用，将数据“画”在按钮上并订阅 OptionModel 的变化
    /// </summary>
    /// <param name="data">包含文字、是否解锁等信息的数据模型</param>
    /// <param name="onSelect">点击按钮后的回调动作，传回该选项的 ID</param>
    public void Render(OptionModel data, Action<string> onSelect)
    {
        // 取消旧订阅
        if (boundModel != null)
        {
            boundModel.OnChanged -= OnModelChanged;
        }

        boundModel = data;
        boundOnSelect = onSelect;

        // 订阅新模型的变化（如果有）
        if (boundModel != null)
        {
            boundModel.OnChanged += OnModelChanged;
        }

        // 首次应用模型到 UI
        ApplyModelToUI();
    }

    private void OnModelChanged(OptionModel model)
    {
        // 当模型变化时更新 UI（来自同一线程）
        ApplyModelToUI();
    }

    private void ApplyModelToUI()
    {
        if (boundModel == null)
        {
            // 清理显示为默认
            if (btnText != null) btnText.text = string.Empty;
            if (mainButton != null) mainButton.onClick.RemoveAllListeners();
            if (lockIcon != null) lockIcon.SetActive(false);
            return;
        }

        // 1. 设置显示的文字
        if (btnText != null)
            btnText.text = boundModel.Text ?? string.Empty;

        // 2. 处理“解锁/锁定”逻辑
        if (mainButton != null)
            mainButton.interactable = boundModel.IsUnlocked;

        if (lockIcon != null)
            lockIcon.SetActive(!boundModel.IsUnlocked);

        // 3. 处理点击事件（先清除之前的监听器，防止重复触发）
        if (mainButton != null)
        {
            mainButton.onClick.RemoveAllListeners();
            mainButton.onClick.AddListener(() => boundOnSelect?.Invoke(boundModel.TargetEventId));
        }
    }

    private void OnDestroy()
    {
        // 取消订阅，防止内存泄漏或回调到已销毁对象
        if (boundModel != null)
        {
            boundModel.OnChanged -= OnModelChanged;
            boundModel = null;
        }
    }
}