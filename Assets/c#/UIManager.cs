using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 单例方便程序A调用

    [Header("Event Window")]
    public TextMeshProUGUI contentText;
    public Transform optionsParent;
    public GameObject optionPrefab;

    [Header("Popups")]
    public GameObject[] allPanels; // 拖入背包、状态、树状图面板
    [Header("Visual References")]
    public Image backgroundImage; // 拖入场景中的 Background 对象
    public Image windowImage;     // 拖入场景中的 Window_Frame 对象
    private void Awake() => Instance = this;

    // --- 给程序 A 调用的核心方法 ---
    public void ShowNewEvent(string text, List<OptionInfo> options)
    {
        contentText.text = text;

        // 1. 清理旧选项
        foreach (Transform child in optionsParent) Destroy(child.gameObject);

        // 2. 生成新选项
        foreach (var opt in options)
        {
            var go = Instantiate(optionPrefab, optionsParent);
            go.GetComponent<OptionButtonUI>().Setup(opt.text, opt.isUnlocked, () => {
                Debug.Log("选择了：" + opt.text);
                // 这里写逻辑：通知程序A处理该选项的后果
            });
        }
    }
    public void SetBackground(Sprite newBg)
    {
        if (newBg != null)
        {
            backgroundImage.sprite = newBg;
        }
    }
    // --- 底部导航逻辑 ---
    public void OpenPanel(int index)
    {
        // 关闭所有，打开指定的（简单逻辑）
        foreach (var p in allPanels) p.SetActive(false);
        allPanels[index].SetActive(true);
    }
    public void SetWindowStyle(Sprite newWindowSprite)
    {
        if (newWindowSprite != null)
        {
            windowImage.sprite = newWindowSprite;
        }
    }
}

// 模拟数据结构，实际由程序 A 定义
public struct OptionInfo
{
    public string text;
    public bool isUnlocked;
}