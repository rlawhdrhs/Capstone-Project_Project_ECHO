using Fusion;
using UnityEngine;

public class SoundEmitter_Network : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 0.8f;
    public float soundLifetime = 0.3f;
    public float footstepYOffset = -1f;

    [Header("State")]
    public bool isRunning = false;

    // ★ 추가: 발소리 오버라이드 상태를 기억할 변수들
    private bool isOverridden = false;
    private SoundType overriddenSoundType;

    private Vector3 lastPosition;
    private float accumulatedDistance;

    public override void Spawned()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority && Runner.IsForward)
        {
            Vector3 currentPosition = transform.position;

            if (StealthDetector.Instance != null && StealthDetector.Instance.isStealthMode)
            {
                accumulatedDistance = 0f;
                lastPosition = currentPosition;
                return;
            }

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
    }

    void EmitFootstep(Vector3 currentPosition)
    {
        SoundType soundType;

        // ★ 변경: 만약 특정 구역(유리, 물 위 등)에 들어와서 소리가 오버라이드 되었다면 그 소리를 우선 사용
        if (isOverridden)
        {
            soundType = overriddenSoundType;
        }
        else
        {
            soundType = isRunning ? SoundType.RunFootstep : SoundType.WalkFootstep;
        }

        Vector3 soundPosition = currentPosition + Vector3.up * footstepYOffset;

        // RPC를 호출하여 나와 상대방 모두에게 소리를 내라고 명령합니다.
        RPC_EmitSound(soundPosition, soundLifetime, soundType);
    }

    // ★ 추가: 외부 구역(Trigger)에서 발소리를 변경하기 위해 호출할 함수들
    public void SetFootstepOverride(SoundType type)
    {
        isOverridden = true;
        overriddenSoundType = type;
    }

    public void ClearFootstepOverride()
    {
        isOverridden = false;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EmitSound(Vector3 position, float lifetime, SoundType soundType)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(position, lifetime, soundType);
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }
}