using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowView : MonoBehaviour
{
    [Header("UI 引用（与 UIManager 字段一致，方便替换）")]
    public Image backgroundImage;       // 全屏或节点背景
    public Image windowImage;           // 窗口框图
    public TextMeshProUGUI contentText; // 事件正文

    [Header("选项生成（与 UIManager 配置一致）")]
    public Transform optionsParent;     // 存放选项的容器
    public GameObject optionPrefab;     // 预制体

    public void Render(StoryEventData node, List<OptionData> visibleOptions, Action<OptionData> onOptionClick)
    {
        if (node == null)
        {
            ClearOptions();
            if (contentText != null)
            {
                contentText.text = string.Empty;
            }
            return;
        }

        if (contentText != null)
            contentText.text = node.Text ?? string.Empty;

        ClearOptions();

        if (visibleOptions == null || visibleOptions.Count == 0)
            return;

        if (optionPrefab == null || optionsParent == null)
        {
            Debug.LogWarning("WindowView.Render: optionPrefab 或 optionsParent 未绑定，无法生成选项。", this);
            return;
        }

        for (int i = 0; i < visibleOptions.Count; i++)
        {
            OptionData opt = visibleOptions[i];
            if (opt == null) continue;

            GameObject go = Instantiate(optionPrefab, optionsParent);
            if (go == null) continue;

            OptionButtonUI btn = go.GetComponent<OptionButtonUI>();
            if (btn == null)
            {
                Debug.LogWarning("WindowView: optionPrefab 缺少 OptionButtonUI 组件。", this);
                Destroy(go);
                continue;
            }

            btn.Render(opt, true, onOptionClick);
        }
    }

    /// <summary>
    /// 清空容器中已有选项（供其他流程复用）
    /// </summary>
    public void ClearOptions()
    {
        if (optionsParent == null) return;
        for (int i = optionsParent.childCount - 1; i >= 0; i--)
        {
            var child = optionsParent.GetChild(i);
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    // 若将 WindowView 用作事件订阅的视图，可在此缓存 model 并在 OnDestroy 中取消订阅（类似 HPBarView 的做法）。
}