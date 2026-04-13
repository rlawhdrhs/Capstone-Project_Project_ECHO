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
            return;
        }

        if (visionCone != null)
        {
            visionCone.SetCone(detectAngle, detectDistance, originHeight, heightRange * 2f);
        }

        bool isDetected = CheckPlayerDetected();

        if (player != null)
        {
            player.SetDetected(isDetected);
            player.UpdateGauge(isDetected);
        }
    }

    bool CheckPlayerDetected()
    {
        if (player == null || player.isRemoved) return false;

        Transform[] detectPoints = player.DetectPoints;
        if (detectPoints == null || detectPoints.Length == 0) return false;

        Vector3 origin = transform.position + Vector3.up * originHeight;
        Vector3 forwardDir = transform.forward;

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
                    Debug.Log($"[LaserDetector] 감지됨! Player: {player.gameObject.name}, Point: {point.name}");
                    return true;
                }
            }
        }

        return false;
    }
}