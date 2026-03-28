using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包视图：
/// - 在 Inspector 配置所有可能的物品预制体（id 对应）
/// - 监听 BackBoard 的黑板事件（可配置前缀）来添加 / 移除物品
/// - 以固定间距排列：一行 5 个，最多 15 个（最多 3 行）
/// - 支持 GridLayoutGroup（优先）或无布局时的简单网格定位回退
/// </summary>
public class BagUI : MonoBehaviour
{
    [Serializable]
    public class ItemEntry
    {
        public string id;           // 物品唯一 id（用于添加/移除）
        public GameObject prefab;   // 对应的按钮预制体（应包含 Button + Image/Text）
    }

    public List<ItemEntry> itemPrefabs = new List<ItemEntry>();

    [Header("容器与布局")]
    public RectTransform contentParent;      // 按钮容器（推荐添加 GridLayoutGroup）
    public Vector2 cellSize = new Vector2(100f, 100f);    // 回退布局时单元大小
    public Vector2 spacing = new Vector2(8f, 8f);         // 回退布局时间距

    [Header("行为设置")]
    public int itemsPerRow = 5;      // 每行数量（固定为 5）
    public int maxItems = 15;        // 最大物品数量（固定为 15）
    public string bagRefreshPrefix = "bag:";  // 全量刷新事件前缀，格式例："bag:CHARACTER_ID"
    public string addPrefix = "bag.add:";     // 黑板事件 add 前缀，格式例： "bag.add:ITEM_ID"
    public string removePrefix = "bag.remove:"; // 黑板事件 remove 前缀，格式例： "bag.remove:ITEM_ID"

    [Header("显示控制")]
    public GameObject bagRoot;       // 整个背包面板的根（可在 Inspector 指定）；若为空则使用本对象
    public bool startVisible = false; // 启动时是否显示背包

    // 运行时数据
    private readonly Dictionary<string, GameObject> spawned = new Dictionary<string, GameObject>(StringComparer.Ordinal);
    private GridLayoutGroup gridLayout;
    private bool isOpen;

    private void Awake()
    {
        if (contentParent != null)
            gridLayout = contentParent.GetComponent<GridLayoutGroup>();

        if (bagRoot == null)
            bagRoot = this.gameObject;

        isOpen = startVisible;
        if (bagRoot != null)
            bagRoot.SetActive(startVisible);
    }

    private void OnEnable()
    {
        // 订阅 BackBoard 的黑板变更事件（如果存在）
        if (BackBoard.Instance != null)
        {
            BackBoard.Instance.OnBlackboardValueChanged += HandleBlackboardChanged;
        }

        RefreshFromCurrentBag();
    }

    private void OnDisable()
    {
        if (BackBoard.Instance != null)
        {
            BackBoard.Instance.OnBlackboardValueChanged -= HandleBlackboardChanged;
        }
    }

    private void OnDestroy()
    {
        // 清理所有生成物
        ClearAll();
    }

    private void HandleBlackboardChanged(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        // full refresh
        if (!string.IsNullOrEmpty(bagRefreshPrefix) && key.StartsWith(bagRefreshPrefix, StringComparison.Ordinal))
        {
            RefreshFromCurrentBag();
            return;
        }

        // add
        if (!string.IsNullOrEmpty(addPrefix) && key.StartsWith(addPrefix, StringComparison.Ordinal))
        {
            string id = key.Substring(addPrefix.Length);
            if (!string.IsNullOrEmpty(id))
                AddItemById(id);
            return;
        }

        // remove
        if (!string.IsNullOrEmpty(removePrefix) && key.StartsWith(removePrefix, StringComparison.Ordinal))
        {
            string id = key.Substring(removePrefix.Length);
            if (!string.IsNullOrEmpty(id))
                RemoveItemById(id);
            return;
        }
    }

