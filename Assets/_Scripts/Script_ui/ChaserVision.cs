using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChaserVision : MonoBehaviour
{
    public Camera chaserCamera;
    public float viewDistance = 50f;

    [Header("Volume")]
    public Volume chaserGlobalVolume;

    [Header("Color Settings")]
    public Color normalColor = new Color(0.3f, 0.3f, 1f, 1f);   // 평소 파랑
    public Color alertColor = new Color(1f, 0.15f, 0.15f, 1f); // 감지 시 빨강
    public float colorChangeSpeed = 6f;

    private ColorAdjustments colorAdjustments;
    private Color currentColor;

    void Start()
    {
        if (chaserGlobalVolume == null)
        {
            Debug.LogError("ChaserGlobalVolume이 연결되지 않았습니다.");
            return;
        }

        if (!chaserGlobalVolume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("Volume Profile 안에 Color Adjustments가 없습니다.");
            return;
        }

        colorAdjustments.colorFilter.overrideState = true;
        currentColor = normalColor;
        colorAdjustments.colorFilter.value = currentColor;
    }

    void Update()
    {
        bool hasDetectedPlayer = DetectInfiltrators();
        UpdateVolumeColor(hasDetectedPlayer);
    }

    bool DetectInfiltrators()
    {
        Infiltrator[] targets = FindObjectsOfType<Infiltrator>();
        bool hasDetected = false;

        foreach (var target in targets)
        {
            bool detected = IsVisible(target);
            target.SetDetected(detected);

            // 레이어 말고, 지금 프레임 감지 결과로 바로 판정
            if (detected)
            {
                hasDetected = true;
            }
        }

        return hasDetected;
    }

    void UpdateVolumeColor(bool hasDetectedPlayer)
    {
        if (colorAdjustments == null) return;

        Color targetColor = hasDetectedPlayer ? alertColor : normalColor;
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorChangeSpeed);
        colorAdjustments.colorFilter.value = currentColor;
    }

    bool IsVisible(Infiltrator target)
    {
        if (chaserCamera == null) return false;

        Renderer rend = target.GetComponent<Renderer>();
        if (rend == null) return false;

        Vector3 point = rend.bounds.center;
        Vector3 viewPos = chaserCamera.WorldToViewportPoint(point);

        bool inFront = viewPos.z > 0f;
        bool inView =
            viewPos.x >= 0f && viewPos.x <= 1f &&
            viewPos.y >= 0f && viewPos.y <= 1f;

        if (!inFront || !inView) return false;

        Vector3 origin = chaserCamera.transform.position;
        Vector3 dir = (point - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance))
        {
            return hit.transform == target.transform || hit.transform.IsChildOf(target.transform);
        }

        return false;
    }
}


//using UnityEngine;

//public class ChaserVision : MonoBehaviour
//{
//    public Camera chaserCamera;
//    public float viewDistance = 50f;

//    void Update()
//    {
//        DetectInfiltrators();
//    }

//    void DetectInfiltrators()
//    {
//        Infiltrator[] targets = FindObjectsOfType<Infiltrator>();

//        foreach (var target in targets)
//        {
//            bool detected = IsVisible(target);
//            target.SetDetected(detected);
//        }
//    }

//    bool IsVisible(Infiltrator target)
//    {
//        Renderer rend = target.GetComponent<Renderer>();
//        if (rend == null) return false;

//        Vector3 point = rend.bounds.center;
//        Vector3 viewPos = chaserCamera.WorldToViewportPoint(point);

//        bool inFront = viewPos.z > 0f;
//        bool inView =
//            viewPos.x >= 0f && viewPos.x <= 1f &&
//            viewPos.y >= 0f && viewPos.y <= 1f;

//        if (!inFront || !inView) return false;

//        Vector3 origin = chaserCamera.transform.position;
//        Vector3 dir = (point - origin).normalized;

//        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance))
//        {
//            if (hit.transform == target.transform ||
//                hit.transform.IsChildOf(target.transform))
//            {
//                return true;
//            }
//        }

//        return false;
//    }
//}