using Fusion;
using UnityEngine;

public class LaserDetector_Network : NetworkBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 20f;
    public float detectAngle = 20f;

    [Header("VR / Camera Origin")]
    public Transform laserOrigin;

    public bool isSensorRobot = false;
    [Header("Height Limit")]
    public bool useHeightLimit = false;
    public float heightRange = 0.7f;

    [Header("Obstacle")]
    public LayerMask obstacleMask;

    [Networked] public NetworkBool isDetectorActive { get; set; }
    [Networked] public NetworkBool prevSpace { get; set; }

    [Header("Debug")]
    public bool showDebugRay = true;
    public bool showAngleGuide = true;

    private PlayerDetectable_Network targetPlayer;

    public override void Spawned()
    {
        if (laserOrigin == null)
        {
            Transform foundCameraPoint = transform.Find("CameraPoint");
            laserOrigin = foundCameraPoint != null ? foundCameraPoint : transform;
        }

        if (Object.HasStateAuthority)
        {
            isDetectorActive = !isSensorRobot;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isDetectorActive) return;
        // 1. 입력 처리 (제거 버튼만 남김)
        if (GetInput(out NetworkInputData data))
        {
            bool isSpacePressedThisFrame = data.keySpace && !prevSpace;
            if (isSpacePressedThisFrame && Object.HasStateAuthority)
            {
                if (targetPlayer != null && targetPlayer.isRemovable)
                {
                    targetPlayer.RequestRemove();
                }
            }
            prevSpace = data.keySpace;
        }

        // 2. 잠입자 타겟 런타임 캐싱
        if (targetPlayer == null && NetworkManager.Instance.InfiltratorObject != null)
        {
            targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
        }

        // 3. 레이저 판정 로직
        if (!Object.HasStateAuthority) return;

        if (targetPlayer != null && !targetPlayer.isRemoved)
        {
            if (CheckPlayerDetected())
            {
                targetPlayer.NotifyDetected();
            }
        }
    }

    private bool CheckPlayerDetected()
    {
        Transform[] detectPoints = targetPlayer.DetectPoints;
        if (detectPoints == null || detectPoints.Length == 0) return false;

        Ray aimRay = GetAimRay();
        Vector3 origin = aimRay.origin;
        Vector3 forwardDir = aimRay.direction;

        foreach (Transform point in detectPoints)
        {
            if (point == null) continue;

            if (useHeightLimit)
            {
                float heightDifference = Mathf.Abs(point.position.y - origin.y);
                if (heightDifference > heightRange) continue;
            }

            Vector3 toTarget = point.position - origin;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget > detectDistance) continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(forwardDir, dirToTarget);

            if (angle > detectAngle * 0.5f) continue;

            bool isBlocked = Physics.Linecast(origin, point.position, obstacleMask);
            if (isBlocked) continue;

            return true;
        }

        return false;
    }

    private Ray GetAimRay()
    {
        return laserOrigin != null ? new Ray(laserOrigin.position, laserOrigin.forward)
                                   : new Ray(transform.position, transform.forward);
    }

    public override void Render()
    {
        if (showDebugRay && isDetectorActive)
        {
            Ray aimRay = GetAimRay();
            Debug.DrawRay(aimRay.origin, aimRay.direction * detectDistance, Color.red);

            if (showAngleGuide)
            {
                Vector3 leftDir = Quaternion.AngleAxis(-detectAngle * 0.5f, Vector3.up) * aimRay.direction;
                Vector3 rightDir = Quaternion.AngleAxis(detectAngle * 0.5f, Vector3.up) * aimRay.direction;
                Debug.DrawRay(aimRay.origin, leftDir * detectDistance, Color.yellow);
                Debug.DrawRay(aimRay.origin, rightDir * detectDistance, Color.yellow);
            }
        }
    }
}