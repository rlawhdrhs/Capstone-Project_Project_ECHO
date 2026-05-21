using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VRMovementVignette : MonoBehaviour
{
    [Header("Volume")]
    public Volume globalVolume;

    [Header("Player Movement")]
    public Transform playerRoot; // XR Origin, Player, CameraPivot, Main Camera 중 하나

    [Header("Vignette Settings")]
    public float moveThreshold = 0.01f;
    public float rotateThreshold = 0.3f;

    //[Range(0f, 0.4f)]
    public float maxIntensity = 0.6f;

    //[Range(0f, 1f)]
    public float smoothness = 0.4f;

    public float fadeSpeed = 1f;
    public float holdTime = 0.2f; // 움직임 감지 후 유지 시간
    private float movementTimer = 0f;


    private Vignette vignette;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float targetIntensity = 0f;

    void Start()
    {
        if (playerRoot == null)
            playerRoot = transform;

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
            Debug.LogWarning("Vignette를 찾지 못했습니다. Global Volume에 Vignette Override가 있는지 확인하세요.");
        }

        lastPosition = playerRoot.position;
        lastRotation = playerRoot.rotation;
    }

    void Update()
    {
        if (vignette == null || playerRoot == null) return;

        float movedDistance = Vector3.Distance(playerRoot.position, lastPosition);
        float rotatedAngle = Quaternion.Angle(playerRoot.rotation, lastRotation);

        bool isMoving = movedDistance > moveThreshold;
        bool isRotating = rotatedAngle > rotateThreshold;

        if (isMoving || isRotating)
        {
            movementTimer = holdTime;
        }
        else
        {
            movementTimer -= Time.deltaTime;
        }

        targetIntensity = movementTimer > 0f ? maxIntensity : 0f;

        //targetIntensity = (isMoving || isRotating) ? maxIntensity : 0f;

        vignette.smoothness.value = smoothness;

        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetIntensity,
            Time.deltaTime * fadeSpeed
        );

        lastPosition = playerRoot.position;
        lastRotation = playerRoot.rotation;
    }
}