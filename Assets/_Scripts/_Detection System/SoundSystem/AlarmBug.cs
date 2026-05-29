using System.Collections;
using UnityEngine;

public class AlarmBug : MonoBehaviour
{
    private enum AlarmBugState
    {
        Idle,       // 대기 상태
        Warning,   // 경고음 재생 중
        Armed,     // 활성화 / 감시 상태
        Alarm,      // 큰 경보음 발생 중
        Disabled    // 완전 비활성화
    }

    [Header("Target")]
    [SerializeField] private string intruderTag = "Player";

    [Header("Detection Range")]
    [Tooltip("Armed 상태에서 실제 경보 조건을 검사하는 거리입니다. Sphere Collider Radius와 비슷하게 맞추는 것을 추천합니다.")]
    [SerializeField] private float detectionRadius = 4.0f;

    [Header("Movement Check")]
    [Tooltip("이 속도 이상으로 움직이고, 조심히 걷기 상태가 아니면 알람이 발생합니다.")]
    [SerializeField] private float minMoveSpeedToTrigger = 0.15f;

    [Header("Quiet Walk Check")]
    [Tooltip("true면 외부에서 SetQuietWalking()으로 조심히 걷기 상태를 넣어줍니다.")]
    [SerializeField] private bool useExternalQuietWalkState = true;

    [Tooltip("테스트용 조심히 걷기 상태입니다. useExternalQuietWalkState가 true일 때 사용됩니다.")]
    [SerializeField] private bool isQuietWalking = false;

    [Tooltip("선택 사항. useExternalQuietWalkState가 false일 때, HMD 높이로 조심히 걷기를 판단합니다.")]
    [SerializeField] private Transform hmdTransform;

    [Tooltip("HMD local Y가 이 값보다 낮으면 조심히 걷기 상태로 판단합니다.")]
    [SerializeField] private float quietWalkHeightThreshold = 1.2f;

    [Header("Timing")]
    [SerializeField] private float warningDuration = 0.7f;
    [SerializeField] private float alarmDuration = 2.0f;

    [Header("Warning Sound - Sensor X")]
    [Tooltip("삐리릭 경고음 전용 AudioSource입니다. 센서 감지용이 아닙니다.")]
    [SerializeField] private AudioSource localAudioSource;

    [Tooltip("감지 범위에 들어왔을 때 재생되는 짧은 경고음입니다.")]
    [SerializeField] private AudioClip warningClip;

    [Header("Alarm Sound - Sensor O")]
    [Tooltip("SoundManager에 등록되어 있어야 하는 SoundType입니다.")]
    [SerializeField] private SoundType alarmSoundType = SoundType.AlarmBugBeep;

