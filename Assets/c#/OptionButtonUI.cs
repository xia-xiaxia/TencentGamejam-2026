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
    public float lockedAlpha = 0.55f;

    private OptionData boundOption;
    private Action<OptionData> boundOnSelect;
    private bool isInteractable = true;
    private CanvasGroup rootCanvasGroup;

    private void Awake()
    {
        EnsureBindings();
    }

    private void OnValidate()
    {
        EnsureBindings();
    }

    /// <summary>
    /// 根据 Base_Data 的 OptionData 渲染按钮。
    /// </summary>
    public void Render(OptionData data, bool interactable, Action<OptionData> onSelect)
    {
        EnsureBindings();
        boundOption = data;
        isInteractable = interactable;
        boundOnSelect = onSelect;
        ApplyModelToUI();
    }

    private void EnsureBindings()
    {
        if (mainButton == null)
        {
            mainButton = GetComponent<Button>();
        }

        if (btnText == null)
        {
            btnText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (lockIcon == null)
        {
            Transform lockTransform = transform.Find("LockIcon");
            if (lockTransform != null)
            {
                lockIcon = lockTransform.gameObject;
            }
        }

        if (rootCanvasGroup == null)
        {
            rootCanvasGroup = GetComponent<CanvasGroup>();
            if (rootCanvasGroup == null)
            {
                rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void ApplyModelToUI()
    {
        if (boundOption == null)
        {
            if (btnText != null) btnText.text = string.Empty;
            if (mainButton != null) mainButton.onClick.RemoveAllListeners();
            if (lockIcon != null) lockIcon.SetActive(false);
            return;
        }

        if (btnText != null)
            btnText.text = BuildDisplayText(boundOption, isInteractable);

        if (mainButton != null)
        {
            mainButton.interactable = isInteractable;
            mainButton.onClick.RemoveAllListeners();
            if (isInteractable && boundOnSelect != null)
            {
                mainButton.onClick.AddListener(() => boundOnSelect.Invoke(boundOption));
            }
        }

        if (lockIcon != null)
            lockIcon.SetActive(!isInteractable);

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = isInteractable ? 1f : Mathf.Clamp01(lockedAlpha);
            rootCanvasGroup.blocksRaycasts = isInteractable;
            rootCanvasGroup.interactable = isInteractable;
        }

        if (mainButton == null)
        {
            Debug.LogWarning("OptionButtonUI 未找到 Button 组件，无法绑定点击事件。", this);
        }
    }

    private static string BuildDisplayText(OptionData option, bool interactable)
    {
        if (option == null)
        {
            return string.Empty;
        }

        string baseText = option.Text ?? string.Empty;
        bool requiresNodeUnlock = !string.IsNullOrEmpty(option.requiredUnlockedNodeId);
        if (!requiresNodeUnlock || interactable)
        {
            return baseText;
        }

        string hint = option.unlockConditionText;
        if (string.IsNullOrEmpty(hint))
        {
            hint = "需解锁节点: " + option.requiredUnlockedNodeId;
        }

        return baseText + "（" + hint + "）";
    }
}