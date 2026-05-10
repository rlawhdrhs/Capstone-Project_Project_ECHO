using Fusion;
using UnityEngine;

public class SoundEmitter_Network : NetworkBehaviour
{
    public float stepDistance = 1.5f;
    public float baseSoundIntensity = 1f;
    public float soundLifetime = 0.3f;

    private Vector3 lastPosition;
    private float accumulatedDistance;

    public override void Spawned()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            Vector3 currentPosition = transform.position;
            float movedDistance = Vector3.Distance(currentPosition, lastPosition);

            if (movedDistance > 0.001f)
            {
                accumulatedDistance += movedDistance;
                while (accumulatedDistance >= stepDistance)
                {
                    EmitSound(currentPosition, movedDistance);
                    accumulatedDistance -= stepDistance;
                }
            }
            lastPosition = currentPosition;
        }
    }

    void EmitSound(Vector3 currentPosition, float movedDistance)
{
    Vector3 soundPos = currentPosition + Vector3.down * 1f;
    float speed = movedDistance / Runner.DeltaTime;

    RPC_PlaySound(soundPos, soundLifetime, (int)SoundType.Footstep);
}

    [Rpc(RpcSources.All, RpcTargets.All)]
public void RPC_PlaySound(Vector3 pos, float lifetime, SoundType soundType)
{
    if (SoundManager.Instance != null)
    {
        SoundManager.Instance.RegisterSound(pos, lifetime, soundType);
    }
}
}