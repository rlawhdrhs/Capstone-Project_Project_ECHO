using Fusion;
using UnityEngine;

public class RobotScanner_Network : NetworkBehaviour
{
    [Header("Scanner Settings")]
    public VisionConeMesh visionConeMesh; // 인스펙터에서 비전 콘 모델 연결
    public float viewAngle = 25f;
    public float viewDistance = 10f;

    [Header("Network States")]
    [Networked] public NetworkBool isScannerOn { get; set; }

    // 토글 방식(눌렀을 때 한 번만 작동)을 위한 이전 프레임 입력 저장
    [Networked] private NetworkBool prevKeyR { get; set; }
    [Networked] private NetworkBool prevKeySpace { get; set; }

    public override void FixedUpdateNetwork()
    {
        // 1. 입력 가져오기 (권한이 있는 클라이언트와 서버에서 실행됨)
        if (GetInput(out NetworkInputData data))
        {
            // --- R키 토글 로직 ---
            bool currentKeyR = data.keyR;
            if (currentKeyR && !prevKeyR) // 이전엔 안 눌렸는데 지금 눌렀다면
            {
                isScannerOn = !isScannerOn; // 스캐너 On/Off 전환
            }
            prevKeyR = currentKeyR;

            // --- 스캐너가 켜져 있을 때 감지 로직 ---
            if (isScannerOn)
            {
                DetectAndRemovePlayer(data);
            }
        }
    }

    public override void Render()
    {
        // 시각적인 비전 콘 끄고 켜기는 Render에서 처리 (모든 클라이언트 화면에 동기화)
        if (visionConeMesh != null)
        {
            visionConeMesh.gameObject.SetActive(isScannerOn);
        }
    }

    private void DetectAndRemovePlayer(NetworkInputData data)
    {
        // 잠입자(Infiltrator) 찾기
        if (NetworkManager.Instance == null || NetworkManager.Instance.InfiltratorObject == null) return;

        PlayerDetectable_Network targetPlayer = NetworkManager.Instance.InfiltratorObject.GetComponent<PlayerDetectable_Network>();
        if (targetPlayer == null) return;

        Vector3 dirToTarget = targetPlayer.transform.position - transform.position;
        float distance = dirToTarget.magnitude;

        // 1. 거리 안에 들어왔는가?
        if (distance <= viewDistance)
        {
            // 2. 내 시야각(viewAngle) 안에 들어왔는가? (전방 벡터와 타겟 벡터의 각도 비교)
            float angle = Vector3.Angle(transform.forward, dirToTarget);
            if (angle <= viewAngle)
            {
                targetPlayer.NotifyDetected();

                // --- Spacebar 제거 로직 ---
                bool currentKeySpace = data.keySpace;
                if (currentKeySpace && !prevKeySpace)
                {
                    // 잠입자의 게이지가 다 차서 제거 가능한 상태라면
                    if (targetPlayer.isRemovable)
                    {
                        targetPlayer.RequestRemove();
                        Debug.Log("잠입자 제거 성공!");
                    }
                }
                prevKeySpace = currentKeySpace;
            }
        }
    }
}