using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 背包物品悬浮提示：
/// 附加到【物品预制体】上，鼠标悬浮时显示本地配置的文本
/// 模仿 NodeHoverHandler 写法，与 BagUI 完美适配
/// </summary>
public class ItemHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemIcon; // 物品图标（可选，方便在 Inspector 直接配置）
    [Header("本地配置物品信息（直接在这里写）")]
    public string itemName;        // 物品名称
    [TextArea] public string itemDescription; // 物品描述

    [Header("提示文本设置")]
    public TextMeshProUGUI hoverTipText; // 拖入场景里的提示文本框
    public bool showName = true;
    public bool showDescription = true;
    private void Start()
    {
        hoverTipText = BagUI.Instance.ItemText; // 从 BagUI 获取提示文本组件
        if (hoverTipText != null)
        {
            hoverTipText.gameObject.SetActive(false);
        }
        itemIcon = BagUI.Instance.ItemImage; // 从 BagUI 获取物品图标组件
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(false);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTip();
    }

    void ShowTip()
    {
        if (hoverTipText == null) return;

        // 拼接要显示的文本（本地配置）
        string tip = "";
        if (showName) tip += itemName + "\n";
        if (showDescription) tip += itemDescription;

        hoverTipText.text = tip;
        hoverTipText.gameObject.SetActive(true);
        itemIcon.gameObject.SetActive(true);
    }

    void HideTip()
    {
        if (hoverTipText == null) return;
        hoverTipText.gameObject.SetActive(false);
        if (itemIcon == null) return;
        itemIcon.gameObject.SetActive(false);
    }
}