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
        if (eventContentText != null)
        {
            eventContentText.text = node != null ? (node.Text ?? string.Empty) : string.Empty;
        }

        if (backgroundImage != null && storyRuntime != null)
        {
            backgroundImage.sprite = storyRuntime.GetCurrentNodeSprite();
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
            if (btnScript != null)
            {
                btnScript.Render(option, true, _ => OnClickOption(capturedIndex));
                continue;
            }

            Button fallbackButton = go != null ? go.GetComponent<Button>() : null;
            if (fallbackButton != null)
            {
                fallbackButton.interactable = true;
                fallbackButton.onClick.RemoveAllListeners();
                fallbackButton.onClick.AddListener(() => OnClickOption(capturedIndex));

                TextMeshProUGUI fallbackText = go.GetComponentInChildren<TextMeshProUGUI>(true);
                if (fallbackText != null)
                {
                    fallbackText.text = option.Text ?? string.Empty;
                }

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
}