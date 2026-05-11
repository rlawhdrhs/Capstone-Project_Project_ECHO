using Fusion;
using UnityEngine;

public class ChaserSetup : NetworkBehaviour
{
    [Header("추격자 본체 카메라")]
    public Camera myLocalCamera; // 내 프리팹 안에 있는 카메라

    public override void Spawned()
    {
        // 1. 이 캐릭터가 '나(로컬 클라이언트)'의 조종을 받는 캐릭터일 때만 실행
        if (HasInputAuthority)
        {
            if (NetworkSensorManager.Instance != null && myLocalCamera != null)
            {
                // 매니저에게 내 카메라를 넘겨줌
                NetworkSensorManager.Instance.RegisterChaserCamera(myLocalCamera);
            }
            else
            {
                Debug.LogError("SensorManager가 씬에 없거나, 추격자 카메라가 연결되지 않았습니다.");
            }
        }
        else
        {
            // 2. 남(호스트)의 화면에 보이는 내 캐릭터 껍데기의 카메라는 꺼버림 (이중 렌더링/시야 겹침 방지)
            if (myLocalCamera != null)
            {
                myLocalCamera.gameObject.SetActive(false);
            }
        }
    }
}