using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
public class EventTreeUI : MonoBehaviour
{
    [Header("=== 面板设置 ===")]
    public GameObject treePanel;       // 事件树
    public GameObject detailPanel;     // 详情框
    public TextMeshProUGUI detailText;            // 详情文字

    [Header("=== 节点配置  ===")]
    public EventNode[] eventNodes;

    [Header("=== 连线配置 ===")]
    public EventLine[] eventLines;

    // 当前进度（字符串ID）
    private string currentEventId = "A0";
    private StoryEventData currentNode;
    private string lastClickId = "";

    // 当前悬浮的节点 id（用于退出时判断）
    private string lastHoverId;

    private HashSet<string> unlockedIds = new HashSet<string>(System.StringComparer.Ordinal);

    [System.Serializable]
    public class EventNode
    {
        public string nodeId;
        public GameObject nodeObj;
        [TextArea(2, 5)] public string detailDesc;
    }

    [System.Serializable]
    public class EventLine
    {
        public GameObject lineObj;
        public string fromNodeId;
        public string toNodeId;
    }

    void Awake()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        // 给所有节点加点击（只绑定一次），并添加悬浮处理器
        foreach (var node in eventNodes)
        {
            if (node == null || node.nodeObj == null) continue;

            var btn = node.nodeObj.GetComponent<Button>();
            if (btn == null) btn = node.nodeObj.AddComponent<Button>();

            string clickId = node.nodeId;
            // 防止重复绑定（在编辑器重复进入 Play 时可能重复）
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnNodeClicked(clickId));

