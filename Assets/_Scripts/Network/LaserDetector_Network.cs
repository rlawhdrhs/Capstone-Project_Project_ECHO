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

    // 현재 빙의된 로봇인지 확인 (매니저에서 조작)
    public bool isControlledByMe = false;

    private PlayerDetectable_Network targetPlayer;

    public override void Spawned()
    {
        if (visionCone != null) visionCone.gameObject.SetActive(false);
    }

    public override void FixedUpdateNetwork()
    {
        // 1. 트리거 입력 처리 (서버/클라이언트 동기화)
        if (GetInput(out NetworkInputData data))
        {
            // '이번 틱에 눌렸고, 이전 틱에는 안 눌렸을 때'만 작동
            bool isTriggerPressedThisFrame = data.rightTrigger && !prevRightTrigger;
            prevRightTrigger = data.rightTrigger;

            if (isControlledByMe && isTriggerPressedThisFrame)
            {
                if (Object.HasStateAuthority)
                {
                    isLaserOn = !isLaserOn;
                }
            }
        }

        // 2. 잠입자 타겟 캐싱
        if (targetPlayer == null && NetworkManager.Instance.InfiltratorObject != null)
        {
            targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
        }

        // 3. 비전 콘 메쉬 껐다 켜기 (동기화)
        if (visionCone != null && visionCone.gameObject.activeSelf != isLaserOn)
        {
            visionCone.gameObject.SetActive(isLaserOn);
            if (isLaserOn) visionCone.SetCone(detectAngle, detectDistance, originHeight, heightRange * 2f);
        }

        // =========================================================
        // 4. 레이저 판정 로직 (오직 서버만 연산)
        // =========================================================
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
                        // 🌟 [핵심] 찾았으면 찌릅니다. (기존 CheckPlayerDetected와 덮어쓰기 로직은 삭제!)
                        target.NotifyDetected();
                    }
                }
            }
        }
    } // 🌟 FixedUpdateNetwork를 닫는 중괄호 추가 완료
}