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

    [Header("동기화 보정")]
    public Vector3 centerPositionOffset; // 필요한 경우 몸통 미세조정

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

    // 아바타의 기본 목 길이를 저장
    private float defaultAvatarHeight = 1.7f;
    public float avatarDefaultEyeHeight = 1.7f;

    public override void Spawned()
    {
        if (avatarHead != null)
        {
            defaultAvatarHeight = avatarHead.position.y - transform.position.y;
            if (defaultAvatarHeight < 1.0f) defaultAvatarHeight = avatarDefaultEyeHeight;
        }

        if (HasInputAuthority)
        {
            _cc = GetComponent<CharacterController>();

            if (LocalVRRig.Instance != null && !isSensorRobot)
            {
                LocalVRRig.Instance.isOnlineMode = true;

                // 기존 이동/물리 컴포넌트 비활성화
                var moveProvider = LocalVRRig.Instance.GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
                if (moveProvider != null) moveProvider.enabled = false;

                var localCC = LocalVRRig.Instance.GetComponent<CharacterController>();
                if (localCC != null) localCC.enabled = false;

                // 내 온라인 아바타 몸통에 Local 하드웨어 연결
                LocalVRRig.Instance.avatarRoot = this.transform;
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsFrozen || localFreeze) return;

        if (GetInput(out NetworkInputData data))
        {
            // ==========================================
            // 1. 아바타 동기화 핵심 로직 (LocalVRRig와 동일한 수학 적용)
            // ==========================================

            // A. 회전 (머리가 바라보는 방향)
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(headForward);
            }

            // B. 초기 위치 (Y는 현재 바닥 유지)
            Vector3 targetRootPosition = data.headPosition;
            targetRootPosition.y = transform.position.y;
            transform.position = targetRootPosition;

            // C. 실제 키 측정 및 스케일 적용
            float currentHmdHeight = data.headPosition.y - transform.position.y;
            if (currentHmdHeight < 0.5f) currentHmdHeight = avatarDefaultEyeHeight;

            float scaleRatio = currentHmdHeight / defaultAvatarHeight;
            scaleRatio = Mathf.Clamp(scaleRatio, 0.5f, 1.5f);

            // 네트워크 변수에 스케일 저장 (Render에서 남들에게 보여주기 위함)
            netAvatarScale = scaleRatio;
            transform.localScale = Vector3.one * netAvatarScale;

            // D. 정수리 보임 해결 (XZ 오차 밀어내기)
            if (avatarHead != null)
            {
                Vector3 headOffset = data.headPosition - avatarHead.position;
                headOffset.y = 0f; // 높이는 이미 스케일로 맞췄으므로 무시

                transform.position += headOffset; // 오차만큼 아바타를 끌어당김
                transform.position += transform.TransformDirection(centerPositionOffset); // 미세 오프셋
            }

            // E. IK (머리/손 회전 동기화)
            if (avatarHead != null) avatarHead.rotation = data.headRotation;
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

            // ==========================================
            // 2. 이동 및 상호작용 (기존 로직 유지)
            // ==========================================


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

        // 3. 애니메이션 데이터 갱신
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