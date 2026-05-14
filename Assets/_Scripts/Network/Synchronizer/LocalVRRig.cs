using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
    public float crouchingHeight = 1.0f;
    public float currentCrouch;

    [Header("애니메이션 보정")]
    public float animationSmoothness = 10f;
    private float currentMoveX;
    private float currentMoveZ;

    public bool isOnlineMode = false;

    [Header("아바타 캘리브레이션 (키 맞춤)")]
    public float avatarDefaultEyeHeight = 1.7f;

    private float defaultAvatarHeight;

    private CharacterController localCC;
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

        if (!isOnlineMode)
        {
            if (avatarRoot != null && avatarHead != null)
            {
                SynchronizeTransforms();
            }
        }

        UpdateAnimation();

        if (localCC != null)
        {
            // 캡슐 높이가 0.5 미만으로 찌그러지면 강제로 복구
            if (localCC.height < 0.5f)
            {
                localCC.height = 0.5f;
                localCC.center = new Vector3(localCC.center.x, 0.25f, localCC.center.z); // 중심점도 높이의 절반으로 맞춰줌
            }
        }
    }

    void SynchronizeTransforms()
    {
        // 1. 몸통 회전
        Vector3 headForward = hardwareHead.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude > 0.01f)
            avatarRoot.rotation = Quaternion.LookRotation(headForward);

        // 2. 키 측정 및 스케일 적용
        float currentHmdHeight = hardwareHead.position.y - transform.position.y;
        if (currentHmdHeight < 0.5f) currentHmdHeight = avatarDefaultEyeHeight;

        float scaleRatio = currentHmdHeight / defaultAvatarHeight;
        scaleRatio = Mathf.Clamp(scaleRatio, 0.5f, 1.5f);
        avatarRoot.localScale = Vector3.one * scaleRatio;

        Vector3 headOffset = hardwareHead.position - avatarHead.position;
        avatarRoot.position += headOffset;

        // 추가 오프셋 적용
        avatarRoot.position += avatarRoot.TransformDirection(centerPositionOffset);

        // 4. 머리 회전 및 손 동기화
        avatarHead.rotation = hardwareHead.rotation;

        if (avatarLeftHand != null && hardwareLeftHand != null)
        {
            avatarLeftHand.position = hardwareLeftHand.position;
            avatarLeftHand.rotation = hardwareLeftHand.rotation;
        }
        if (avatarRightHand != null && hardwareRightHand != null)
        {
            avatarRightHand.position = hardwareRightHand.position;
            avatarRightHand.rotation = hardwareRightHand.rotation;
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

        float currentScale = avatarRoot.localScale.y;
        float scaledStandingHeight = standingHeight * currentScale;
        float scaledCrouchingHeight = crouchingHeight * currentScale;

        // 로컬 포지션이 아니라 HMD의 실제 높이 사용
        float currentHmdHeight = hardwareHead.position.y - transform.position.y;

        currentCrouch = Mathf.InverseLerp(scaledStandingHeight, scaledCrouchingHeight, currentHmdHeight);
        animator.SetFloat("Crouch", currentCrouch);
    }
}