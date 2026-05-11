using Fusion;
using UnityEngine;

public class LaserDetector_Network : NetworkBehaviour
{
    [Header("Detection Settings")]
    public float detectDistance = 10f;
    public float detectAngle = 25f;
    public float originHeight = 0.3f;
    public float heightRange = 0.15f;
    public float detectRadius = 10f;
    public LayerMask playerLayer;

    public VisionConeMesh visionCone;

    // 퓨전 네트워크 동기화 변수
    [Networked] public NetworkBool isLaserOn { get; set; }
    [Networked] public NetworkBool prevRightTrigger { get; set; }
    [Networked] public NetworkBool prevSpace { get; set; }

    // 현재 빙의된 로봇인지 확인 (매니저에서 조작)
    public bool isControlledByMe = false;

    private PlayerDetectable_Network targetPlayer;

    public override void Spawned()
    {
        if (visionCone != null) visionCone.gameObject.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        // 1. 입력 처리 (누군가 이 로봇에 빙의해 있다면 실행됨)
        if (GetInput(out NetworkInputData data))
        {
            // --- R키 / Right Trigger (레이저 On/Off) ---
            // (NetworkManager에서 R키가 rightTrigger로 매핑되어 있습니다)
            bool isTriggerPressedThisFrame = data.rightTrigger && !prevRightTrigger;

            if (isTriggerPressedThisFrame)
            {
                // 입력이 들어왔을 때, 상태를 바꾸는 건 서버가 담당합니다.
                if (Object.HasStateAuthority)
                {
                    isLaserOn = !isLaserOn;
                }
            }
            prevRightTrigger = data.rightTrigger;

            // --- Space키 (잠입자 제거) ---
            bool isSpacePressedThisFrame = data.keySpace && !prevSpace;
            if (isSpacePressedThisFrame)
            {
                // 제거 명령도 서버에서만 실행합니다.
                if (Object.HasStateAuthority && targetPlayer != null && targetPlayer.isRemovable)
                {
                    targetPlayer.RequestRemove();
                    Debug.Log("잠입자 파괴 명령 전달!");
                }
            }
            prevSpace = data.keySpace;
        }

        // 2. 잠입자 타겟 캐싱
        if (targetPlayer == null && NetworkManager.Instance.InfiltratorObject != null)
        {
            targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
        }

        // 3. 비전 콘 메쉬 껐다 켜기 (모두에게 동기화)
        if (visionCone != null && visionCone.gameObject.activeSelf != isLaserOn)
        {
            visionCone.gameObject.SetActive(isLaserOn);
            if (isLaserOn) visionCone.SetCone(detectAngle, detectDistance, originHeight, heightRange * 2f);
        }

        // 4. 레이저 판정 로직 (오직 서버만 연산)
        if (!Object.HasStateAuthority) return;

        if (isLaserOn)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);

            foreach (var hit in hits)
            {
                Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
                float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

                if (angleToTarget < detectAngle)
                {
                    PlayerDetectable_Network target = hit.GetComponentInParent<PlayerDetectable_Network>();
                    if (target != null)
                    {
                        target.NotifyDetected();
                    }
                }
            }
        }
    }
}