using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class EventTreeUI : MonoBehaviour
{
    [Header("=== 面板设置 ===")]
    public GameObject treePanel;       // 事件树
    public GameObject detailPanel;      // 详情框
    public Text detailText;            // 详情文字

    [Header("=== 节点配置  ===")]
    public EventNode[] eventNodes;

    public Dictionary<string, EventNode> nodeDict;

    [Header("=== 连线配置 ===")]
    public EventLine[] eventLines;

    // 当前进度（字符串ID）
    private string currentEventId = "0";
    private string lastClickId = "";

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

        // 给所有节点加点击（只绑定一次）
        foreach (var node in eventNodes)
        {
            if (node == null || node.nodeObj == null) continue;

            var btn = node.nodeObj.GetComponent<Button>();
            if (btn == null) btn = node.nodeObj.AddComponent<Button>();

            string clickId = node.nodeId;
            // 防止重复绑定（在编辑器重复进入 Play 时可能重复）
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnNodeClicked(clickId));
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
            if (nodeDict == null) nodeDict = new Dictionary<string, EventNode>(System.StringComparer.Ordinal);
            nodeDict[node.nodeId] = node;
        }
        // 再次保证初始显示正确（BackBoard 可能在 Start 里触发第一次 OnNodeChanged）
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
            UpdateUnlockedFromBackboard();
        }
        if(nodeDict.ContainsKey(node.id)) {
            nodeDict[node.id].detailDesc = node.Text; // 自动同步文本
        }
        lastClickId = "";
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
        return id == "0";
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

        // 若点击的不是当前节点，则尝试通过 BackBoard 进入该节点（会触发 OnNodeChanged）
        if (BackBoard.Instance != null && id != currentEventId)
        {
            bool entered = BackBoard.Instance.EnterNode(id);
            if (entered)
            {
                // EnterNode 会触发 HandleNodeChanged 并刷新 UI；直接返回以避免重复处理
                return;
            }
        }

        // 显示详情：优先从 BackBoard 的事件表获取文本（自动同步），不存在则使用 inspector 的 detailDesc
        string displayText = node.detailDesc ?? string.Empty;
        if (detailText != null) detailText.text = displayText;
        if (detailPanel != null) detailPanel.SetActive(true);
        lastClickId = id;
    }
}