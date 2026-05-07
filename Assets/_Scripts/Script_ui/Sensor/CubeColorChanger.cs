using UnityEngine;

public class CubeColorChanger : MonoBehaviour
{
    [Header("색 바꿀 Renderer")]
    public Renderer targetRenderer;

    [Header("기본 색")]
    public Color normalColor = Color.white;

    [Header("감지 색")]
    public Color detectedColor = Color.red;

    void Start()
    {
        ChangeToNormalColor();
    }

    public void ChangeToDetectedColor()
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = detectedColor;
        }
    }

    public void ChangeToNormalColor()
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = normalColor;
        }
    }
}