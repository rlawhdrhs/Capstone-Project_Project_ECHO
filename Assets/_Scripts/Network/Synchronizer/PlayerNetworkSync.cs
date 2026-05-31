using Fusion;
using UnityEngine;

public class PlayerNetworkSync : NetworkBehaviour
{
    public override void Spawned()
    {
        // 내 로컬 플레이어의 아바타가 네트워크 상에 생성 완료된 시점일 때만 실행
        if (Object.HasInputAuthority)
        {
            if (LocalVRRig.Instance != null)
            {
                // 1. 캐릭터 컨트롤러를 일시적으로 끕니다 (순간이동 차단 해제)
                if (LocalVRRig.Instance.TryGetComponent<CharacterController>(out var cc))
                {
                    cc.enabled = false;
                }

                // 2. 내 진짜 VR 몸뚱아리를 서버가 지정해준 이 아바타의 스폰 위치/회전값으로 텔레포트 시킵니다.
                LocalVRRig.Instance.transform.position = transform.position;
                LocalVRRig.Instance.transform.rotation = transform.rotation;

                // 3. 순간이동이 완료되었으므로 캐릭터 컨트롤러를 다시 켭니다.
                if (cc != null)
                {
                    cc.enabled = true;
                }

                Debug.Log($"<color=lime>[동기화 성공] 로컬 VR Rig를 {transform.position} 좌표로 강제 소환 완료!</color>");
            }
        }
    }
}