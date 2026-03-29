using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarView : MonoBehaviour
{
    public Image hpImage;
    public TextMeshProUGUI hpText;
    public Sprite[] hpSprites = new Sprite[5]; // 预设不同血量阶段的图片

    // 是否在文本中显示最大生命值，例如：65/100。
    public bool showMaxHealthInText = false;

    private BackBoard boundBoard;

    public void Initialize()
    {
        TryBindBackBoardEvents();
        Refresh();
    }

    private void OnEnable()
    {
        TryBindBackBoardEvents();
        Refresh();
    }

    private void LateUpdate()
    {
        // BackBoard 可能晚于 UI 初始化，逐帧尝试绑定并在实例切换时重绑。
        TryBindBackBoardEvents();
    }

    private void OnDisable()
    {
        UnbindBackBoardEvents();
    }

    private void OnDestroy()
    {
        UnbindBackBoardEvents();
    }

    private void Refresh()
    {
        if (BackBoard.Instance == null)
        {
            if (hpText != null)
            {
                hpText.text = "0";
            }

            if (hpImage != null)
            {
                hpImage.fillAmount = 0f;
            }

            return;
        }

        float current = BackBoard.Instance.GetCurrentHealth();
        float max = BackBoard.Instance.GetCurrentMaxHealth();
        float ratio = max > 0f ? current / max : 0f;
        float clampedRatio = Mathf.Clamp01(ratio);

        if (hpText != null)
        {
            hpText.text = showMaxHealthInText
                ? string.Format("{0:0}/{1:0}", current, max)
                : current.ToString("0");
        }

        if (hpImage != null)
        {
            hpImage.fillAmount = clampedRatio;
            UpdateHpSprite(clampedRatio);
        }
    }

    private void UpdateHpSprite(float ratio)
    {
        if (hpImage == null || hpSprites == null || hpSprites.Length == 0)
        {
            return;
        }

        int count = hpSprites.Length;
        int index = Mathf.Clamp(Mathf.FloorToInt(ratio * count), 0, count - 1);
        Sprite stageSprite = hpSprites[index];
        if (stageSprite != null)
        {
            hpImage.sprite = stageSprite;
        }
    }

    private void TryBindBackBoardEvents()
    {
        BackBoard board = BackBoard.Instance;
        if (board == null)
        {
            return;
        }

        if (boundBoard == board)
        {
            return;
        }

        UnbindBackBoardEvents();

        boundBoard = board;
        boundBoard.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        boundBoard.OnBlackboardValueChanged += HandleBlackboardValueChanged;
        boundBoard.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
        boundBoard.OnCurrentCharacterChanged += HandleCurrentCharacterChanged;

        Refresh();
    }

    private void UnbindBackBoardEvents()
    {
        if (boundBoard == null)
        {
            return;
        }

        boundBoard.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        boundBoard.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
        boundBoard = null;
    }

    private void HandleBlackboardValueChanged(string key)
    {
        // 生命值相关 key 变化时刷新；同时对 character.* 做兜底，避免上限改动后不刷新。
        if (key == BackBoard.CurrentHealthKey ||
            key == BackBoard.LegacyHealthKey ||
            (!string.IsNullOrEmpty(key) && key.StartsWith("character.", System.StringComparison.Ordinal)))
        {
            Refresh();
        }
    }

    private void HandleCurrentCharacterChanged(CharacterData _)
    {
        Refresh();
    }
}