using Fusion;
using UnityEngine;

public class PlayerCameraHandler : NetworkBehaviour
{
    public GameObject playerCamera;

    // 포톤 퓨전에서 오브젝트가 생성될 때 실행됨
    public override void Spawned()
    {
        // 내가 조종할 권한이 있는가?
        if (Object.HasInputAuthority)
        {
            playerCamera.SetActive(true);

            Camera lobbyCam = Camera.main;
            if (lobbyCam != null && lobbyCam.gameObject != playerCamera)
            {
                lobbyCam.gameObject.SetActive(false);
            }
        }
        else
        {
            // 내 캐릭터가 아니라면 카메라를 비활성화
            playerCamera.SetActive(false);
        }
    }
}