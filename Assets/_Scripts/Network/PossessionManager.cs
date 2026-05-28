using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PossessionManager : NetworkBehaviour
{
    public static PossessionManager Instance;

    [Header("아바타 및 로봇 정보")]
    public VRRigSynchronizer myHumanAvatar;
    public SensorSynchronizer currentDrone;

    [Header("로컬 시스템 정보")]
    public Transform xrOrigin;

    [Header("사운드 설정 (SoundManager 연동)")]
    public SoundType possessSoundType;
    public SoundType returnSoundType;

    private Vector3 humanStoredPosition;
    private Quaternion humanStoredRotation;

    private Vector3 droneInitialPosition;
    private Quaternion droneInitialRotation;

    [Networked] public NetworkBool PrevLeftButtonA { get; set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            Instance = this;
            myHumanAvatar = GetComponent<VRRigSynchronizer>();

            if (LocalVRRig.Instance != null)
            {
                xrOrigin = LocalVRRig.Instance.transform;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            bool isReturnPressedThisFrame = data.leftButtonA && !PrevLeftButtonA;
            PrevLeftButtonA = data.leftButtonA;

            if (currentDrone != null && isReturnPressedThisFrame && Runner.IsForward)
            {
                ReturnToHuman();
            }
        }
    }

    /*private void LateUpdate()
    {
        // 드론에 빙의한 상태이고, 해당 드론에 제한 구역(movementZone)이 설정되어 있을 때만 가동
        if (currentDrone != null && xrOrigin != null && currentDrone.movementZone != null && LocalVRRig.Instance?.hardwareHead != null)
        {
            // 1. 현재 플레이어의 실제 월드 머리(카메라) 위치를 가져옵니다.
            Vector3 currentHeadPos = LocalVRRig.Instance.hardwareHead.position;

            // 2. 해당 머리 위치가 제한 구역 콜라이더 내부/표면 상의 어디에 복사되는지 계산합니다.
            Vector3 clampedHeadPos = currentDrone.movementZone.ClosestPoint(currentHeadPos);

            // 3. 만약 머리가 구역 밖으로 삐져나갔다면 (두 좌표가 다르다면)
            if (currentHeadPos != clampedHeadPos)
            {
                // 구역 밖으로 나간 만큼의 오차 벡터를 계산합니다.
                Vector3 pushOffset = clampedHeadPos - currentHeadPos;

                // CharacterController가 켜져 있으면 좌표계가 충돌하므로 잠시 끄고 이동시킵니다.
                CharacterController xrCC = xrOrigin.GetComponent<CharacterController>();
                if (xrCC != null) xrCC.enabled = false;

                // 오차만큼 xrOrigin을 밀어서 머리를 구역 안으로 강제 진입시킵니다.
                xrOrigin.position += pushOffset;

                if (xrCC != null) xrCC.enabled = true;
            }
        }
    }*/

    public void PossessDrone(SensorSynchronizer targetDrone)
    {
        if (targetDrone == null || myHumanAvatar == null || currentDrone != null) return;

        if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.CurrentMissionIndex == 0)
        {
            Debug.LogWarning("<color=red>⚠️ [빙의 실패] 첫 번째 미션(전력 복구)이 완료되기 전에는 드론에 빙의할 수 없습니다!</color>");
            return; // 함수를 여기서 즉시 종료하여 텔레포트 및 RPC 실행을 막습니다.
        }

        currentDrone = targetDrone;

        if (targetDrone.localBoundaryWall != null)
        {
            targetDrone.localBoundaryWall.SetActive(true);
            Debug.Log($"🧱 [로컬] {targetDrone.name}의 이동 제한 벽 활성화!");
        }

        if (Runner.IsForward)
        {
            humanStoredPosition = xrOrigin.position;
            humanStoredRotation = xrOrigin.rotation;

            droneInitialPosition = targetDrone.transform.position;
            droneInitialRotation = targetDrone.transform.rotation;

            TeleportXRToDrone(targetDrone);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.EmitSound(targetDrone.transform.position, 2f, possessSoundType);
            }
        }

        myHumanAvatar.localFreeze = true;

        RPC_RequestPossession(myHumanAvatar.Object, targetDrone.Object, Runner.LocalPlayer);
    }

    public void ReturnToHuman()
    {
        if (currentDrone == null) return;

        SensorSynchronizer previousDrone = currentDrone;
        currentDrone = null;

        if (previousDrone.localBoundaryWall != null)
        {
            previousDrone.localBoundaryWall.SetActive(false);
            Debug.Log($"🔓 [로컬] {previousDrone.name}의 이동 제한 벽 해제!");
        }

        myHumanAvatar.localFreeze = false;
        RPC_RequestReturn(myHumanAvatar.Object, previousDrone.Object, Runner.LocalPlayer, droneInitialPosition, droneInitialRotation);

        TeleportXRToHuman(humanStoredPosition, humanStoredRotation);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(humanStoredPosition, 2f, returnSoundType);
        }
    }

    private void TeleportXRToDrone(SensorSynchronizer drone)
    {
        CharacterController xrCC = xrOrigin.GetComponent<CharacterController>();
        if (xrCC != null) xrCC.enabled = false;

        xrOrigin.position = drone.transform.position;
        xrOrigin.rotation = drone.transform.rotation;

        LocalVRRig.Instance.avatarRoot = drone.transform;
        LocalVRRig.Instance.avatarHead = drone.droneBody;
        LocalVRRig.Instance.avatarLeftHand = null;
        LocalVRRig.Instance.avatarRightHand = null;
        LocalVRRig.Instance.animator = null;

        if (xrCC != null) xrCC.enabled = true;
    }

    private void TeleportXRToHuman(Vector3 forcePos, Quaternion forceRot)
    {
        CharacterController xrCC = xrOrigin.GetComponent<CharacterController>();

        if (xrCC != null) xrCC.enabled = false;
        xrOrigin.position = forcePos;
        xrOrigin.rotation = forceRot;

        LocalVRRig.Instance.avatarRoot = myHumanAvatar.transform;
        LocalVRRig.Instance.avatarHead = myHumanAvatar.avatarHead;
        LocalVRRig.Instance.avatarLeftHand = myHumanAvatar.avatarLeftHand;
        LocalVRRig.Instance.avatarRightHand = myHumanAvatar.avatarRightHand;
        LocalVRRig.Instance.animator = myHumanAvatar.animator;

        if (xrCC != null) xrCC.enabled = true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPossession(NetworkObject humanObj, NetworkObject droneObj, PlayerRef player)
    {
        humanObj.GetComponent<VRRigSynchronizer>().IsFrozen = true;
        droneObj.AssignInputAuthority(player);

        if (droneObj.TryGetComponent(out LaserDetector_Network dLaser)) dLaser.isDetectorActive = true;

        if (droneObj.TryGetComponent(out SensorSynchronizer sensor))
        {
            sensor.netIsLightOn = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestReturn(NetworkObject humanObj, NetworkObject droneObj, PlayerRef player, Vector3 droneReturnPos, Quaternion droneReturnRot)
    {
        droneObj.transform.position = droneReturnPos;
        droneObj.transform.rotation = droneReturnRot;

        droneObj.RemoveInputAuthority();
        humanObj.GetComponent<VRRigSynchronizer>().IsFrozen = false;

        if (droneObj.TryGetComponent(out LaserDetector_Network dLaser)) dLaser.isDetectorActive = false;

        if (droneObj.TryGetComponent(out SensorSynchronizer sensor))
        {
            sensor.netIsLightOn = false;
        }
    }
}