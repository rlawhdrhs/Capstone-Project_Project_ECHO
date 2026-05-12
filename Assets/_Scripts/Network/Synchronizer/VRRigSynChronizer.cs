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

    [Header("동기화 보정 (매우 중요)")]
    [Tooltip("VR 카메라(눈)와 아바타 Head 본(목) 사이의 거리 차이. 보통 Y는 -0.1~-0.15, Z는 -0.05~-0.1 정도입니다.")]
    public Vector3 headPositionOffset = new Vector3(0f, -0.12f, -0.05f);
    public Vector3 headRotationOffset = Vector3.zero; // 아바타 모델에 따라 머리가 숙여져 있다면 X축 조절 필요

    public Vector3 leftHandPositionOffset = Vector3.zero;
    public Vector3 leftHandRotationOffset = Vector3.zero;
    public Vector3 rightHandPositionOffset = Vector3.zero;
    public Vector3 rightHandRotationOffset = Vector3.zero;

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
    public float moveSpeed = 3f;

    [Header("아바타 스케일 (사용자 키 맞춤)")]
    [Networked] public float netAvatarScale { get; set; } = 1f;
    private float defaultHeadToRootOffset;

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

                LocalVRRig.Instance.avatarRoot = this.transform;
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;

                if (avatarHead != null)
                {
                    defaultHeadToRootOffset = avatarHead.position.y - transform.position.y;
                }
                //LocalVRRig.Instance.CalibrateAvatarHeight(this);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsFrozen || localFreeze) return;

        if (GetInput(out NetworkInputData data))
        {
            // 1. 목표 위치는 현재 사용자의 카메라(머리) 위치
            Vector3 targetRootPosition = data.headPosition;

            // 2. 카메라 높이에서, 아바타의 원래 키(머리~발바닥 거리)만큼 아래로 내린 곳이 발바닥 위치가 됨!
            targetRootPosition.y -= defaultHeadToRootOffset;

            // 3. 아바타 몸통 전체를 이동 (아바타에 CharacterController가 없으므로 직접 position 변경)
            transform.position = targetRootPosition;


            // --- 회전 처리 (기존과 동일) ---
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(headForward);
            }

            // --- IK 동기화 (주의: 이제 머리(avatarHead) position은 건드리지 마세요!) ---
            // avatarHead.position = data.headPosition; // <--- 이 코드는 반드시 삭제!!

            if (avatarHead != null)
            {
                // 머리의 '위치'는 몸통이 따라왔으므로 자연스럽게 맞춰짐. '회전(고개 각도)'만 맞춰줍니다.
                avatarHead.rotation = data.headRotation;
            }

            if (avatarLeftHand != null)
            {
                avatarLeftHand.position = data.leftHandPosition;
                avatarLeftHand.rotation = data.leftHandRotation;
            }
            if (avatarRightHand != null)
            {
                avatarRightHand.position = data.rightHandPosition;
                avatarRightHand.rotation = data.rightHandRotation;
            }

            // --- 4. 상호작용 ---
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

        // --- 5. 애니메이션 갱신 ---
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

        if (netAvatarScale > 0.1f)
        {
            transform.localScale = Vector3.one * netAvatarScale;
        }
    }

    // RPC 함수들은 기존과 동일하게 유지...
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetFrozenState(NetworkBool freezeState) { /* ... */ }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetFrozenState(NetworkBool freezeState, Vector3 freezePos) { /* ... */ }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetAvatarScale(float scale)
    {
        netAvatarScale = scale;
    }
}