    private void RefreshFromCurrentBag()
    {
        if (contentParent == null)
        {
            return;
        }

        ClearAll();

        if (BackBoard.Instance == null)
        {
            return;
        }

        CharacterData character = BackBoard.Instance.GetCurrentCharacter();
        if (character == null || character.bag == null)
        {
            return;
        }

        for (int i = 0; i < character.bag.Count; i++)
        {
            ItemData item = character.bag[i];
            if (item == null || string.IsNullOrEmpty(item.id))
            {
                continue;
            }

            AddItemById(item.id);
        }
    }

    /// <summary>
    /// 尝试添加物品（由 id 指定）。若已存在或已达上限则忽略。
    /// </summary>
    public bool AddItemById(string id)
    {
        if (string.IsNullOrEmpty(id) || contentParent == null) return false;
        if (spawned.ContainsKey(id)) return false;
        if (spawned.Count >= Math.Min(maxItems, 15)) return false; // 强制不超过 15

        ItemEntry entry = itemPrefabs.Find(e => string.Equals(e.id, id, StringComparison.Ordinal));
        if (entry == null || entry.prefab == null)
        {
            Debug.LogWarning($"BagUI: 未找到 id={id} 的物品预制体。", this);
            return false;
        }

        GameObject go = Instantiate(entry.prefab, contentParent);
        if (go == null) return false;

        go.transform.localScale = Vector3.one;
        spawned[id] = go;

        // 若没有 GridLayoutGroup，则手动定位
        if (gridLayout == null)
        {
            LayoutManual();
        }

        return true;
    }

    /// <summary>
    /// 移除物品
    /// </summary>
    public bool RemoveItemById(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        GameObject go;
        if (!spawned.TryGetValue(id, out go) || go == null)
        {
            return false;
        }

        spawned.Remove(id);
        Destroy(go);

        // 重新布局
        if (gridLayout == null)
        {
            LayoutManual();
        }

        return true;
    }

    /// <summary>
    /// 清空背包
    /// </summary>
    public void ClearAll()
    {
        foreach (var kv in spawned)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        spawned.Clear();
    }

    /// <summary>
    /// 回退的手动网格布局（当容器没有 GridLayoutGroup 时使用）。
    /// 以 top-left 为起点向右、向下排列，每行 itemsPerRow 个。
    /// </summary>
    private void LayoutManual()
    {
        if (contentParent == null) return;

        int index = 0;
        foreach (Transform child in contentParent)
        {
            // 仅对主动生成的子对象进行定位；跳过非实例化项（若有）
            if (child == null) continue;
            if (index >= 15) // 强制上限
            {
                // 超出上限的直接隐藏或销毁 — 这里选择隐藏
                child.gameObject.SetActive(false);
                index++;
                continue;
            }

            int col = index % itemsPerRow;
            int row = index / itemsPerRow;

            // 计算位置（锚点与 pivot 可能影响表现，假定 contentParent pivot 在左上）
            float x = col * (cellSize.x + spacing.x);
            float y = -row * (cellSize.y + spacing.y);

            RectTransform rt = child as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.anchoredPosition = new Vector2(x, y);
                rt.sizeDelta = cellSize;
                child.gameObject.SetActive(true);
            }

            index++;
        }
    }

    /// <summary>
    /// 外部接口：返回当前已显示的物品 id 列表（按创建顺序）。
    /// </summary>
    public List<string> GetCurrentItemIds()
    {
        return new List<string>(spawned.Keys);
    }

    // ========== 显示控制方法（供按钮 OnClick 指向） ==========

    /// <summary>
    /// 显示背包面板（供按钮或脚本调用）。
    /// </summary>
    public void Show()
    {
        if (bagRoot == null) return;
        if (isOpen) return;
        bagRoot.SetActive(true);
        isOpen = true;

        // 显示时确保布局正确
        if (gridLayout == null)
            LayoutManual();
    }

    /// <summary>
    /// 隐藏背包面板（供按钮或脚本调用）。
    /// </summary>
    public void Hide()
    {
        if (bagRoot == null) return;
        if (!isOpen) return;
        bagRoot.SetActive(false);
        isOpen = false;
    }

    /// <summary>
    /// 切换显示状态（可选）。
    /// </summary>
    public void Toggle()
    {
        if (isOpen) Hide(); else Show();
    }
}