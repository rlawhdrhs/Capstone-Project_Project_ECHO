using UnityEngine;

public class LaserDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 20f;
    public float detectAngle = 20f;

    [Header("VR / Camera Origin")]
    [Tooltip("VR에서는 CenterEyeAnchor 또는 HMD Camera Transform을 연결하세요.")]
    public Transform laserOrigin;

    [Header("Height Limit")]
    public bool useHeightLimit = false;
    public float heightRange = 0.7f;

    [Header("Target")]
    public PlayerDetectable player;

    [Header("Obstacle")]
    [Tooltip("Wall, Obstacle 등 레이저를 막는 레이어만 넣으세요. Player 레이어는 넣지 마세요.")]
    public LayerMask obstacleMask;

    [Header("Input Test")]
    [Tooltip("네트워크 적용 시 false로 두고, 네트워크/VR 입력 스크립트에서 SetLaserActive()를 호출하세요.")]
    public bool useKeyboardInput = true;
    public KeyCode toggleKey = KeyCode.R;

    [Header("Debug")]
    public bool showDebugRay = true;
    public bool showAngleGuide = true;

    private bool isLaserOn = false;
    private bool wasDetectedLastFrame = false;

    private PlayerDetectable currentDetectedTarget;
    private Transform currentDetectedPoint;

    public bool IsLaserOn => isLaserOn;
    public PlayerDetectable CurrentDetectedTarget => currentDetectedTarget;
    public Transform CurrentDetectedPoint => currentDetectedPoint;

    private void Start()
    {
        if (laserOrigin == null)
        {
            Transform foundCameraPoint = transform.Find("CameraPoint");

            if (foundCameraPoint != null)
            {
                laserOrigin = foundCameraPoint;
            }
            else
            {
                laserOrigin = transform;
                Debug.LogWarning($"{name}: Laser Origin이 비어 있어서 자기 transform을 사용합니다. VR에서는 CenterEyeAnchor 또는 CameraPoint를 연결하세요.");
            }
        }

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerDetectable>();
        }
    }

    private void Update()
    {
        if (useKeyboardInput)
        {
            HandleKeyboardInput();
        }

        if (!isLaserOn)
        {
            ClearDetectionState();
            return;
        }

        bool isDetected = CheckPlayerDetected(out PlayerDetectable detectedTarget, out Transform detectedPoint);

        currentDetectedTarget = detectedTarget;
        currentDetectedPoint = detectedPoint;

        if (isDetected && !wasDetectedLastFrame)
        {
            Debug.Log($"[LaserDetector] 감지 시작! Player: {detectedTarget.gameObject.name}, Point: {detectedPoint.name}");
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

        if (showDebugRay)
        {
            DrawDebugLaser(isDetected);
        }
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetLaserActive(!isLaserOn);
        }
    }

    public void SetLaserActive(bool value)
    {
        isLaserOn = value;
        wasDetectedLastFrame = false;

        if (!isLaserOn)
        {
            ClearDetectionState();
        }

        Debug.Log($"{name} Laser Active: {isLaserOn}");
    }

    public void SetLaserActiveByControl(bool value)
    {
        SetLaserActive(false);
        enabled = value;
    }

    public bool TryEliminateCurrentTarget()
    {
        if (currentDetectedTarget == null)
        {
            Debug.Log("[LaserDetector] 제거할 대상 없음");
            return false;
        }

        if (!currentDetectedTarget.isRemovable)
        {
            Debug.Log("[LaserDetector] 대상이 아직 제거 가능 상태가 아님");
            return false;
        }

        currentDetectedTarget.TryRemove();
        return true;
    }

    private void ClearDetectionState()
    {
        currentDetectedTarget = null;
        currentDetectedPoint = null;

        if (player != null)
        {
            player.SetDetected(false);
            player.UpdateGauge(false);
        }

        wasDetectedLastFrame = false;
    }

    private bool CheckPlayerDetected(out PlayerDetectable detectedTarget, out Transform detectedPoint)
    {
        detectedTarget = null;
        detectedPoint = null;

        if (player == null || player.isRemoved)
            return false;

        Transform[] detectPoints = player.DetectPoints;

        if (detectPoints == null || detectPoints.Length == 0)
            return false;

        Ray aimRay = GetAimRay();

        Vector3 origin = aimRay.origin;
        Vector3 forwardDir = aimRay.direction;

        foreach (Transform point in detectPoints)
        {
            if (point == null)
                continue;

            if (useHeightLimit)
            {
                float heightDifference = Mathf.Abs(point.position.y - origin.y);

                if (heightDifference > heightRange)
                    continue;
            }

            Vector3 toTarget = point.position - origin;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget > detectDistance)
                continue;

            Vector3 dirToTarget = toTarget.normalized;
            float angle = Vector3.Angle(forwardDir, dirToTarget);

            if (angle > detectAngle * 0.5f)
                continue;

            bool isBlocked = Physics.Linecast(origin, point.position, obstacleMask);

            if (isBlocked)
                continue;

            detectedTarget = player;
            detectedPoint = point;
            return true;
        }

        return false;
    }

    private Ray GetAimRay()
    {
        if (laserOrigin != null)
        {
            return new Ray(laserOrigin.position, laserOrigin.forward);
        }

        return new Ray(transform.position, transform.forward);
    }

    private void DrawDebugLaser(bool isDetected)
    {
        Ray aimRay = GetAimRay();

        Debug.DrawRay(
            aimRay.origin,
            aimRay.direction * detectDistance,
            isDetected ? Color.red : Color.green
        );

        if (!showAngleGuide)
            return;

        Vector3 leftDir = Quaternion.AngleAxis(-detectAngle * 0.5f, Vector3.up) * aimRay.direction;
        Vector3 rightDir = Quaternion.AngleAxis(detectAngle * 0.5f, Vector3.up) * aimRay.direction;

        Debug.DrawRay(aimRay.origin, leftDir * detectDistance, Color.yellow);
        Debug.DrawRay(aimRay.origin, rightDir * detectDistance, Color.yellow);
    }
}