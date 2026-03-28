using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarView : MonoBehaviour
{
    public Image hpImage;
    public TextMeshProUGUI hpText;
    public Sprite[] hpSprites = new Sprite[5]; // 预设不同血量阶段的图片

    private PlayerModel model;

    public void Initialize(PlayerModel playerModel)
    {
        if (playerModel == null) return;

        // 防止重复订阅：先取消旧的订阅
        if (model != null)
        {
            model.OnHPChanged -= Refresh;
        }

        model = playerModel;
        model.OnHPChanged += Refresh;
        Refresh(model.HP); // 初始化显示
    }

    private void Refresh(int current)
    {
        hpText.text = $"{current}";
        // 美术预留可以更改 Image
    }

    // 在对象销毁时取消订阅（防止内存泄漏）
    private void OnDestroy()
    {
        if (model != null)
        {
            model.OnHPChanged -= Refresh;
        }
    }
}