using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 单例模式：让程序 A 或 Controller 随时能通过 UIManager.Instance 找到它
    public static UIManager Instance;

    [Header("文本与容器引用")]
    public TextMeshProUGUI eventContentText; // 事件描述的正文
    public Transform optionsParent;          // 存放选项按钮的父节点 (带 VerticalLayoutGroup)

    [Header("环境与窗口引用")]
    public Image backgroundImage;            // 全屏背景图
    public Image windowImage;                // 事件弹窗的底框图

    [Header("预制体")]
    public GameObject optionPrefab;          // 刚才做好的 Option_Button_Prefab

    private void Awake() => Instance = this;

    /// <summary>
    /// 【核心接口】当“回合数据”发生变化时，Controller 会调用此方法
    /// </summary>
    /// <param name="model">程序 A 传来的完整回合数据包</param>
    /// <param name="onOptionClick">当玩家点击某个选项时，UI 层通知逻辑层的方法</param>
    public void OnRoundDataChanged(RoundModel model, System.Action<string> onOptionClick)
    {
        // 1. 刷新文本内容
        eventContentText.text = model.Content;

        // 2. 刷新背景与窗口皮肤（如果数据里提供了新的 Sprite）
        if (model.BgSprite != null) backgroundImage.sprite = model.BgSprite;
        if (model.WindowSprite != null) windowImage.sprite = model.WindowSprite;

        // 3. 清理旧的选项按钮
        // 遍历所有子物体并销毁
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);

        // 4. 根据数据模型生成新的选项按钮
        foreach (var optData in model.Options)
        {
            // 实例化预制体
            GameObject go = Instantiate(optionPrefab, optionsParent);
            // 获取按钮身上的脚本并执行渲染逻辑
            OptionButtonUI btnScript = go.GetComponent<OptionButtonUI>();

            // 将单个选项的数据和“点击回调”传给按钮
            btnScript.Render(optData, onOptionClick);
        }
    }
}