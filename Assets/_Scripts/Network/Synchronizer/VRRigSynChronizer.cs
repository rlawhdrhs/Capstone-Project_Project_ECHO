using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("아바타 뼈대 연결")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

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


    [Header("아바타 스케일 (사용자 키 맞춤)")]
    [Networked] public float netAvatarScale { get; set; } = 1f;

    // 아바타의 기본 목 길이를 저장
    private float defaultAvatarHeight = 1.7f;
    public float avatarDefaultEyeHeight = 1.7f;
    private Vector3 _localHeadOffset;

    public override void Spawned()
    {
        if (avatarHead != null)
        {
            defaultAvatarHeight = avatarHead.position.y - transform.position.y;
            if (defaultAvatarHeight < 1.0f) defaultAvatarHeight = avatarDefaultEyeHeight;

            _localHeadOffset = transform.InverseTransformPoint(avatarHead.position);
        }

        if (HasInputAuthority)
        {

            if (LocalVRRig.Instance != null && !isSensorRobot)
            {
                LocalVRRig.Instance.isOnlineMode = true;

                CharacterController localCC = LocalVRRig.Instance.GetComponent<CharacterController>();
                if (localCC != null) localCC.enabled = false;

                // xr_origin을 네트워크 매니저가 정해준 이 아바타의 스폰 위치로 순간이동
                LocalVRRig.Instance.transform.position = this.transform.position;
                LocalVRRig.Instance.transform.rotation = this.transform.rotation;

                // 이동 끝났으니 CC 다시 켜기
                if (localCC != null) localCC.enabled = true;


                // 기존 이동 컴포넌트 비활성화
                var moveProvider = LocalVRRig.Instance.GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
                if (moveProvider != null) moveProvider.enabled = false;

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
            // 1. 머리가 바라보는 방향으로 몸통 회전
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(headForward);

            // ========================================================
            // 2. 플레이어 키 측정 및 아바타 크기(스케일) 조절
            // (카메라 높이 1.7m와 XR Origin 높이 4.4m의 차이를 완벽히 계산)
            // ========================================================
            float currentHmdHeight = data.headPosition.y - data.rootPosition.y;

            // 시뮬레이터 오류로 카메라가 바닥에 박혔을 때만 방어
            if (currentHmdHeight < 0.1f) currentHmdHeight = avatarDefaultEyeHeight;

            // 아바타가 작아지거나 커지도록 스케일 적용
            netAvatarScale = currentHmdHeight / defaultAvatarHeight;
            transform.localScale = Vector3.one * netAvatarScale;

            Vector3 alignedRootPosition = new Vector3(data.headPosition.x, data.rootPosition.y, data.headPosition.z);

            transform.position = alignedRootPosition;

            // (선택) 정수리가 보인다면 인스펙터의 centerPositionOffset Y값을 아주 살짝(0.1 등) 올려주세요.
            transform.position += transform.TransformDirection(centerPositionOffset);


            // 4. IK 동기화 (머리와 손의 회전/위치 덮어쓰기)
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

            // --- 상호작용 로직 ---
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

        // 5. 애니메이션 갱신
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