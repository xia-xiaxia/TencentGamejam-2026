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

            int capturedIndex = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickOption(capturedIndex));
            Debug.Log("EventUIManager 绑定点击: index=" + i + ", button=" + button.name, this);
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
            eventImage.sprite = storyRuntime != null ? storyRuntime.GetCurrentNodeSprite() : null;
            eventImage.enabled = eventImage.sprite != null;
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
            eventImage.sprite = null;
            eventImage.enabled = false;
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
            EnsureOptionClickable(i);
            if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionTexts[i].text = visibleOptions[i] != null ? (visibleOptions[i].Text ?? string.Empty) : string.Empty;
            }
        }
    }

    private void EnsureOptionClickable(int index)
    {
        if (index < 0 || OptionButton == null || index >= OptionButton.Length)
        {
            return;
        }

        Button button = optionButtons != null && index < optionButtons.Length ? optionButtons[index] : null;
        if (button == null)
        {
            return;
        }

        button.enabled = true;
        button.interactable = true;

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic != null)
        {
            targetGraphic.raycastTarget = true;
        }

        CanvasGroup group = button.GetComponentInParent<CanvasGroup>(true);
        if (group != null)
        {
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        CanvasGroup[] groups = button.GetComponentsInParent<CanvasGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            CanvasGroup g = groups[i];
            if (g == null)
            {
                continue;
            }

            g.blocksRaycasts = true;
            g.interactable = true;
            g.alpha = Mathf.Max(g.alpha, 0.01f);
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
