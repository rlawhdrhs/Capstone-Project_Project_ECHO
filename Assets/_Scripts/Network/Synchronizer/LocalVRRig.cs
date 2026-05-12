using System.Security.Cryptography;
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
    void Start()
    {
        if (avatarRoot != null) previousPosition = avatarRoot.position;

        if (avatarRoot != null && avatarHead != null)
        {
            defaultAvatarHeight = avatarHead.position.y - avatarRoot.position.y;

            if (defaultAvatarHeight < 1.0f) defaultAvatarHeight = avatarDefaultEyeHeight;
        }
    }

    void LateUpdate()
    {
        if (hardwareHead == null) return;

        if (isOnlineMode)
        {
            if (avatarRoot != null)
            {
                Vector3 targetPos = avatarRoot.position;

                targetPos.x -= hardwareHead.localPosition.x;
                targetPos.z -= hardwareHead.localPosition.z;

                transform.position = targetPos;
            }
        }
        else
        {
            // 오프라인일 때만 카메라가 몸통을 끌고 다님
            if (avatarRoot != null && avatarHead != null)
            {
                SynchronizeTransforms();
            }
        }

        UpdateAnimation();
    }

    void SynchronizeTransforms()
    {
        // 1. 몸통 회전 (머리가 바라보는 방향)
        Vector3 headForward = hardwareHead.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude > 0.01f)
            avatarRoot.rotation = Quaternion.LookRotation(headForward);

        Vector3 targetRootPosition = hardwareHead.position;
        targetRootPosition.y = transform.position.y; // XR Origin(플레이어 몸통)의 바닥 높이로 고정
        avatarRoot.position = targetRootPosition + centerPositionOffset;

        // B. 내 실제 머리 높이 측정 (로컬 바닥 기준)
        float currentHmdHeight = hardwareHead.localPosition.y;
        if (currentHmdHeight < 0.5f) currentHmdHeight = avatarDefaultEyeHeight;

        // C. 스케일 계산: (현재 내 실제 키 / 아바타의 원래 키)
        // 예를 들어 내 HMD가 1.2m고 아바타가 1.7m면, 아바타 크기를 약 0.7배로 줄입니다.
        float scaleRatio = currentHmdHeight / defaultAvatarHeight;
        scaleRatio = Mathf.Clamp(scaleRatio, 0.5f, 1.5f); // 너무 작아지거나 커지는 것 방지

        // D. 아바타에 스케일 적용 (발은 바닥에 고정되어 있으므로, 키만 줄어듭니다)
        avatarRoot.localScale = Vector3.one * scaleRatio;


        // 3. 머리 회전 및 손 동기화
        avatarHead.rotation = hardwareHead.rotation; // 위치는 스케일로 맞춰졌으니 회전만 적용

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

        float currentHeadHeight = hardwareHead.localPosition.y;
        currentCrouch = Mathf.InverseLerp(standingHeight, crouchingHeight, currentHeadHeight);
        animator.SetFloat("Crouch", currentCrouch);
    }

    //public void CalibrateAvatarHeight(VRRigSynchronizer syncScript)
    //{
    //    if (hardwareHead == null) return;

    //    // 1. 현재 사용자의 실제 머리(HMD) 높이 가져오기 (방바닥 기준)
    //    float currentHmdHeight = hardwareHead.localPosition.y;

    //    // 만약 기기 인식이 안 되어 높이가 0 근처라면 기본값 유지
    //    if (currentHmdHeight < 0.5f) currentHmdHeight = avatarDefaultEyeHeight;

    //    // 2. 스케일 비율 계산 = 내 실제 키 / 아바타의 원래 키
    //    float scaleRatio = currentHmdHeight / avatarDefaultEyeHeight;

    //    scaleRatio = Mathf.Clamp(scaleRatio, 0.5f, 1.5f);

    //    // 3. 내 화면에서 즉시 아바타 크기 적용
    //    if (avatarRoot != null)
    //    {
    //        avatarRoot.localScale = Vector3.one * scaleRatio;
    //    }

    //    // 4. 이 크기를 서버에 전송하여 남들에게도 동기화
    //    if (syncScript != null)
    //    {
    //        syncScript.RPC_SetAvatarScale(scaleRatio);
    //    }

    //    Debug.Log($"[키 맞춤 완료] 실제 HMD 높이: {currentHmdHeight}m, 아바타 스케일: {scaleRatio}배");
}