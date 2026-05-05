using UnityEngine;

public class LocalVRRig : MonoBehaviour
{
    public static LocalVRRig Instance;
    private void Awake() => Instance = this;

    [Header("실제 VR 하드웨어 (XR Origin 하위 요소들)")]
    public Transform hardwareHead;
    public Transform hardwareLeftHand;
    public Transform hardwareRightHand;

    [Header("내 아바타 요소 (FBX 모델의 뼈대들)")]
    public Transform avatarRoot;    // 캐릭터 최상단
    public Transform avatarHead;    // 머리 뼈대
    public Transform avatarLeftHand;  // 손 뼈대 또는 IK 타겟
    public Transform avatarRightHand; // 손 뼈대 또는 IK 타겟

    [Header("설정")]
    public Animator animator;
    public Vector3 centerPositionOffset; // 캐릭터가 바닥에 꽂히면 Y값을 조절

    private Vector3 previousPosition;

    [Header("아바타 키 설정")]
    public float standingHeight = 1.7f;
    public float crouchingHeight = 1.0f;
    public float currentCrouch;

    [Header("애니메이션 보정")]
    public float animationSmoothness = 10f;
    private float currentMoveX;
    private float currentMoveZ;

    void Start()
    {
        if (avatarRoot != null) previousPosition = avatarRoot.position;
    }

    void LateUpdate()
    {
        if (hardwareHead == null || avatarRoot == null || avatarHead == null) return;
        if (hardwareHead.position == Vector3.zero) return;
        // 애니메이션 파라미터 업데이트
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // 1. Y축 움직임 무시
        Vector3 currentPos = new Vector3(avatarRoot.position.x, 0, avatarRoot.position.z);
        Vector3 prevPos = new Vector3(previousPosition.x, 0, previousPosition.z);

        Vector3 velocity = (currentPos - prevPos) / Time.deltaTime;
        previousPosition = avatarRoot.position;

        Vector3 localVelocity = avatarRoot.InverseTransformDirection(velocity);

        // 2. 속도 정규화
        float maxWalkSpeed = 2.0f;
        float targetX = Mathf.Clamp(localVelocity.x / maxWalkSpeed, -1f, 1f);
        float targetZ = Mathf.Clamp(localVelocity.z / maxWalkSpeed, -1f, 1f);

        // 3. 강제 브레이크
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
}