using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Runtime Source")]
    [SerializeField] private MonoBehaviour storyRuntimeSource;

    [Header("文本与容器引用")]
    public TextMeshProUGUI eventContentText;
    public Transform optionsParent;

    [Header("环境与窗口引用")]
    public Image backgroundImage;
    public Image windowImage;

    [Header("预制体")]
    public GameObject optionPrefab;

    [Header("Views")]
    public HPBarView hpBar;
    public StatusPanelView statusPanel;
    public InventoryPanelView inventoryPanel;

    private IStoryRuntime storyRuntime;

    private void Awake() => Instance = this;

    private void OnEnable()
    {
        ResolveStoryRuntime();
        BindRuntimeEvents();
        RefreshAllViews();
    }

    private void OnDisable()
    {
        UnbindRuntimeEvents();
    }

    private void OnDestroy()
    {
        UnbindRuntimeEvents();
    }

    private bool ResolveStoryRuntime()
    {
        if (storyRuntime != null)
        {
            return true;
        }

        if (storyRuntimeSource != null)
        {
            storyRuntime = storyRuntimeSource as IStoryRuntime;
            if (storyRuntime != null)
            {
                return true;
            }
        }

        if (BackBoard.Instance != null)
        {
            storyRuntime = BackBoard.Instance;
            return true;
        }

        return false;
    }

    private void BindRuntimeEvents()
    {
        if (!ResolveStoryRuntime())
        {
            return;
        }

        storyRuntime.OnNodeChanged -= HandleNodeChanged;
        storyRuntime.OnNodeChanged += HandleNodeChanged;
    }

    private void UnbindRuntimeEvents()
    {
        if (storyRuntime == null)
        {
            return;
        }

        storyRuntime.OnNodeChanged -= HandleNodeChanged;
    }

    private void RefreshAllViews()
    {
        if (storyRuntime != null)
        {
            HandleNodeChanged(storyRuntime.CurrentNode);
        }

        if (hpBar != null)
        {
            hpBar.Initialize();
        }

        if (statusPanel != null)
        {
            statusPanel.Initialize();
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.Initialize();
        }
    }

    private void HandleNodeChanged(StoryEventData node)
    {
        Debug.Log("节点变化：" + (node != null ? node.id : "(null)"));
        if (eventContentText != null)
        {
            eventContentText.text = node != null ? (node.Text ?? string.Empty) : string.Empty;
        }

        if (backgroundImage != null && storyRuntime != null)
        {
            backgroundImage.sprite = node != null ? storyRuntime.GetCurrentNodeSprite() : null;
        }

        RefreshOptions();
    }

    private void RefreshOptions()
    {
        if (optionsParent == null)
        {
            return;
        }

        foreach (Transform child in optionsParent)
        {
            Destroy(child.gameObject);
        }

        if (storyRuntime == null || optionPrefab == null)
        {
            return;
        }

        List<OptionData> visibleOptions = storyRuntime.GetVisibleOptions();
        for (int i = 0; i < visibleOptions.Count; i++)
        {
            OptionData option = visibleOptions[i];
            if (option == null)
            {
                continue;
            }

            GameObject go = Instantiate(optionPrefab, optionsParent);
            OptionButtonUI btnScript = go != null ? go.GetComponent<OptionButtonUI>() : null;
            int capturedIndex = i;
            bool isUnlocked = storyRuntime.IsOptionUnlocked(capturedIndex);
            if (btnScript != null)
            {
                btnScript.Render(option, isUnlocked, isUnlocked ? _ => OnClickOption(capturedIndex) : null);
                continue;
            }

            Button fallbackButton = go != null ? go.GetComponent<Button>() : null;
            if (fallbackButton != null)
            {
                fallbackButton.interactable = isUnlocked;
                fallbackButton.onClick.RemoveAllListeners();
                if (isUnlocked)
                {
                    fallbackButton.onClick.AddListener(() => OnClickOption(capturedIndex));
                }

                TextMeshProUGUI fallbackText = go.GetComponentInChildren<TextMeshProUGUI>(true);
                if (fallbackText != null)
                {
                    fallbackText.text = BuildOptionDisplayText(option, isUnlocked);
                }

                CanvasGroup fallbackGroup = go.GetComponent<CanvasGroup>();
                if (fallbackGroup == null)
                {
                    fallbackGroup = go.AddComponent<CanvasGroup>();
                }

                fallbackGroup.alpha = isUnlocked ? 1f : 0.55f;
                fallbackGroup.blocksRaycasts = isUnlocked;
                fallbackGroup.interactable = isUnlocked;

                continue;
            }

            Debug.LogWarning("UIManager: 选项预制体缺少 OptionButtonUI 和 Button，无法自动绑定点击函数。", this);
        }
    }

    private void OnClickOption(int optionIndex)
    {
        if (storyRuntime == null)
        {
            return;
        }

        storyRuntime.ChooseOption(optionIndex);
    }

    private static string BuildOptionDisplayText(OptionData option, bool isUnlocked)
    {
        if (option == null)
        {
            return string.Empty;
        }

        string baseText = option.Text ?? string.Empty;
        bool requiresNodeUnlock = !string.IsNullOrEmpty(option.requiredUnlockedNodeId);
        if (!requiresNodeUnlock || isUnlocked)
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