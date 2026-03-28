using UnityEngine;
using UnityEngine.UI;

public class EventTreeUI : MonoBehaviour
{
    [Header("=== 面板设置 ===")]
    public GameObject treePanel;       // 事件树
    public GameObject detailPanel;      // 详情框
    public Text detailText;            // 详情文字

    [Header("=== 节点配置  ===")]
    public EventNode[] eventNodes;

    [Header("=== 连线配置 ===")]
    public EventLine[] eventLines;

    // 当前进度（字符串ID）
    private string currentEventId = "0";
    private string lastClickId = "";

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
        detailPanel.SetActive(false);


        // 给所有节点加点击
        foreach (var node in eventNodes)
        {
            var btn = node.nodeObj.GetComponent<Button>();
            if (btn == null) btn = node.nodeObj.AddComponent<Button>();

            string clickId = node.nodeId;
            btn.onClick.AddListener(() => OnNodeClicked(clickId));
        }
    }

    void Start()
    {
        RefreshTreeUI();
    }
    // 外部调用：更新进度

    public void SetProgress(string newId)
    {
        currentEventId = newId;
        lastClickId = "";
        detailPanel.SetActive(false);
        RefreshTreeUI();
    }


    // 刷新UI状态

    void RefreshTreeUI()
    {
        foreach (var node in eventNodes)
        {
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
            bool fromOk = IsUnlocked(line.fromNodeId);
            bool toOk = IsUnlocked(line.toNodeId);
            line.lineObj.SetActive(fromOk && toOk);
        }
    }

    // 判断节点是否解锁（简单规则：等于当前ID 或 是前置节点）
    bool IsUnlocked(string id)
    {
        if (id == currentEventId) return true;
        if (currentEventId == "A1" || currentEventId == "A2") return id == "A0";
        if (currentEventId == "B1" || currentEventId == "B2") return id == "A0";
        return id == "A0";
    }


    void OnNodeClicked(string id)
    {
        var node = System.Array.Find(eventNodes, n => n.nodeId == id);
        if (node == null) return;

        // 重复点击 → 关闭
        if (lastClickId == id && detailPanel.activeSelf)
        {
            detailPanel.SetActive(false);
            lastClickId = "";
            return;
        }

        // 显示详情
        detailText.text = node.detailDesc;
        detailPanel.SetActive(true);
        lastClickId = id;
    }
}