using Fusion;
using UnityEngine;

public class SoundEmitter_Network : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 0.8f;
    public float soundLifetime = 0.3f;
    public float footstepYOffset = -1f;

    [Header("Meta XR Audio Acoustic Fix")]
    public LayerMask groundLayers;
    public float groundSurfaceOffset = 0.05f;
    public float maxGroundCheckDistance = 1.8f;

    [Header("State")]
    public bool isRunning = false;

    private bool isOverridden = false;
    private SoundType overriddenSoundType;

    private Vector3 lastPosition;
    private float accumulatedDistance;

    public override void Spawned()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RegisterInfiltrator(Object);
        }

        if (Object.HasInputAuthority && LocalVRRig.Instance != null)
        {
            // LocalVRRig(XR_Origin)에서 링크 컴포넌트를 가져오거나 없으면 새로 붙임
            XROriginStepSoundLinker linker = LocalVRRig.Instance.GetComponent<XROriginStepSoundLinker>();
            if (linker == null)
            {
                linker = LocalVRRig.Instance.gameObject.AddComponent<XROriginStepSoundLinker>();
            }

            // 이 링크 스크립트에 자기 자신(아바타 발소리 스크립트)을 등록!
            linker.networkSoundEmitter = this;
        }
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


                float currentStepDistance = stepDistance;
                if (StealthDetector.Instance != null && StealthDetector.Instance.isStealthMode)
                {
                    currentStepDistance = stepDistance;
                }

                while (accumulatedDistance >= currentStepDistance)
                {
                    EmitFootstep(currentPosition);
                    accumulatedDistance -= currentStepDistance;
                }
            }

            lastPosition = currentPosition;
        }
    }

    void EmitFootstep(Vector3 currentPosition)
    {
        SoundType soundType;

        if (isOverridden)
        {
            soundType = overriddenSoundType;
        }
        else if (StealthDetector.Instance != null && StealthDetector.Instance.isStealthMode)
        {
            soundType = SoundType.StealthFootstep;
        }
        else
        {
            soundType = isRunning ? SoundType.RunFootstep : SoundType.WalkFootstep;
        }

        Vector3 soundPosition;

        Vector3 rayStart = currentPosition + Vector3.up * 0.1f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxGroundCheckDistance, groundLayers, QueryTriggerInteraction.Collide))
        {
            soundPosition = hit.point + Vector3.up * groundSurfaceOffset;
        }
        else
        {
            soundPosition = currentPosition + Vector3.up * footstepYOffset;
            Debug.LogWarning($"[SoundEmitter] 바닥 레이어 감지 실패! 백업 오프셋 위치로 소리를 생성합니다: {soundPosition}");
        }

        RPC_EmitSound(soundPosition, soundLifetime, soundType);
    }

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

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestStunToMe(float duration)
    {
        // 주권자인 호스트(잠입자) 컴퓨터에서만 실행됨이 100% 보장됩니다.
        Debug.Log($"📡 [호스트 수신 완료] 잠입자가 추격자로부터 {duration}초 스턴 RPC를 받았습니다!");

        if (LocalVRRig.Instance != null)
        {
            RunawayStatus runaway = LocalVRRig.Instance.GetComponent<RunawayStatus>();
            if (runaway != null)
            {
                runaway.ApplyStun(duration);
            }
            else
            {
                Debug.LogError("❌ [에러] LocalVRRig은 찾았으나 RunawayStatus 컴포넌트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError("❌ [에러] 호스트 화면에 LocalVRRig.Instance가 null입니다!");
        }
    }
}