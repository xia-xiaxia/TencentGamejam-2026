using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

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
        for (int i = 0; i < OptionButton.Length; i++)
        {
            GameObject buttonObject = OptionButton[i];
            if (buttonObject == null)
            {
                continue;
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                continue;
            }

            int capturedIndex = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClickOption(capturedIndex));
        }
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
            if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
            {
                optionTexts[i].text = visibleOptions[i] != null ? (visibleOptions[i].Text ?? string.Empty) : string.Empty;
            }
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
        if (!ResolveStoryRuntime())
        {
            return;
        }

        bool success = storyRuntime.ChooseOption(visibleOptionIndex);
        if (!success)
        {
            RefreshOptions();
        }
    }
}
