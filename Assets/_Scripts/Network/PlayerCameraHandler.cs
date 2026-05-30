using Fusion;
using UnityEngine;

public class PlayerCameraHandler : NetworkBehaviour
{
    [Header("추격자 본체 카메라")]
    public Camera myLocalCamera;

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            // 1. 내 카메라 켜기
            myLocalCamera.gameObject.SetActive(true);

            // 2. 로비/기본 카메라 끄기
            Camera lobbyCam = Camera.main;
            if (lobbyCam != null && lobbyCam.gameObject != myLocalCamera.gameObject)
            {
                lobbyCam.gameObject.SetActive(false);
            }

            // 3. 센서 매니저에 내 카메라 등록
            if (NetworkSensorManager.Instance != null)
            {
                NetworkSensorManager.Instance.RegisterChaserCamera(myLocalCamera);
            }
        }
        else
        {
            myLocalCamera.gameObject.SetActive(false);
        }
    }
}