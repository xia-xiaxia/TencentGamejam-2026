using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 附加到每个节点对象上，转发鼠标指针进入/离开事件到 EventTreeUI。
/// </summary>
public class NodeHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string nodeId;
    public EventTreeUI owner;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (owner != null)
            owner.OnNodeHoverEnter(nodeId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (owner != null)
            owner.OnNodeHoverExit(nodeId);
    }
}