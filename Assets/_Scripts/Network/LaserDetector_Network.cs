using Fusion;
using UnityEngine;

public class LaserDetector_Network : NetworkBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 10f;
    public float detectAngle = 25f;

    [Header("References")]
    public PlayerDetectable_Network targetPlayer;
    public VisionConeMesh visionCone;

    [Header("Layer Mask")]
    public LayerMask obstacleLayer;

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.R;

    [Networked] public NetworkBool isLaserOn { get; set; }
    private bool lastDetectedState = false;

    public override void Spawned()
    {
        visionCone = GetComponentInChildren<VisionConeMesh>();
    }

    void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(toggleKey))
        {
            RPC_ToggleLaser();
        }

        if (visionCone != null)
        {
            visionCone.gameObject.SetActive(isLaserOn);
            if (isLaserOn) visionCone.SetCone(detectAngle, detectDistance);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ToggleLaser()
    {
        isLaserOn = !isLaserOn;
    }

    public override void FixedUpdateNetwork()
    {

        if (targetPlayer == null)
        {
            if (NetworkManager.Instance.InfiltratorObject != null)
                targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
            return;
        }

        bool currentlyDetected = false;

        if (isLaserOn)
        {
            Vector3 origin = transform.position + Vector3.up * 1.2f + transform.forward * 0.5f;
            Vector3 targetPos = targetPlayer.transform.position + Vector3.up * 0.8f;
            Vector3 dir = targetPos - origin;
            float distance = dir.magnitude;

            // 각도/거리 체크
            if (distance <= detectDistance && Vector3.Angle(transform.forward, dir.normalized) <= detectAngle * 0.5f)
            {
                // 장애물 체크 (로그 추가)
                if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, distance, obstacleLayer))
                {
                    // 장애물 이름 확인용 로그
                    Debug.Log($"[감지 실패] {hit.collider.name}가 가로막음");
                }
                else
                {
                    currentlyDetected = true;
                    Debug.Log("[감지 성공] 잠입자가 레이저에 닿음!");
                }
            }
        }

        if (Object.HasStateAuthority)
        {
            targetPlayer.isDetected = currentlyDetected;
        }
    }
}