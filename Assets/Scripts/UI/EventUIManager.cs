using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EventUIManager : MonoBehaviour
{
    public static EventUIManager Instance { get; private set; }

    [SerializeField] private MonoBehaviour storyRuntimeSource;
    public GameObject eventPanel;
    public GameObject[] OptionButton = new GameObject[4];
    public TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[4];
    public TextMeshProUGUI eventText;
    public Image eventImage;

    private IStoryRuntime storyRuntime;
    private Coroutine bindCoroutine;
    private Button[] optionButtons;
    private Vector2 eventImageBaseSize;
    private bool hasEventImageBaseSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("EventUIManager 存在多个实例，这可能会导致数据混乱。", this);
        }

        if (eventPanel == null)
        {
            eventPanel = gameObject;
            Debug.LogWarning("EventUIManager 未设置 eventPanel，已默认使用当前对象。", this);
        }

        if (eventText == null)
        {
            Debug.LogWarning("EventUIManager 未绑定 eventText，事件文字将无法显示。", this);
        }

        if (eventImage != null)
        {
            eventImageBaseSize = eventImage.rectTransform.sizeDelta;
            hasEventImageBaseSize = true;
            eventImage.preserveAspect = true;
        }

        if (storyRuntimeSource != null)
        {
            storyRuntime = storyRuntimeSource as IStoryRuntime;
            if (storyRuntime == null)
            {
                Debug.LogWarning("EventUIManager 的 storyRuntimeSource 未实现 IStoryRuntime，将尝试自动回退到 BackBoard。", this);
            }
        }

        BindOptionButtons();
    }

    private void OnEnable()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventUIManager 未检测到 EventSystem，UI 按钮将无法点击。", this);
        }

        BindOptionButtons();

        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
        }

        bindCoroutine = StartCoroutine(BindRuntimeWhenReady());
    }

    private void OnDisable()
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
            bindCoroutine = null;
        }

        UnsubscribeStoryEvents();
    }

    private void OnDestroy()
    {
        if (bindCoroutine != null)
        {
            StopCoroutine(bindCoroutine);
            bindCoroutine = null;
        }

        UnsubscribeStoryEvents();
    }

    private IEnumerator BindRuntimeWhenReady()
    {
        while (!ResolveStoryRuntime())
        {
            yield return null;
        }

        SubscribeStoryEvents();
        RefreshFromCurrentNode();
        bindCoroutine = null;
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

    private void SubscribeStoryEvents()
    {
        if (!ResolveStoryRuntime())
        {
            return;
        }

        storyRuntime.OnNodeChanged -= HandleNodeChanged;
        storyRuntime.OnNodeChanged += HandleNodeChanged;
    }

    private void UnsubscribeStoryEvents()
    {
        if (storyRuntime == null)
        {
            return;
        }

        storyRuntime.OnNodeChanged -= HandleNodeChanged;
    }

    private void BindOptionButtons()
    {
        if (OptionButton == null)
        {
            Debug.LogWarning("EventUIManager OptionButton 数组为空，无法绑定点击。", this);
            return;
        }

        if (optionButtons == null || optionButtons.Length != OptionButton.Length)
        {
            optionButtons = new Button[OptionButton.Length];
        }

        for (int i = 0; i < OptionButton.Length; i++)
        {
            GameObject optionRoot = OptionButton[i];
            if (optionRoot == null)
            {
                Debug.LogWarning("EventUIManager 选项对象为空，无法绑定点击: index=" + i, this);
                continue;
            }

            Button button = FindBestButton(optionRoot);
            if (button == null)
            {
                Debug.LogWarning("EventUIManager 选项对象未找到 Button 组件，无法绑定点击: index=" + i, this);
                continue;
            }

            optionButtons[i] = button;
            button.onClick.RemoveAllListeners();
        }
    }

    private static Button FindBestButton(GameObject optionRoot)
    {
        if (optionRoot == null)
        {
            return null;
        }

        Button[] allButtons = optionRoot.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button b = allButtons[i];
            if (b != null && b.gameObject != optionRoot)
            {
                return b;
            }
        }

        return optionRoot.GetComponent<Button>();
    }

    private void RefreshFromCurrentNode()
    {
        if (!ResolveStoryRuntime())
        {
            return;
        }

        HandleNodeChanged(storyRuntime.CurrentNode);
    }

    private void HandleNodeChanged(StoryEventData node)
    {
        Debug.Log("节点变换：" + (node != null ? node.id : "(null)"));
        if (node == null)
        {
            ClearEventView();
            if (eventPanel != null && eventPanel != gameObject)
            {
                SetEventPanelVisible(false);
            }
            return;
        }

        SetEventPanelVisible(true);

        if (eventText != null)
        {
            eventText.text = node.Text ?? string.Empty;
        }

        if (eventImage != null)
        {
            Sprite sprite = storyRuntime != null ? storyRuntime.GetCurrentNodeSprite() : null;
            UpdateEventImage(sprite);
        }

        RefreshOptions();
    }

    private void ClearEventView()
    {
        if (eventText != null)
        {
            eventText.text = string.Empty;
        }

        if (eventImage != null)
        {
            UpdateEventImage(null);
        }

        for (int i = 0; i < OptionButton.Length; i++)
        {
            SetOptionVisible(i, false);
        }
    }

    private void RefreshOptions()
    {
        // 每次刷新重新扫描并绑定，避免运行时层级变化导致缓存失效。
        BindOptionButtons();
        Debug.LogWarning("EventUIManager RefreshOptions 执行。", this);

        for (int i = 0; i < OptionButton.Length; i++)
        {
            SetOptionVisible(i, false);
        }

        if (!ResolveStoryRuntime())
        {
            return;
        }

        var visibleOptions = storyRuntime.GetVisibleOptions();
        int optionCount = Mathf.Min(visibleOptions.Count, OptionButton.Length);
        for (int i = 0; i < optionCount; i++)
        {
            SetOptionVisible(i, true);
            bool isUnlocked = storyRuntime.IsOptionUnlocked(i);
            EnsureOptionState(i, isUnlocked);
            if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionTexts[i].text = BuildOptionDisplayText(visibleOptions[i], isUnlocked);
            }
        }
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

    private void EnsureOptionState(int index, bool isUnlocked)
    {
        if (index < 0 || OptionButton == null || index >= OptionButton.Length)
        {
            return;
        }

        GameObject optionRoot = OptionButton[index];
        if (optionRoot == null)
        {
            return;
        }

        Button button = optionButtons != null && index < optionButtons.Length ? optionButtons[index] : null;
        if (button == null)
        {
            return;
        }

        int capturedIndex = index;
        button.onClick.RemoveAllListeners();
        if (isUnlocked)
        {
            button.onClick.AddListener(() => OnClickOption(capturedIndex));
        }

        button.enabled = true;
        button.interactable = isUnlocked;

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = isUnlocked;
        }

        // 仅控制当前选项根节点，避免多个选项共用父级 CanvasGroup 时互相覆盖透明度。
        CanvasGroup optionGroup = optionRoot.GetComponent<CanvasGroup>();
        if (optionGroup == null)
        {
            optionGroup = optionRoot.AddComponent<CanvasGroup>();
        }

        optionGroup.blocksRaycasts = isUnlocked;
        optionGroup.interactable = isUnlocked;
        optionGroup.alpha = 1f;

        ApplyOptionVisualAlpha(optionRoot, isUnlocked ? 1f : 0.1f);
    }

    /// <summary>
    /// 仅调整选项的非文本可视元素透明度，避免影响文字可读性。
    /// </summary>
    private static void ApplyOptionVisualAlpha(GameObject optionRoot, float alpha)
    {
        if (optionRoot == null)
        {
            return;
        }

        Graphic[] graphics = optionRoot.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null)
            {
                continue;
            }

            if (graphic is TMP_Text)
            {
                continue;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private void SetOptionVisible(int index, bool visible)
    {
        if (OptionButton == null || index < 0 || index >= OptionButton.Length)
        {
            return;
        }

        if (OptionButton[index] != null)
        {
            OptionButton[index].SetActive(visible);
        }
    }

    private void SetEventPanelVisible(bool visible)
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(visible);
        }
    }

    /// <summary>
    /// 每次切换节点都按图片比例刷新显示，避免不同资源比例导致拉伸变形。
    /// </summary>
    private void UpdateEventImage(Sprite sprite)
    {
        if (eventImage == null)
        {
            return;
        }

        eventImage.sprite = sprite;
        eventImage.enabled = sprite != null;
        eventImage.preserveAspect = true;

        if (!hasEventImageBaseSize)
        {
            eventImageBaseSize = eventImage.rectTransform.sizeDelta;
            hasEventImageBaseSize = true;
        }

        if (sprite == null)
        {
            eventImage.rectTransform.sizeDelta = eventImageBaseSize;
            return;
        }

        ResizeEventImageToAspect(sprite);
    }

    private void ResizeEventImageToAspect(Sprite sprite)
    {
        RectTransform rect = eventImage.rectTransform;
        if (rect == null || sprite == null)
        {
            return;
        }

        float baseWidth = Mathf.Max(1f, eventImageBaseSize.x);
        float baseHeight = Mathf.Max(1f, eventImageBaseSize.y);
        float baseAspect = baseWidth / baseHeight;

        float spriteWidth = Mathf.Max(1f, sprite.rect.width);
        float spriteHeight = Mathf.Max(1f, sprite.rect.height);
        float spriteAspect = spriteWidth / spriteHeight;

        if (spriteAspect >= baseAspect)
        {
            rect.sizeDelta = new Vector2(baseWidth, baseWidth / spriteAspect);
            return;
        }

        rect.sizeDelta = new Vector2(baseHeight * spriteAspect, baseHeight);
    }

    public void OnClickOption(int visibleOptionIndex)
    {
        Debug.LogWarning("EventUIManager 收到点击: index=" + visibleOptionIndex, this);

        if (!ResolveStoryRuntime())
        {
            Debug.LogWarning("EventUIManager 点击失败：storyRuntime 不可用。", this);
            return;
        }

        bool success = storyRuntime.ChooseOption(visibleOptionIndex);
        if (!success)
        {
            RefreshOptions();
        }
    }
}
