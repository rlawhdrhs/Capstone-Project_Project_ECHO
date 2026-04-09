using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("남들에게 보일 아바타")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

    [Header("실제 로컬 아바타")]
    private Transform localHead;
    private Transform localLeftHand;
    private Transform localRightHand;

    [Networked] public Vector3 networkHeadPos { get; set; }
    [Networked] public Quaternion networkHeadRot { get; set; }

    [Networked] public Vector3 networkLeftHandPos { get; set; }
    [Networked] public Quaternion networkLeftHandRot { get; set; }

    [Networked] public Vector3 networkRightHandPos { get; set; }
    [Networked] public Quaternion networkRightHandRot { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            localHead = LocalVRRig.Instance.localHead;
            localLeftHand = LocalVRRig.Instance.localLeftHand;
            localRightHand = LocalVRRig.Instance.localRightHand;

            avatarHead.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 주인(방장)만 내 실제 VR 위치를 네트워크 변수에 덮어씌움
        if (HasStateAuthority && localHead != null)
        {
            networkHeadPos = localHead.position;
            networkHeadRot = localHead.rotation;

            networkLeftHandPos = localLeftHand.position;
            networkLeftHandRot = localLeftHand.rotation;

            networkRightHandPos = localRightHand.position;
            networkRightHandRot = localRightHand.rotation;
        }
    }

    public override void Render()
    {
        // 모든 사람 화면에서 위치 동기화
        avatarHead.position = networkHeadPos;
        avatarHead.rotation = networkHeadRot;

        avatarLeftHand.position = networkLeftHandPos;
        avatarLeftHand.rotation = networkLeftHandRot;

        avatarRightHand.position = networkRightHandPos;
        avatarRightHand.rotation = networkRightHandRot;
    }
}