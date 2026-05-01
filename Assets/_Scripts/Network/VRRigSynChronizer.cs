using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("아바타 뼈대 연결 (NetworkTransform이 달린 부위들)")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;
    public float heightOffset = 0.9f;
    [Header("애니메이션 동기화")]
    public Animator animator;
    [Networked] public float netMoveX { get; set; }
    [Networked] public float netMoveZ { get; set; }
    [Networked] public float netCrouch { get; set; }

    public override void Spawned()
    {
        // 1. 내가 조종하는 내 캐릭터일 때
        if (HasInputAuthority)
        {
            if (LocalVRRig.Instance != null)
            {
                LocalVRRig.Instance.avatarRoot = this.transform; // 캐릭터 몸통
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;
            }

            // 2. 내 머리 렌더러만 끄기
            if (avatarHead != null)
            {
                avatarHead.localScale = Vector3.zero;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 bodyPosition = data.headPosition;

            bodyPosition.y = heightOffset;

            transform.position = bodyPosition;
            // 1. 머리 및 양손 위치 동기화
            if (avatarHead != null)
            {
                avatarHead.position = data.headPosition;
                avatarHead.rotation = data.headRotation;
            }

            // 2. 몸통 회전 동기화
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f;

            if (headForward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(headForward);
            }

            // 3. IK 타겟 동기화 (기존 코드)
            if (avatarLeftHand != null) avatarLeftHand.position = data.leftHandPosition;
            if (avatarRightHand != null) avatarRightHand.position = data.rightHandPosition;
        }
        // 내 캐릭터일 때
        if (HasInputAuthority && animator != null)
        {
            netMoveX = animator.GetFloat("MoveX");
            netMoveZ = animator.GetFloat("MoveZ");
            netCrouch = animator.GetFloat("Crouch");
        }
    }

    public override void Render()
    {
        // 남의 캐릭터일 때
        if (!HasInputAuthority && animator != null)
        {
            animator.SetFloat("MoveX", netMoveX);
            animator.SetFloat("MoveZ", netMoveZ);
            animator.SetFloat("Crouch", netCrouch);
        }
    }
}