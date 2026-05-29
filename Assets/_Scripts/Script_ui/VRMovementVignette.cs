using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VRMovementVignette : MonoBehaviour
{
    [Header("Volume")]
    public Volume globalVolume;

    [Header("Player Root")]
    public Transform playerRoot;

    [Header("Vignette Speed Thresholds")]
    [Tooltip("초당 몇 미터 이상 움직일 때 켤 것인가 (m/s)")]
    public float moveSpeedThreshold = 0.2f;
    [Tooltip("초당 몇 도 이상 회전할 때 켤 것인가 (deg/s)")]
    public float rotateSpeedThreshold = 10f;

    [Header("Vignette Visual Settings")]
    public float maxIntensity = 0.6f;
    public float smoothness = 0.4f;

    [Tooltip("비네트가 깜빡이지 않고 부드럽게 켜지고 꺼지는 속도 (낮으면 너무 느림)")]
    public float fadeSpeed = 8f;
    [Tooltip("움직임이 멈춘 후 비네트를 유지할 시간")]
    public float holdTime = 0.15f;

    private Vignette vignette;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float targetIntensity = 0f;
    private float movementTimer = 0f;

    void Start()
    {
        // 명시적으로 지정하지 않았다면 에러 방지용으로 본인 지정
        if (playerRoot == null)
        {
            Debug.LogWarning("playerRoot가 비어있습니다! 제대로 된 작동을 위해 XR Origin 오브젝트를 넣어주세요.");
            playerRoot = transform;
        }

        if (globalVolume == null)
            globalVolume = FindObjectOfType<Volume>();

        if (globalVolume != null && globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;

            vignette.intensity.value = 0f;
            vignette.smoothness.value = smoothness;
        }
        else
        {
            Debug.LogWarning("Vignette를 찾지 못했습니다. Global Volume에 Vignette Override가 배치되어 있는지 확인하세요.");
        }

        lastPosition = playerRoot.position;
        lastRotation = playerRoot.rotation;
    }

    void Update()
    {
        if (vignette == null || playerRoot == null) return;

        // 1. 지난 프레임으로부터의 델타값 계산
        float movedDistance = Vector3.Distance(playerRoot.position, lastPosition);
        float rotatedAngle = Quaternion.Angle(playerRoot.rotation, lastRotation);

        // 2. [핵심 수정] 시간(Time.deltaTime)으로 나누어 프레임 레이트 독립적인 '초당 속도' 계산
        float currentMoveSpeed = movedDistance / Time.deltaTime;
        float currentRotateSpeed = rotatedAngle / Time.deltaTime;

        // 3. 설정한 속도 임계값과 비교
        bool isMoving = currentMoveSpeed > moveSpeedThreshold;
        bool isRotating = currentRotateSpeed > rotateSpeedThreshold;

        if (isMoving || isRotating)
        {
            movementTimer = holdTime;
        }
        else
        {
            movementTimer -= Time.deltaTime;
        }

        // 4. 타겟 인텐시티 설정
        targetIntensity = movementTimer > 0f ? maxIntensity : 0f;

        // 5. 비네팅 값 반영 및 Lerp (fadeSpeed를 올려서 반응 속도를 향상)
        vignette.smoothness.value = smoothness;
        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetIntensity,
            Time.deltaTime * fadeSpeed
        );

        // 6. 기준 좌표 갱신
        lastPosition = playerRoot.position;
        lastRotation = playerRoot.rotation;
    }
}