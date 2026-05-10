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

    private Vector3 humanStoredPosition;
    private Quaternion humanStoredRotation;

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
            if (currentDrone != null && data.leftButtonA)
            {
                ReturnToHuman();
            }
        }
    }

    private void LateUpdate()
    {
        if (currentDrone != null && xrOrigin != null && LocalVRRig.Instance.hardwareHead != null)
        {
            // 1. 목표 지점은 드론의 바닥이 아니라 드론의 '카메라(눈)' 위치입니다.
            Vector3 targetEyePosition = currentDrone.droneBody.position;

            // 2. 내 실제 헤드셋(카메라)이 목표 지점에 가기 위해 얼마나 이동해야 하는지 계산합니다.
            Vector3 offset = targetEyePosition - LocalVRRig.Instance.hardwareHead.position;

            // 3. XR Origin 자체를 그 오차만큼 밀어줍니다. (로코모션이 움직이려 해도 여기서 강제로 붙잡음)
            xrOrigin.position += offset;
        }
    }

    public void PossessDrone(SensorSynchronizer targetDrone)
    {
        if (targetDrone == null || myHumanAvatar == null) return;

        currentDrone = targetDrone;

        humanStoredPosition = xrOrigin.position;
        humanStoredRotation = xrOrigin.rotation;

        myHumanAvatar.localFreeze = true;

        RPC_RequestPossession(myHumanAvatar.Object, targetDrone.Object, Runner.LocalPlayer);
        TeleportXRToDrone(targetDrone);
    }

    public void ReturnToHuman()
    {
        if (currentDrone == null) return;

        SensorSynchronizer previousDrone = currentDrone;
        currentDrone = null;

        myHumanAvatar.localFreeze = false;

        RPC_RequestReturn(myHumanAvatar.Object, previousDrone.Object, Runner.LocalPlayer);
        TeleportXRToHuman(humanStoredPosition, humanStoredRotation);
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
    }

    private void TeleportXRToHuman(Vector3 forcePos, Quaternion forceRot)
    {
        xrOrigin.position = forcePos;
        xrOrigin.rotation = forceRot;

        LocalVRRig.Instance.avatarRoot = myHumanAvatar.transform;
        LocalVRRig.Instance.avatarHead = myHumanAvatar.avatarHead;
        LocalVRRig.Instance.avatarLeftHand = myHumanAvatar.avatarLeftHand;
        LocalVRRig.Instance.avatarRightHand = myHumanAvatar.avatarRightHand;
        LocalVRRig.Instance.animator = myHumanAvatar.animator;

        CharacterController xrCC = xrOrigin.GetComponent<CharacterController>();
        if (xrCC != null) xrCC.enabled = true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPossession(NetworkObject humanObj, NetworkObject droneObj, PlayerRef player)
    {
        humanObj.GetComponent<VRRigSynchronizer>().IsFrozen = true;
        droneObj.AssignInputAuthority(player);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestReturn(NetworkObject humanObj, NetworkObject droneObj, PlayerRef player)
    {
        droneObj.RemoveInputAuthority();
        humanObj.GetComponent<VRRigSynchronizer>().IsFrozen = false;
    }
}