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

        // =========================================================
        // 3. [질문자님 아이디어 완벽 적용] 머리를 카메라에 정확히 맞춤!
        // =========================================================
        // Y축(높이)을 무시하지 않고, X, Y, Z 모든 오차를 다 더해서 
        // 아바타 머리를 카메라 위치로 강제로 텔레포트(이동) 시킵니다.
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

        // ... (MoveX, MoveZ 계산하는 위쪽 코드는 기존과 동일하게 유지) ...
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

        // =========================================================
        // [중요 Fix] 스케일에 맞춰서 애니메이터의 Crouch 기준점도 수정
        // =========================================================
        // 아바타 스케일이 0.7배로 줄었다면, 서 있는 키 1.7m도 0.7배 줄여서 계산해야 합니다!
        float currentScale = avatarRoot.localScale.y;
        float scaledStandingHeight = standingHeight * currentScale;
        float scaledCrouchingHeight = crouchingHeight * currentScale;

        // 로컬 포지션이 아니라 HMD의 실제 높이 사용
        float currentHmdHeight = hardwareHead.position.y - transform.position.y;

        // 내 키에 맞춰진 새로운 기준값으로 Crouch 계산
        currentCrouch = Mathf.InverseLerp(scaledStandingHeight, scaledCrouchingHeight, currentHmdHeight);
        animator.SetFloat("Crouch", currentCrouch);
    }

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