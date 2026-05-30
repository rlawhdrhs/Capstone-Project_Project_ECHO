using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 1.5f;
    public float soundLifetime = 0.3f;
    public float footstepYOffset = -1f;

    [Header("State")]
    public bool isRunning = false;

    [Header("Surface Override")]
    [SerializeField] private bool useFootstepOverride = false;
    [SerializeField] private SoundType overrideFootstepType = SoundType.WalkFootstep;

    private Vector3 lastPosition;
    private float accumulatedDistance;

    void Start()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        float movedDistance = Vector3.Distance(currentPosition, lastPosition);

        if (movedDistance > 0.001f)
        {
            accumulatedDistance += movedDistance;

            while (accumulatedDistance >= stepDistance)
            {
                EmitFootstep(currentPosition);
                accumulatedDistance -= stepDistance;
            }
        }

        lastPosition = currentPosition;
    }

    void EmitFootstep(Vector3 currentPosition)
    {
        SoundType soundType = GetCurrentFootstepSoundType();
        Vector3 soundPosition = currentPosition + Vector3.up * footstepYOffset;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(
                soundPosition,
                soundLifetime,
                soundType
            );
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }

    private SoundType GetCurrentFootstepSoundType()
    {
        if (useFootstepOverride)
        {
            return overrideFootstepType;
        }

        return isRunning ? SoundType.RunFootstep : SoundType.WalkFootstep;
    }

    public void SetFootstepOverride(SoundType soundType)
    {
        useFootstepOverride = true;
        overrideFootstepType = soundType;

        Debug.Log($"[SoundEmitter] Footstep override ON: {soundType}");
    }

    public void ClearFootstepOverride()
    {
        useFootstepOverride = false;

        Debug.Log("[SoundEmitter] Footstep override OFF");
    }

    public void EmitSound(Vector3 position, SoundType soundType)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(
                position,
                soundLifetime,
                soundType
            );
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }
}