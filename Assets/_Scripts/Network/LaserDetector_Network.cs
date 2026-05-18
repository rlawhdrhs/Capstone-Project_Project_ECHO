using Fusion;
using UnityEngine;

public class LaserDetector_Network : NetworkBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 20f;
    public float detectAngle = 20f;

    [Header("VR / Camera Origin")]
    [Tooltip("VR에서는 CenterEyeAnchor 또는 로봇의 CameraPoint Transform을 연결하세요.")]
    public Transform laserOrigin;

    [Header("Height Limit")]
    public bool useHeightLimit = false;
    public float heightRange = 0.7f;

    [Header("Obstacle")]
    [Tooltip("Wall, Obstacle 등 레이저를 막는 레이어만 넣으세요. Player 레이어는 절대 넣지 마세요!")]
    public LayerMask obstacleMask;
    public bool isControlledByMe = false;

    // 퓨전 네트워크 동기화 변수
    [Networked] public NetworkBool isLaserOn { get; set; }
    [Networked] public NetworkBool prevRightTrigger { get; set; }
    [Networked] public NetworkBool prevSpace { get; set; }

    [Header("Debug")]
    public bool showDebugRay = true;
    public bool showAngleGuide = true;

    // 타겟 캐싱
    private PlayerDetectable_Network targetPlayer;

    public override void Spawned()
    {
        if (laserOrigin == null)
        {
            Transform foundCameraPoint = transform.Find("CameraPoint");
            laserOrigin = foundCameraPoint != null ? foundCameraPoint : transform;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 1. 입력 처리 (빙의한 플레이어의 입력)
        if (GetInput(out NetworkInputData data))
        {
            bool isTriggerPressedThisFrame = data.rightTrigger && !prevRightTrigger;
            if (isTriggerPressedThisFrame && Object.HasStateAuthority)
            {
                isLaserOn = !isLaserOn;
            }
            prevRightTrigger = data.rightTrigger;

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

        // 2. 잠입자 타겟 런타임 캐싱 (네트워크 환경용)
        if (targetPlayer == null && NetworkManager.Instance.InfiltratorObject != null)
        {
            targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
        }

        // 3. 레이저 판정 로직 (오직 호스트/서버만 연산)
        if (!Object.HasStateAuthority) return;

        if (isLaserOn && targetPlayer != null && !targetPlayer.isRemoved)
        {
            if (CheckPlayerDetected())
            {
                targetPlayer.NotifyDetected();
            }
        }
    }

    // 팀원이 작성한 Raycast 탐지 로직 이식
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

            // 1. 높이 제한 확인
            if (useHeightLimit)
            {
                float heightDifference = Mathf.Abs(point.position.y - origin.y);
                if (heightDifference > heightRange) continue;
            }

            Vector3 toTarget = point.position - origin;
            float distanceToTarget = toTarget.magnitude;

            // 2. 거리 확인
            if (distanceToTarget > detectDistance) continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(forwardDir, dirToTarget);

            // 3. 시야각 확인
            if (angle > detectAngle * 0.5f) continue;

            // 4. 장애물(벽)에 가려졌는지 확인
            bool isBlocked = Physics.Linecast(origin, point.position, obstacleMask);
            if (isBlocked) continue;

            // 모든 조건을 통과하면 감지 성공!
            return true;
        }

        return false;
    }

    private Ray GetAimRay()
    {
        return laserOrigin != null ? new Ray(laserOrigin.position, laserOrigin.forward)
                                   : new Ray(transform.position, transform.forward);
    }

    // 디버그 레이는 클라이언트에서도 볼 수 있게 Render에서 처리
    public override void Render()
    {
        if (showDebugRay && isLaserOn)
        {
            Ray aimRay = GetAimRay();
            // 호스트에서는 isDetected 여부를 정확히 알지만, 클라이언트에서는 레이저 On 상태만 표시
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