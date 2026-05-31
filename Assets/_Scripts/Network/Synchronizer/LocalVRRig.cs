using UnityEngine;

public class LocalVRRig : MonoBehaviour
{
    public static LocalVRRig Instance;
    private void Awake() => Instance = this;

    [Header("실제 VR 하드웨어")]
    public Transform hardwareHead;
    public Transform hardwareLeftHand;
    public Transform hardwareRightHand;

    [Header("내 아바타 요소")]
    public Transform avatarRoot;
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

    [Header("설정")]
    public Animator animator;
    public Vector3 centerPositionOffset;

    private Vector3 previousPosition;

    [Header("아바타 키 설정")]
    public float standingHeight = 1.7f;
    public float crouchingHeight = 0.5f;
    public float currentCrouch;
    private float calibratedStandingHeight = 1.7f;
    public float crouchActivationRatio = 0.7f;
    private bool isHeightCalibrated = false;

    [Header("애니메이션 보정")]
    public float animationSmoothness = 10f;
    private float currentMoveX;
    private float currentMoveZ;
    public float crouchRate = 0.15f;
    public bool isOnlineMode = false;

    [Header("아바타 캘리브레이션")]
    public float avatarDefaultEyeHeight = 1.7f;
    private float defaultAvatarHeight;
    private CharacterController localCC;
    private bool isLocalScaleCalibrated = false;


    void Start()
    {
        if (avatarRoot != null) previousPosition = avatarRoot.position;

        if (avatarRoot != null && avatarHead != null)
        {
            defaultAvatarHeight = avatarHead.position.y - avatarRoot.position.y;
            if (defaultAvatarHeight < 1.0f) defaultAvatarHeight = avatarDefaultEyeHeight;
        }
        localCC = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        if (hardwareHead == null) return;

        if (!isOnlineMode && !isLocalScaleCalibrated && hardwareHead.localPosition.y > 0.5f)
        {
            float currentHmdHeight = hardwareHead.localPosition.y;
            float scaleRatio = currentHmdHeight / defaultAvatarHeight;
            if (avatarRoot != null)
            {
                avatarRoot.localScale = Vector3.one * Mathf.Clamp(scaleRatio, 0.7f, 1.4f);
                isLocalScaleCalibrated = true;
            }
        }

        // 플레이어 캐릭터 컨트롤러 콜라이더 높이 조절
        if (localCC != null)
        {
            float targetHeight = hardwareHead.localPosition.y;
            targetHeight = Mathf.Clamp(targetHeight, 0.5f, 2.5f);

            localCC.height = targetHeight;
            localCC.center = new Vector3(localCC.center.x, targetHeight / 2f, localCC.center.z);
        }

        // 온라인 모드가 아닐 때(로컬/오프라인 테스트)만 최소한의 트래킹 매핑을 수행합니다.
        if (!isOnlineMode)
        {
            if (avatarRoot != null && avatarHead != null)
            {
                SynchronizeTransformsLocal();
            }
        }

        UpdateAnimation();
    }

    // 오프라인 상태에서도 아바타가 깨지지 않도록 회전과 손 위치만 매핑 (스케일 조절 전면 삭제)
    void SynchronizeTransformsLocal()
    {
        // 1. 몸통 회전
        Vector3 headForward = hardwareHead.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude > 0.01f)
            avatarRoot.rotation = Quaternion.LookRotation(headForward);

        // 2. 위치 동기화
        Vector3 alignedRootPosition = new Vector3(hardwareHead.position.x, transform.position.y, hardwareHead.position.z);
        Vector3 dynamicCrouchOffset = avatarRoot.forward * (currentCrouch * crouchRate);
        avatarRoot.position = alignedRootPosition + avatarRoot.TransformDirection(centerPositionOffset) + dynamicCrouchOffset;

        // 3. 머리 회전 및 양손 IK 타겟 위치 동기화
        avatarHead.rotation = hardwareHead.rotation;

        if (avatarLeftHand != null && hardwareLeftHand != null)
        {
            avatarLeftHand.position = hardwareLeftHand.position;
            avatarLeftHand.rotation = hardwareLeftHand.rotation * Quaternion.Euler(new Vector3(30, 0, 0));
        }
        if (avatarRightHand != null && hardwareRightHand != null)
        {
            avatarRightHand.position = hardwareRightHand.position;
            avatarRightHand.rotation = hardwareRightHand.rotation * Quaternion.Euler(new Vector3(30, 0, 0));
        }
    }

    void UpdateAnimation()
    {
        if (animator == null || avatarRoot == null) return;

        Vector3 currentPos = new Vector3(avatarRoot.position.x, 0, avatarRoot.position.z);
        Vector3 prevPos = new Vector3(previousPosition.x, 0, previousPosition.z);
        Vector3 velocity = (currentPos - prevPos) / Time.deltaTime;
        previousPosition = avatarRoot.position;

        Vector3 localVelocity = avatarRoot.InverseTransformDirection(velocity);
        float maxWalkSpeed = 2.0f;
        float targetX = Mathf.Clamp(localVelocity.x / maxWalkSpeed, -1f, 1f);
        float targetZ = Mathf.Clamp(localVelocity.z / maxWalkSpeed, -1f, 1f);

        if (Mathf.Abs(targetX) < 0.05f && Mathf.Abs(targetZ) < 0.05f)
        {
            currentMoveX = Mathf.Lerp(currentMoveX, 0f, Time.deltaTime * 20f);
            currentMoveZ = Mathf.Lerp(currentMoveZ, 0f, Time.deltaTime * 20f);
        }
        else
        {
            currentMoveX = Mathf.Lerp(currentMoveX, targetX, Time.deltaTime * animationSmoothness);
            currentMoveZ = Mathf.Lerp(currentMoveZ, targetZ, Time.deltaTime * animationSmoothness);
        }
        animator.SetFloat("MoveX", currentMoveX);
        animator.SetFloat("MoveZ", currentMoveZ);

        float currentHmdHeight = hardwareHead.localPosition.y;

        if (!isHeightCalibrated && currentHmdHeight > 0.5f)
        {
            calibratedStandingHeight = currentHmdHeight;
            isHeightCalibrated = true;
            Debug.Log($"[VR 캘리브레이션 완료] 플레이어 기준 키: {calibratedStandingHeight}m");
        }

        float targetCrouchHeight = calibratedStandingHeight * crouchActivationRatio;

        // 내 현재 높이를 기준으로 0~1 사이값 계산
        currentCrouch = Mathf.InverseLerp(calibratedStandingHeight, targetCrouchHeight, currentHmdHeight);

        // 애니메이터에 안전하게 전달
        animator.SetFloat("Crouch", Mathf.Clamp01(currentCrouch));
    }
}