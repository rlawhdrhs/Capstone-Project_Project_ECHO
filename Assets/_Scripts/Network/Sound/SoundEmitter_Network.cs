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

            float movedDistance = Vector3.Distance(currentPosition, lastPosition);

            if (movedDistance > 0.001f)
            {
                accumulatedDistance += movedDistance;

                while (accumulatedDistance >= stepDistance)
                {
                    // 거리 조건을 만족하면 발소리를 발생
                    EmitFootstep(currentPosition);
                    accumulatedDistance -= stepDistance;
                }
            }

            lastPosition = currentPosition;
        }
    }

    void EmitFootstep(Vector3 currentPosition)
    {
        SoundType soundType = isRunning ? SoundType.RunFootstep : SoundType.WalkFootstep;
        Vector3 soundPosition = currentPosition + Vector3.up * footstepYOffset;

        // RPC를 호출하여 나와 상대방 모두에게 소리를 내라고 명령합니다.
        RPC_EmitSound(soundPosition, soundLifetime, soundType);
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