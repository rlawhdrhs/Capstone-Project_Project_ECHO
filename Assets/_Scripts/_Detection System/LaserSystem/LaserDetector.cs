using UnityEngine;

public class LaserDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 10f;
    public float detectAngle = 25f;

    [Header("Laser Height")]
    public float originHeight = 0.3f;
    public float heightRange = 0.15f;

    [Header("References")]
    public PlayerDetectable player;
    public VisionConeMesh visionCone;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.R;

    private bool isLaserOn = false;
    private bool wasDetectedLastFrame = false;


    void Start()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerDetectable>();
        }

        if (visionCone != null)
        {
            visionCone.gameObject.SetActive(false);
            visionCone.SetCone(detectAngle, detectDistance, originHeight, heightRange * 2f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isLaserOn = !isLaserOn;
            Debug.Log($"{name} Laser Toggle: {isLaserOn}");
            
            if (visionCone != null)
            {
                visionCone.gameObject.SetActive(isLaserOn);
            }
        }

        float moveSpeed = 1.0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            originHeight += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            originHeight -= moveSpeed * Time.deltaTime;
        }

        if (!isLaserOn)
        {
            if (player != null)
            {
                player.SetDetected(false);
                player.UpdateGauge(false);
            }

            wasDetectedLastFrame = false;
            return;
        }

        if (visionCone != null)
        {
            visionCone.SetCone(detectAngle, detectDistance, originHeight, heightRange * 2f);
        }

        Transform detectedPoint;
        bool isDetected = CheckPlayerDetected(out detectedPoint);

        if (isDetected && !wasDetectedLastFrame)
        {
            Debug.Log($"[LaserDetector] 감지 시작! Player: {player.gameObject.name}, Point: {detectedPoint.name}");
        }
        
        if (!isDetected && wasDetectedLastFrame)
        {
            Debug.Log("[LaserDetector] 감지 해제!");
        }

        wasDetectedLastFrame = isDetected;

        if (player != null)
        {
            player.SetDetected(isDetected);
            player.UpdateGauge(isDetected);
        }
    }

    bool CheckPlayerDetected(out Transform detectedPoint)
    {
        detectedPoint = null;

        if (player == null || player.isRemoved) return false;

        Transform[] detectPoints = player.DetectPoints;
        if (detectPoints == null || detectPoints.Length == 0) return false;

        Vector3 origin = transform.position + Vector3.up * originHeight;
        Vector3 forwardDir = Camera.main.transform.forward;

        foreach (Transform point in detectPoints)
        {
            if (point == null) continue;

            float heightDifference = Mathf.Abs(point.position.y - origin.y);
            if (heightDifference > heightRange)
                continue;

            Vector3 toTarget = point.position - origin;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget > detectDistance)
                continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(forwardDir, dirToTarget);

            if (angle > detectAngle * 0.5f)
                continue;

            RaycastHit hit;
            if (Physics.Raycast(origin, dirToTarget, out hit, distanceToTarget))
            {
                if (hit.transform == point || hit.transform.IsChildOf(player.transform))
                {
                    detectedPoint = point;
                    return true;
                }
            }
        }

        return false;
    }

    public void SetLaserActiveByControl(bool value)
    {
        isLaserOn = false;
        wasDetectedLastFrame = false;

        if (visionCone != null)
            visionCone.gameObject.SetActive(false);

        if (player != null)
        {
            player.SetDetected(false);
            player.UpdateGauge(false);
        }

        enabled = value;
    }
}