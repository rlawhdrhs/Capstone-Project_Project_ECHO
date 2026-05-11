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

    void Start()
    {
        if (avatarRoot != null) previousPosition = avatarRoot.position;
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
        Vector3 finalBodyPosition = hardwareHead.position;
        finalBodyPosition.y = transform.position.y;
        avatarRoot.position = finalBodyPosition + centerPositionOffset;

        Vector3 headForward = hardwareHead.forward;
        headForward.y = 0f;
        if (headForward != Vector3.zero)
            avatarRoot.rotation = Quaternion.LookRotation(headForward);

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
}