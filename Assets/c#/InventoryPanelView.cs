using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryPanelView : MonoBehaviour
{
    public Transform contentParent; // 物品列表的父物体
    public GameObject itemPrefab;   // 单个物品的预制体（包含一个 Text）

    public void Initialize(InventoryModel model)
    {
        model.OnItemsChanged += RefreshInventory;
        RefreshInventory(model.ItemNames);
    }

    private void RefreshInventory(List<string> items)
    {
        // 1. 清空旧列表
        foreach (Transform t in contentParent) Destroy(t.gameObject);

        // 2. 生成新物品
        foreach (var itemName in items)
        {
            var go = Instantiate(itemPrefab, contentParent);
            go.GetComponentInChildren<TextMeshProUGUI>().text = itemName;
        }
    }
}