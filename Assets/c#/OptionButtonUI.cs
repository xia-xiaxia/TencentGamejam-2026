using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OptionButtonUI : MonoBehaviour
{
    public TextMeshProUGUI btnText;
    public Button mainButton;
    public GameObject lockIcon; // 锁定的图标（如果有的话）

    // 外部调用：初始化按钮状态
    public void Setup(string text, bool isUnlocked, Action onClickAction)
    {
        btnText.text = text;

        // 如果未解锁：按钮不可交互，颜色变暗
        mainButton.interactable = isUnlocked;
        if (lockIcon != null) lockIcon.SetActive(!isUnlocked);

        // 绑定点击事件
        mainButton.onClick.RemoveAllListeners();
        mainButton.onClick.AddListener(() => onClickAction?.Invoke());
    }
}