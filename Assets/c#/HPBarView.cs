using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarView : MonoBehaviour
{
    public Image hpImage;
    public TextMeshProUGUI hpText;
    public Sprite[] hpSprites = new Sprite[5]; // 预设不同血量阶段的图片

    public void Initialize()
    {
        BindBackBoardEvents();
        Refresh();
    }

    private void OnEnable()
    {
        BindBackBoardEvents();
        Refresh();
    }

    private void OnDisable()
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

        if (hpText != null)
        {
            hpText.text = current.ToString("0") ;
        }

        if (hpImage != null)
        {
            hpImage.fillAmount = Mathf.Clamp01(ratio);
        }
    }

    private void BindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        BackBoard.Instance.OnBlackboardValueChanged += HandleBlackboardValueChanged;
        BackBoard.Instance.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
        BackBoard.Instance.OnCurrentCharacterChanged += HandleCurrentCharacterChanged;
    }

    private void UnbindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        BackBoard.Instance.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
    }

    private void HandleBlackboardValueChanged(string key)
    {
        if (key == BackBoard.CurrentHealthKey || key == BackBoard.LegacyHealthKey)
        {
            Refresh();
        }
    }

    private void HandleCurrentCharacterChanged(CharacterData _)
    {
        Refresh();
    }
}