using UnityEngine;

public class ChaserVision : MonoBehaviour
{
    public Camera chaserCamera;
    public float viewDistance = 50f;

    void Update()
    {
        DetectInfiltrators();
    }

    void DetectInfiltrators()
    {
        Infiltrator[] targets = FindObjectsOfType<Infiltrator>();

        foreach (var target in targets)
        {
            bool detected = IsVisible(target);
            target.SetDetected(detected);
        }
    }

    bool IsVisible(Infiltrator target)
    {
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
            if (hit.transform == target.transform ||
                hit.transform.IsChildOf(target.transform))
            {
                return true;
            }
        }

        return false;
    }
}