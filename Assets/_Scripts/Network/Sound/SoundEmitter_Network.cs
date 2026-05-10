using Fusion;
using UnityEngine;

public class SoundEmitter_Network : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 1.5f;
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
        // 이 캐릭터를 조종하는 사람 컴퓨터에서만 거리 계산
        if (Object.HasInputAuthority)
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

    // 모든 클라이언트에서 실행되는 네트워크 함수
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_EmitSound(Vector3 position, float lifetime, SoundType soundType)
    {
        // 이 코드는 양쪽 컴퓨터에서 동시에 실행됩니다.
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