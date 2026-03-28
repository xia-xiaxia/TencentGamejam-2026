using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasShow : MonoBehaviour
{
    [Header("要切换的 Canvas（可为 Canvas 或 UI 根 GameObject）")]
    public GameObject canvasA;
    public GameObject canvasB;

    [Header("启动时显示哪一个（如果两个都为空则不做操作）")]
    public bool startShowA = true;
    private void Awake()
    {
        // 启动时根据配置设置可见性
        if (canvasA == null && canvasB == null) return;

        if (canvasA != null) canvasA.SetActive(startShowA);
        if (canvasB != null) canvasB.SetActive(!startShowA);
    }
    public void ShowA()
    {
        if (canvasA != null) canvasA.SetActive(true);
        if (canvasB != null) canvasB.SetActive(false);
    }
    public void ShowB()
    {
        if (canvasB != null) canvasB.SetActive(true);
        if (canvasA != null) canvasA.SetActive(false);
    }
}
