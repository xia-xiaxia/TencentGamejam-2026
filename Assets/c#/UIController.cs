using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    [Header("目标 UI")]
    public RectTransform targetRect; // 拖入你的 MainPanel

    [Header("缩放参数")]
    public float zoomSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 3.0f;

    private bool isDragging = false;
    private Vector2 lastMousePosition;

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    // 1. 缩放逻辑：以鼠标指针为中心
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 记录缩放前的鼠标相对坐标
            Vector3 mouseWorldPosBefore = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, mouseWorldPosBefore, null, out Vector2 localPointBefore);

            // 执行缩放
            Vector3 newScale = targetRect.localScale + Vector3.one * scroll * zoomSpeed;
            newScale.x = Mathf.Clamp(newScale.x, minScale, maxScale);
            newScale.y = Mathf.Clamp(newScale.y, minScale, maxScale);
            targetRect.localScale = newScale;

            // 补偿位移：保持鼠标指向的点位置不变
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, mouseWorldPosBefore, null, out Vector2 localPointAfter);
            Vector3 shift = (localPointAfter - localPointBefore) * targetRect.localScale.x;
            targetRect.localPosition += shift;
        }
    }

    // 2. 拖动逻辑：左键长按拖拽
    private void HandleDrag()
    {
        // 只有点击在 UI 上或正在拖拽时才处理
        if (Input.GetMouseButtonDown(0))
        {
            // 检查鼠标是否点在 targetRect (背景) 上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 diff = currentMousePosition - lastMousePosition;

            // 移动面板
            targetRect.position += (Vector3)diff;
            lastMousePosition = currentMousePosition;
        }
    }
}