            // 添加或配置悬浮处理组件，用于显示悬浮文本
            var hover = node.nodeObj.GetComponent<NodeHoverHandler>();
            if (hover == null) hover = node.nodeObj.AddComponent<NodeHoverHandler>();
            hover.nodeId = node.nodeId;
            hover.owner = this;
        }
    }

    void OnEnable()
    {
        // 订阅 BackBoard 节点变化事件以同步 UI（若存在）
        if (BackBoard.Instance != null)
        {
            BackBoard.Instance.OnNodeChanged += HandleNodeChanged;
            // 初始化当前 id 与解锁集合
            currentEventId = BackBoard.Instance.CurrentNodeId ?? currentEventId;
            UpdateUnlockedFromBackboard();
        }
        RefreshTreeUI();
    }

    void OnDisable()
    {
        if (BackBoard.Instance != null)
        {
            BackBoard.Instance.OnNodeChanged -= HandleNodeChanged;
        }
    }

    void Start()
    {
        foreach (var node in eventNodes)
        {
            if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;
        }
        // 再次保证初始化显示正确（BackBoard 可能在 Start 里触发第一次 OnNodeChanged）
        if (BackBoard.Instance != null)
        {
            currentEventId = BackBoard.Instance.CurrentNodeId ?? currentEventId;
            UpdateUnlockedFromBackboard();
        }
        RefreshTreeUI();
    }

    // 外部调用：更新进度（保留可手动设置的接口）
    public void SetProgress(string newId)
    {
        currentEventId = newId;
        lastClickId = "";
        lastHoverId = null;
        if (detailPanel != null) detailPanel.SetActive(false);
        RefreshTreeUI();
    }

    // 当 BackBoard 通知节点变化时调用
    private void HandleNodeChanged(StoryEventData node)
    {
        // 从 BackBoard 获取当前节点 id 与解锁集合，然后刷新 UI
        if (BackBoard.Instance != null)
        {
            currentEventId = BackBoard.Instance.CurrentNodeId ?? currentEventId;
            currentNode = node ?? (BackBoard.Instance.CurrentNode);
            UpdateUnlockedFromBackboard();
        }
        lastClickId = "";
        lastHoverId = null;
        if (detailPanel != null) detailPanel.SetActive(false);
        RefreshTreeUI();
    }

    // 从 BackBoard 获取已解锁节点列表（通过公开接口）
    private void UpdateUnlockedFromBackboard()
    {
        unlockedIds.Clear();
        if (BackBoard.Instance == null) return;

        // BackBoard 提供 GetUnlockedNodeIds()（在 DebugView 部分），优先使用
        try
        {
            var list = BackBoard.Instance.GetUnlockedNodeIds();
            if (list != null)
            {
                foreach (var id in list) unlockedIds.Add(id);
            }
        }
        catch
        {
            // 如果没有该方法或调用失败，备用：只将当前节点视为已解锁
            if (!string.IsNullOrEmpty(currentEventId)) unlockedIds.Add(currentEventId);
        }
    }

    // 刷新UI状态
    void RefreshTreeUI()
    {
        foreach (var node in eventNodes)
        {
            if (node == null || node.nodeObj == null) continue;

            bool isUnlocked = IsUnlocked(node.nodeId);
            node.nodeObj.SetActive(isUnlocked);
            if (!isUnlocked) continue;

            var img = node.nodeObj.GetComponent<Image>();
            if (img == null) continue;

            if (node.nodeId == currentEventId)
            {
                img.color = Color.yellow; // 当前节点高亮
            }
            else
            {
                img.color = Color.gray;   // 已过节点变灰
            }
        }

        // 刷新连线
        foreach (var line in eventLines)
        {
            if (line == null || line.lineObj == null) continue;
            bool fromOk = IsUnlocked(line.fromNodeId);
            bool toOk = IsUnlocked(line.toNodeId);
            line.lineObj.SetActive(fromOk && toOk);
        }
    }

    // 判断节点是否解锁：优先使用 BackBoard 的已解锁集合，否则沿用本地简单规则
    bool IsUnlocked(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        if (unlockedIds != null && unlockedIds.Count > 0)
        {
            return unlockedIds.Contains(id);
        }
        if (id == currentEventId) return true;
        return id == "A0";
    }

    void OnNodeClicked(string id)
    {
        var node = System.Array.Find(eventNodes, n => n.nodeId == id);
        if (node == null) return;

        // 若节点未解锁则忽略点击（或可显示提示）
        if (!IsUnlocked(id))
        {
            // 可替换为提示逻辑
            Debug.Log($"EventTreeUI: 尝试点击未解锁节点 {id}");
            return;
        }

        // 单击已选中节点并且详情已显示 -> 关闭详情
        if (lastClickId == id && detailPanel != null && detailPanel.activeSelf)
        {
            detailPanel.SetActive(false);
            lastClickId = "";
            return;
        }

        // 点击现在只显示详情（不直接切换节点）
        string displayText = node.detailDesc ?? string.Empty;
        if (detailText != null) detailText.text = displayText;
        if (detailPanel != null) detailPanel.SetActive(true);
        lastClickId = id;
    }

    // ========== 悬浮处理接口 ==========
    // 鼠标进入节点时调用（由 NodeHoverHandler 转发）
    public void OnNodeHoverEnter(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!IsUnlocked(id)) return; // 只有解锁了才显示

        // 1. 尝试从 eventNodes 配置中找默认文本
        var node = System.Array.Find(eventNodes, n => n.nodeId == id);
        string displayText = (node != null) ? node.detailDesc : string.Empty;

        // 2. 【关键修改】：不再只看 CurrentNode，而是根据 ID 获取文本
        // 假设你的 BackBoard 有一个能根据 ID 获取历史节点数据的方法
        if (BackBoard.Instance != null)
        {
            // 如果 BackBoard 能获取任意节点的 StoryEventData
            var historyNode = BackBoard.Instance.GetstoryBaseDatabyId(id);
            Debug.Log(IsUnlocked(id) ? $"EventTreeUI: 节点 {id} 已解锁，尝试获取文本。" : $"EventTreeUI: 节点 {id} 未解锁，无法获取文本。");  
            if (historyNode != null && !string.IsNullOrEmpty(historyNode.Text))
            {
                displayText = historyNode.Text;
                Debug.Log($"EventTreeUI: 获取到节点 {id} 的文本：{displayText}");
            }
        }

        // 3. 渲染
        if (detailText != null) detailText.text = displayText;
        if (detailPanel != null)
        {
            detailPanel.SetActive(true);
            lastHoverId = id;
        }
    }

    // 鼠标离开节点时调用（由 NodeHoverHandler 转发）
    public void OnNodeHoverExit(string id)
    {
        if (string.IsNullOrEmpty(id)) return;

        // 如果当前该节点被点击并保持展示，则不要在离开时关闭
        if (!string.IsNullOrEmpty(lastClickId) && lastClickId == id) return;

        if (!string.IsNullOrEmpty(lastHoverId) && lastHoverId == id)
        {
            if (detailPanel != null) detailPanel.SetActive(false);
            lastHoverId = null;
        }
    }


}
