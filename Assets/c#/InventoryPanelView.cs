using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InventoryPanelView : MonoBehaviour
{
    public Transform contentParent; // 物品列表的父物体
    public GameObject itemPrefab;   // 单个物品的预制体（包含一个 Text）

    public void Initialize()
    {
        BindBackBoardEvents();
        RefreshInventory();
    }

    private void OnEnable()
    {
        BindBackBoardEvents();
        RefreshInventory();
    }

    private void OnDisable()
    {
        UnbindBackBoardEvents();
    }

    private void RefreshInventory()
    {
        if (contentParent == null)
        {
            return;
        }

        foreach (Transform t in contentParent)
        {
            Destroy(t.gameObject);
        }

        if (itemPrefab == null || BackBoard.Instance == null)
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
            if (item == null)
            {
                continue;
            }

            GameObject go = Instantiate(itemPrefab, contentParent);
            TextMeshProUGUI text = go != null ? go.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (text != null)
            {
                text.text = string.IsNullOrEmpty(item.name) ? item.id : item.name;
            }
        }
    }

    private void BindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        BackBoard.Instance.OnBlackboardValueChanged += HandleBlackboardValueChanged;
        BackBoard.Instance.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
        BackBoard.Instance.OnCurrentCharacterChanged += HandleCurrentCharacterChanged;
    }

    private void UnbindBackBoardEvents()
    {
        if (BackBoard.Instance == null)
        {
            return;
        }

        BackBoard.Instance.OnBlackboardValueChanged -= HandleBlackboardValueChanged;
        BackBoard.Instance.OnCurrentCharacterChanged -= HandleCurrentCharacterChanged;
    }

    private void HandleBlackboardValueChanged(string key)
    {
        if (!string.IsNullOrEmpty(key) && key.StartsWith("bag:"))
        {
            RefreshInventory();
        }
    }

    private void HandleCurrentCharacterChanged(CharacterData _)
    {
        RefreshInventory();
    }
}