using UnityEngine;

public class LaserDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 10f;
    public float detectAngle = 25f;

    [Header("References")]
    public PlayerDetectable player;
    public VisionConeMesh visionCone;

    [Header("Layer Mask")]
    public LayerMask obstacleLayer;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.R;

    private bool isLaserOn = false;

    void Start()
    {
        if (visionCone != null)
        {
            visionCone.gameObject.SetActive(false);
            visionCone.SetCone(detectAngle, detectDistance);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isLaserOn = !isLaserOn;

            if (visionCone != null)
            {
                visionCone.gameObject.SetActive(isLaserOn);
            }
        }

        if (!isLaserOn)
        {
            if (player != null)
            {
                player.SetDetected(false);
                player.UpdateGauge(false);
            }
            return;
        }

        if (visionCone != null)
        {
            visionCone.SetCone(detectAngle, detectDistance);
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forwardDir = transform.forward;

        bool isDetected = false;

        if (player != null && !player.isRemoved)
        {
            Vector3 targetPos = player.transform.position;
            Vector3 toTarget = targetPos - origin;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget <= detectDistance)
            {
                Vector3 dirToTarget = toTarget.normalized;
                float angle = Vector3.Angle(forwardDir, dirToTarget);

                if (angle <= detectAngle * 0.5f)
                {
                    RaycastHit hit;

                    if (!Physics.Raycast(origin, dirToTarget, out hit, distanceToTarget, obstacleLayer))
                    {
                        isDetected = true;
                    }
                }
            }
        }

        if (player != null)
        {
            player.SetDetected(isDetected);
            player.UpdateGauge(isDetected);
        }
    }
}