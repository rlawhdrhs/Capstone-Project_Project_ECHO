using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("아바타 뼈대 연결 (NetworkTransform이 달린 부위들)")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;
    public float heightOffset = 0.9f;
    public float floorY = 0f;
    private CharacterController _cc;
    [Header("애니메이션 동기화")]
    public Animator animator;
    [Networked] public float netMoveX { get; set; }
    [Networked] public float netMoveZ { get; set; }
    [Networked] public float netCrouch { get; set; }
    [Networked] public NetworkBool PrevLeftClick { get; set; }
    [Networked] public NetworkBool IsFrozen { get; set; }
    public bool localFreeze = false;

    [Header("센서 로봇 여부")]
    public bool isSensorRobot = false;
    [Header("중력 관련")]
    [Networked] private float _velocityY { get; set; }
    public float gravity = -9.81f;
    public float jumpForce = 5f;

    public override void Spawned()
    {
        // 1. 내가 조종하는 내 캐릭터일 때
        if (HasInputAuthority)
        {
            _cc = GetComponent<CharacterController>();

            if (LocalVRRig.Instance != null && !isSensorRobot)
            {
                LocalVRRig.Instance.isOnlineMode = true;

                LocalVRRig.Instance.transform.position = transform.position;

                if (LocalVRRig.Instance.avatarRoot != null)
                {
                    LocalVRRig.Instance.avatarRoot.gameObject.SetActive(false);
                }

                LocalVRRig.Instance.avatarRoot = this.transform;
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;
            }

            if (avatarHead != null)
            {
                avatarHead.localScale = Vector3.zero;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsFrozen || localFreeze) return;

        if (GetInput(out NetworkInputData data))
        {
            // --- A. 물리 이동 연산 ---
            if (_cc != null)
            {
                Vector3 currentPos = transform.position;
                Vector3 targetPos = data.headPosition;
                Vector3 moveDelta = new Vector3(targetPos.x - currentPos.x, 0, targetPos.z - currentPos.z);

                if (_cc.isGrounded)
                {
                    _velocityY = -2f;
                    if (data.jump) _velocityY = jumpForce;
                }

                _velocityY += gravity * Runner.DeltaTime;
                moveDelta.y = _velocityY * Runner.DeltaTime;

                _cc.Move(moveDelta);
            }
            else
            {
                Vector3 fallbackPos = data.headPosition;
                fallbackPos.y = transform.position.y;
                transform.position = fallbackPos;
            }

            // --- B. IK 동기화 (머리 및 양손) ---
            if (avatarHead != null)
            {
                avatarHead.position = data.headPosition;
                avatarHead.rotation = data.headRotation;
            }
            if (avatarLeftHand != null) avatarLeftHand.position = data.leftHandPosition;
            if (avatarRightHand != null) avatarRightHand.position = data.rightHandPosition;

            // --- C. 몸통 회전 (센서 로봇이 아닐 때만 몸통이 내 시야를 따라 돎) ---
            if (!isSensorRobot)
            {
                Vector3 headForward = data.headRotation * Vector3.forward;
                headForward.y = 0f;
                if (headForward.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(headForward);
                }
            }

            // --- D. 버튼 클릭 이벤트 ---
            bool isClickedThisFrame = data.leftClick && !PrevLeftClick;
            PrevLeftClick = data.leftClick;

            if (isClickedThisFrame)
            {
                Ray ray = new Ray(data.headPosition, data.headRotation * Vector3.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    VRButton targetButton = hit.collider.GetComponent<VRButton>();
                    if (targetButton != null) targetButton.PressButton();
                }
            }
        }

        // --- E. 애니메이션 동기화 ---
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetFrozenState(NetworkBool freezeState)
    {
        IsFrozen = freezeState;

        // 얼어붙을 때 물리 연산(CC)도 끄기
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !freezeState;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetFrozenState(NetworkBool freezeState, Vector3 freezePos)
    {
        IsFrozen = freezeState;

        if (freezeState)
        {
            // 🌟 픽스 1: 서버 지연 시간 동안 아바타가 로봇 위치로 따라온 것을 강제로 원래 자리에 되돌려 박제함
            transform.position = freezePos;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !freezeState;
    }
}