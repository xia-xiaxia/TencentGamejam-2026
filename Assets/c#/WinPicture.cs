using UnityEngine;
using UnityEngine.UI;

public class ImageByID : MonoBehaviour
{
    [Header("=== 要切换的图片 ===")]
    public Image targetImage;  // 你拖入需要切换的图片

    [Header("=== ID对应图片配置 ===")]
    public IdImagePair[] idImages;

    // 当前显示的ID
    private string currentId = "0";

    [System.Serializable]
    public class IdImagePair
    {
        public string id;          // 自己定义的ID（如 0,1,2,A1,A2）
        public Sprite showSprite;  // 对应显示的图片
    }

    void Start()
    {
        // 一开始就刷新显示
        RefreshImage();
    }

    /// <summary>
    /// 外部调用：传入ID，自动切换对应图片
    /// 例：FindAnyObjectByType<ImageByID>().SetImageID("1");
    /// </summary>
    public void SetImageID(string newId)
    {
        currentId = newId;
        RefreshImage();
    }

    // 刷新图片
    void RefreshImage()
    {
        if (targetImage == null) return;

        // 根据ID找到对应图片
        var pair = System.Array.Find(idImages, p => p.id == currentId);

        if (pair != null)
        {
            targetImage.sprite = pair.showSprite;
        }
    }
}