    [Tooltip("SoundManager soundEvents에 유지될 시간입니다.")]
    [SerializeField] private float alarmSoundLifetime = 2.0f;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0f);
    [SerializeField] private Color armedColor = Color.yellow;
    [SerializeField] private Color alarmColor = Color.red;
    [SerializeField] private Color disabledColor = Color.gray;

    [Header("Disable Option")]
    [SerializeField] private bool hideAfterDisabled = false;
    [SerializeField] private Collider triggerCollider;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private AlarmBugState currentState = AlarmBugState.Idle;

    private Transform intruder;
    private Vector3 lastIntruderPosition;
    private bool hasLastPosition = false;

    private Coroutine stateRoutine;

    private void Reset()
    {
        localAudioSource = GetComponent<AudioSource>();
        triggerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Awake()
    {
        if (localAudioSource == null)
        {
            localAudioSource = GetComponent<AudioSource>();
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        SetState(AlarmBugState.Idle);
    }

    private void Update()
    {
        if (currentState != AlarmBugState.Armed)
        {
            return;
        }

        if (intruder == null)
        {
            return;
        }

        if (IsQuietWalking())
        {
            if (showDebugLog) Debug.Log("[AlarmBug] 잠입자가 다시 숨었습니다. 대기(Idle) 상태로 복귀합니다.");
            ReturnToIdle();
            return;
        }

        float distance = Vector3.Distance(transform.position, intruder.position);

        if (distance > detectionRadius)
        {
            return;
        }

        float moveSpeed = GetIntruderMoveSpeed();
        bool quiet = IsQuietWalking();

        if (showDebugLog)
        {
            Debug.Log(
                $"[AlarmBug] Armed / " +
                $"Distance: {distance:F2}, " +
                $"MoveSpeed: {moveSpeed:F2}, " +
                $"Quiet: {quiet}"
            );
        }

        if (moveSpeed >= minMoveSpeedToTrigger && !quiet)
        {
            TriggerAlarm();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLog)
        {
            Debug.Log($"[AlarmBug] Trigger entered by: {other.name}, Tag: {other.tag}");
        }

        if (!other.CompareTag(intruderTag))
        {
            if (showDebugLog)
            {
                Debug.Log($"[AlarmBug] Ignored object. Expected Tag: {intruderTag}, Actual Tag: {other.tag}");
            }

            return;
        }

        if (IsQuietWalking())
        {
            if (showDebugLog) Debug.Log("[AlarmBug] 무언가 지나갔으나 조용히 걸어서 감지하지 못했습니다.");
            return;
        }

        if (currentState == AlarmBugState.Disabled)
        {
            return;
        }

        if (currentState != AlarmBugState.Idle)
        {
            if (showDebugLog)
            {
                Debug.Log($"[AlarmBug] Intruder entered, but state is not Idle. Current State: {currentState}");
            }

            return;
        }

        intruder = other.transform;
        lastIntruderPosition = intruder.position;
        hasLastPosition = true;

        if (showDebugLog)
        {
            Debug.Log($"[AlarmBug] Intruder detected. Starting warning. Intruder: {intruder.name}");
        }

        StartWarning();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(intruderTag))
        {
            return;
        }

        if (currentState == AlarmBugState.Disabled)
        {
            return;
        }

        if (currentState == AlarmBugState.Idle && !IsQuietWalking())
        {
            intruder = other.transform;
            lastIntruderPosition = intruder.position;
            hasLastPosition = true;

            if (showDebugLog) Debug.Log("[AlarmBug] 범위 안에서 잠입자가 일어난 것을 감지했습니다! 경고 시작.");
            StartWarning();
            return;
        }

        intruder = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(intruderTag))
        {
            return;
        }

        intruder = null;
        hasLastPosition = false;

        if (currentState == AlarmBugState.Warning || currentState == AlarmBugState.Armed)
        {
            ReturnToIdle();
        }
    }

    private void StartWarning()
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        stateRoutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        SetState(AlarmBugState.Warning);

        if (localAudioSource != null && warningClip != null)
        {
            localAudioSource.PlayOneShot(warningClip);
        }
        else if (showDebugLog)
        {
            Debug.LogWarning("[AlarmBug] Warning AudioSource or Warning Clip is missing.");
        }

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Warning sound played.");
        }

        yield return new WaitForSeconds(warningDuration);

        if (currentState == AlarmBugState.Disabled)
        {
            yield break;
        }

        if (intruder == null || IsQuietWalking())
        {
            ReturnToIdle();
            yield break;
        }

        lastIntruderPosition = intruder.position;
        hasLastPosition = true;

        SetState(AlarmBugState.Armed);

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Armed. Waiting for loud movement.");
        }

        stateRoutine = null;
    }

    private void TriggerAlarm()
    {
        if (currentState != AlarmBugState.Armed)
        {
            if (showDebugLog)
            {
                Debug.Log($"[AlarmBug] TriggerAlarm called, but current state is {currentState}");
            }

            return;
        }

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Alarm condition met. Triggering alarm.");
        }

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }

        stateRoutine = StartCoroutine(AlarmRoutine());
    }

    private IEnumerator AlarmRoutine()
    {
        SetState(AlarmBugState.Alarm);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(transform.position, alarmSoundLifetime, alarmSoundType);

            if (showDebugLog)
            {
                Debug.Log(
                    $"[AlarmBug] EmitSound called. " +
                    $"Type: {alarmSoundType}, " +
                    $"Position: {transform.position}, " +
                    $"Lifetime: {alarmSoundLifetime}"
                );
            }
        }
        else
        {
            Debug.LogWarning("[AlarmBug] SoundManager.Instance is missing. Alarm sound was not registered.");
        }

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Alarm sound emitted. Sensor can detect this sound.");
        }

        yield return new WaitForSeconds(alarmDuration);

        DisableBug();

        stateRoutine = null;
    }

    private void ReturnToIdle()
    {
        if (currentState == AlarmBugState.Disabled || currentState == AlarmBugState.Alarm)
        {
            return;
        }

        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        intruder = null;
        hasLastPosition = false;

        SetState(AlarmBugState.Idle);

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Intruder left safely. Back to idle.");
        }
    }

    private void DisableBug()
    {
        SetState(AlarmBugState.Disabled);

        intruder = null;
        hasLastPosition = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (localAudioSource != null)
        {
            localAudioSource.Stop();
        }

        if (hideAfterDisabled && renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                {
                    r.enabled = false;
                }
            }
        }

        if (showDebugLog)
        {
            Debug.Log("[AlarmBug] Disabled permanently.");
        }
    }

    private float GetIntruderMoveSpeed()
    {
        if (intruder == null)
        {
            return 0f;
        }

        if (!hasLastPosition)
        {
            lastIntruderPosition = intruder.position;
            hasLastPosition = true;
            return 0f;
        }

        float distance = Vector3.Distance(intruder.position, lastIntruderPosition);
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);

        lastIntruderPosition = intruder.position;

        return speed;
    }

    private bool IsQuietWalking()
    {
        if (useExternalQuietWalkState)
        {
            if (StealthDetector.Instance != null)
            {
                return StealthDetector.Instance.isStealthMode;
            }
            return isQuietWalking;
        }

        if (hmdTransform != null)
        {
            return hmdTransform.localPosition.y <= quietWalkHeightThreshold;
        }

        return false;
    }

    public void SetQuietWalking(bool value)
    {
        isQuietWalking = value;

        if (showDebugLog)
        {
            Debug.Log($"[AlarmBug] Quiet walking set to: {isQuietWalking}");
        }
    }

    private void SetState(AlarmBugState newState)
    {
        currentState = newState;
        ApplyVisualState(newState);

        if (showDebugLog)
        {
            Debug.Log($"[AlarmBug] State changed to: {currentState}");
        }
    }

    private void ApplyVisualState(AlarmBugState state)
    {
        if (renderers == null)
        {
            return;
        }

        Color targetColor = idleColor;

        switch (state)
        {
            case AlarmBugState.Idle:
                targetColor = idleColor;
                break;

            case AlarmBugState.Warning:
                targetColor = warningColor;
                break;

            case AlarmBugState.Armed:
                targetColor = armedColor;
                break;

            case AlarmBugState.Alarm:
                targetColor = alarmColor;
                break;

            case AlarmBugState.Disabled:
                targetColor = disabledColor;
                break;
        }

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;

            foreach (Material mat in r.materials)
            {
                if (mat != null)
                {
                    mat.color = targetColor;

                    mat.EnableKeyword("_EMISSION");

                    mat.SetColor("_EmissionColor", targetColor * 2.0f);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}