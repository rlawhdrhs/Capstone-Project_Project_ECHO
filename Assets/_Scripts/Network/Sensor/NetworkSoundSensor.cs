using Fusion;
using UnityEngine;

// 기존 SoundSensor를 네트워크 버전으로 업그레이드
public class NetworkSoundSensor : NetworkBehaviour
{
    public int sensorId;
    public Transform cameraPoint; // 로봇의 눈 위치

    // 기존의 잡다한 컴포넌트들
    private SoundListener listener;
    // public SimpleMovement movement; -> 네트워크에서는 NetworkCharacterController 등으로 대체 필요

    // 이 로봇이 현재 조종받고 있는지 네트워크로 모두가 공유하는 변수
    [Networked] public NetworkBool IsControlled { get; set; }

    private void Awake()
    {
        listener = GetComponent<SoundListener>();
    }

    public override void Spawned()
    {
        // 맵에 스폰되면 매니저에 자신을 등록
        if (NetworkSensorManager.Instance != null)
        {
            NetworkSensorManager.Instance.RegisterSensor(this);
        }
    }

    // 서버(호스트)에게 이 로봇을 조종하겠다고 요청하는 RPC 함수
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestControl(PlayerRef player)
    {
        Debug.Log($"[서버 수신 완료] {player} 가 조종권을 달라고 합니다!");

        // 서버가 이 로봇의 조종 권한을 요청한 플레이어에게 부여
        Object.AssignInputAuthority(player);

        VRRigSynchronizer rig = GetComponent<VRRigSynchronizer>();
        if (rig != null)
        {
            rig.IsFrozen = false;
        }
        IsControlled = true;
    }

    // 서버(호스트)에게 이 로봇 조종을 그만두겠다고 요청하는 RPC 함수
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReleaseControl()
    {
        Object.RemoveInputAuthority();

        VRRigSynchronizer rig = GetComponent<VRRigSynchronizer>();
        if (rig != null) rig.IsFrozen = true;

        IsControlled = false;
    }

    // 카메라 셋업은 나(추격자)의 로컬 화면에서만 일어남
    public void SetupLocalCamera(Camera mainCamera)
    {
        if (mainCamera != null && cameraPoint != null)
        {
            mainCamera.transform.SetParent(cameraPoint);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }
    }
}