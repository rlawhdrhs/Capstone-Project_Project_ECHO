using Fusion;
using UnityEngine;

public class VRRigSynchronizer : NetworkBehaviour
{
    [Header("아바타 뼈대 연결")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

    public Animator animator;

    [Header("동기화 보정 오프셋")]
    public Vector3 centerPositionOffset;
    public float crouchRate = 0.15f;
    [Header("IK 손목 회전 보정")]
    public Vector3 leftHandRotationOffset = new Vector3(0, 0, 0);
    public Vector3 rightHandRotationOffset = new Vector3(0, 0, 0);

    // --- 애니메이션 동기화 변수 ---
    [Networked] public float netMoveX { get; set; }
    [Networked] public float netMoveZ { get; set; }
    [Networked] public float netCrouch { get; set; }
    [Networked] public NetworkBool PrevLeftClick { get; set; }
    [Networked] public NetworkBool IsFrozen { get; set; }

    // --- 원격 클라이언트들에게 IK 타겟 위치를 동기화하기 위한 네트워크 변수 ---
    [Networked] public Vector3 netLeftHandPos { get; set; }
    [Networked] public Quaternion netLeftHandRot { get; set; }
    [Networked] public Vector3 netRightHandPos { get; set; }
    [Networked] public Quaternion netRightHandRot { get; set; }
    [Networked] public Quaternion netHeadRot { get; set; }
    [Networked] public float netAvatarScale { get; set; } = 1f;

    public bool localFreeze = false;
    public bool isSensorRobot = false;

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
            if (LocalVRRig.Instance != null && !isSensorRobot)
            {
                LocalVRRig.Instance.isOnlineMode = true;

                CharacterController localCC = LocalVRRig.Instance.GetComponent<CharacterController>();
                if (localCC != null) localCC.enabled = false;

                // 하드웨어 리그를 네트워크 스폰 위치로 정렬
                LocalVRRig.Instance.transform.position = this.transform.position;
                LocalVRRig.Instance.transform.rotation = this.transform.rotation;

                if (localCC != null) localCC.enabled = true;

                // 로컬 리그와 네트워크 아바타 뼈대 연결
                LocalVRRig.Instance.avatarRoot = this.transform;
                LocalVRRig.Instance.avatarHead = this.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = this.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = this.avatarRightHand;
                LocalVRRig.Instance.animator = this.animator;
                LocalVRRig.Instance.centerPositionOffset = this.centerPositionOffset;

                // 최초 스폰 시점에 딱 한 번만 키 정렬(캘리브레이션) 진행
                float currentHmdHeight = LocalVRRig.Instance.hardwareHead.localPosition.y;
                if (currentHmdHeight < 0.5f) currentHmdHeight = avatarDefaultEyeHeight;

                float calibratedScale = currentHmdHeight / defaultAvatarHeight;
                RPC_SetAvatarScale(Mathf.Clamp(calibratedScale, 0.7f, 1.4f));
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (IsFrozen || localFreeze) return;

        if (GetInput(out NetworkInputData data))
        {
            if (data.isPossessingDrone) return;

            // 1. 머리 회전 기준으로 몸통 턴 (Y축 평면만 고정)
            Vector3 headForward = data.headRotation * Vector3.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(headForward);

            // 2. 위치 동기화 (루트 지면 고정)
            Vector3 alignedRootPosition = new Vector3(data.headPosition.x, data.rootPosition.y, data.headPosition.z);
            Vector3 dynamicCrouchOffset = transform.forward * (netCrouch * crouchRate);

            transform.position = alignedRootPosition + transform.TransformDirection(centerPositionOffset) + dynamicCrouchOffset;

            // 3. 조종자의 실시간 트래킹 입력 데이터를 네트워크 변수에 기록하여 브로드캐스팅
            netLeftHandPos = data.leftHandPosition;
            netLeftHandRot = data.leftHandRotation;
            netRightHandPos = data.rightHandPosition;
            netRightHandRot = data.rightHandRotation;
            netHeadRot = data.headRotation;

            // --- 레이캐스트 상호작용 로직 ---
            bool isClickedThisFrame = data.leftClick && !PrevLeftClick;
            PrevLeftClick = data.leftClick;
            if (isClickedThisFrame && Runner.IsForward)
            {
                Ray ray = new Ray(data.headPosition, data.headRotation * Vector3.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, 5f))
                {
                    if (hit.collider.TryGetComponent(out VRButton targetButton))
                        targetButton.PressButton();
                }
            }
        }

        // 4. 애니메이션 파라미터 백업 (InputAuthority 데이터를 네트워크 변수로 승격)
        if (HasInputAuthority && animator != null)
        {
            netMoveX = animator.GetFloat("MoveX");
            netMoveZ = animator.GetFloat("MoveZ");
            netCrouch = animator.GetFloat("Crouch");
        }
    }

    public override void Render()
    {
        if (HasInputAuthority)
        {
            if (LocalVRRig.Instance != null)
            {
                if (avatarHead != null) avatarHead.rotation = LocalVRRig.Instance.hardwareHead.rotation;
                if (avatarLeftHand != null)
                {
                    avatarLeftHand.position = LocalVRRig.Instance.hardwareLeftHand.position;
                    avatarLeftHand.rotation = LocalVRRig.Instance.hardwareLeftHand.rotation * Quaternion.Euler(leftHandRotationOffset);
                }
                if (avatarRightHand != null)
                {
                    avatarRightHand.position = LocalVRRig.Instance.hardwareRightHand.position;
                    avatarRightHand.rotation = LocalVRRig.Instance.hardwareRightHand.rotation * Quaternion.Euler(rightHandRotationOffset);
                }
            }
        }
        else
        {
            // [상대방 화면] 네트워크를 타고 넘어온 변수 데이터를 타인 아바타 IK 타겟에 매핑
            if (avatarHead != null) avatarHead.rotation = netHeadRot;
            if (avatarLeftHand != null)
            {
                avatarLeftHand.position = netLeftHandPos;
                avatarLeftHand.rotation = netLeftHandRot * Quaternion.Euler(leftHandRotationOffset);
            }
            if (avatarRightHand != null)
            {
                avatarRightHand.position = netRightHandPos;
                avatarRightHand.rotation = netRightHandRot * Quaternion.Euler(rightHandRotationOffset);
            }

            // 상대방 애니메이터 동기화
            if (animator != null)
            {
                animator.SetFloat("MoveX", netMoveX);
                animator.SetFloat("MoveZ", netMoveZ);
                animator.SetFloat("Crouch", netCrouch);
            }
        }

        // 고정된 스케일 값 적용
        if (netAvatarScale > 0.1f)
        {
            transform.localScale = Vector3.one * netAvatarScale;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetAvatarScale(float scale)
    {
        netAvatarScale = scale;
    }
}