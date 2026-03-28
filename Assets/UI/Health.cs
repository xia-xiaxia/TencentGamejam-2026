using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public float currentHealth;
    public float maxHealth;
    public Image healthBar;

    private void Start()
    {
        BindAndRefresh();
    }

    private void OnEnable()
    {
        BindBackBoardEvents();
        RefreshHealthDisplay();
    }

    private void OnDisable()
    {
        UnbindBackBoardEvents();
    }

    private void OnDestroy()
    {
        UnbindBackBoardEvents();
    }

    private void BindAndRefresh()
    {
        BindBackBoardEvents();
        RefreshHealthDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeHealth(5f);
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
            RefreshHealthDisplay();
        }
    }

    private void HandleCurrentCharacterChanged(CharacterData _)
    {
        RefreshHealthDisplay();
    }

    private void RefreshHealthDisplay()
    {
        if (BackBoard.Instance == null)
        {
            currentHealth = 0f;
            maxHealth = 0f;
            SetHealthFill(0f);
            return;
        }

        maxHealth = BackBoard.Instance.GetCurrentMaxHealth();
        currentHealth = BackBoard.Instance.GetCurrentHealth();

        float fill = maxHealth > 0f ? currentHealth / maxHealth : 0f;
        SetHealthFill(fill);
    }

    private void SetHealthFill(float fill)
    {
        if (healthBar == null)
        {
            return;
        }

        healthBar.fillAmount = Mathf.Clamp01(fill);
    }

    private void ChangeHealth(float delta = 5f)
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        float newHealth = currentHealth - delta;
        BackBoard.Instance.SetCurrentHealth(newHealth);
    }
}
