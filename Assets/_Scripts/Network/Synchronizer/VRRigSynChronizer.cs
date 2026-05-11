using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("아바타 뼈대 연결")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

    private CharacterController _cc;
    public Animator animator;

    [Networked] public float netMoveX { get; set; }
    [Networked] public float netMoveZ { get; set; }
    [Networked] public float netCrouch { get; set; }
    [Networked] public NetworkBool PrevLeftClick { get; set; }
    [Networked] public NetworkBool IsFrozen { get; set; }
    public bool localFreeze = false;
    public bool isSensorRobot = false;

    [Header("중력 관련")]
    [Networked] private float _velocityY { get; set; }
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    public float moveSpeed = 3f; // 이동 속도 추가

    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            _cc = GetComponent<CharacterController>();

            if (LocalVRRig.Instance != null && !isSensorRobot)
            {
                LocalVRRig.Instance.isOnlineMode = true;

                var moveProvider = LocalVRRig.Instance.GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
                if (moveProvider != null) moveProvider.enabled = false;

                var localCC = LocalVRRig.Instance.GetComponent<CharacterController>();
                if (localCC != null) localCC.enabled = false;

                // 뼈대 연결
                LocalVRRig.Instance.avatarRoot = this.transform;
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;
            }

            if (avatarHead != null) avatarHead.localScale = Vector3.zero; // 내 시야 가림 방지
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsFrozen || localFreeze) return;

        if (GetInput(out NetworkInputData data))
        {
            // --- 1. 회전: 머리가 바라보는 방향(Y축)으로 몸통을 맞춥니다 ---
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f; // 땅과 수평 유지
            if (headForward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(headForward);
            }

            // --- 2. 이동: 아바타의 Character Controller가 직접 움직입니다 ---
            if (_cc != null && _cc.enabled)
            {
                // transform.forward는 이미 위에서 머리 방향으로 맞춰졌으므로, 앞으로(moveZ) 누르면 시선 방향으로 갑니다.
                Vector3 moveDirection = (transform.forward * data.moveZ) + (transform.right * data.moveX);
                Vector3 moveDelta = moveDirection * moveSpeed * Runner.DeltaTime;

                if (_cc.isGrounded)
                {
                    _velocityY = -2f;
                    if (data.jump) _velocityY = jumpForce;
                }

                _velocityY += gravity * Runner.DeltaTime;
                moveDelta.y = _velocityY * Runner.DeltaTime;

                _cc.Move(moveDelta);
            }

            // --- 3. IK (머리와 양손 위치 동기화) ---
            if (avatarHead != null)
            {
                avatarHead.position = data.headPosition;
                avatarHead.rotation = data.headRotation;
            }
            if (avatarLeftHand != null) avatarLeftHand.position = data.leftHandPosition;
            if (avatarRightHand != null) avatarRightHand.position = data.rightHandPosition;

            // --- 4. 상호작용 (버튼 클릭) ---
            bool isClickedThisFrame = data.leftClick && !PrevLeftClick;
            PrevLeftClick = data.leftClick;
            if (isClickedThisFrame)
            {
                Ray ray = new Ray(data.headPosition, data.headRotation * Vector3.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.collider.TryGetComponent(out VRButton targetButton))
                        targetButton.PressButton();
                }
            }
        }

        // --- 5. 애니메이션 데이터 갱신 ---
        if (HasInputAuthority && animator != null)
        {
            netMoveX = animator.GetFloat("MoveX");
            netMoveZ = animator.GetFloat("MoveZ");
            netCrouch = animator.GetFloat("Crouch");
        }
    }

    public override void Render()
    {
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
            transform.position = freezePos;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = !freezeState;
    }
}