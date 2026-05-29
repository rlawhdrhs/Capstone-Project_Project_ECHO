using UnityEngine;

public class FootstepOverrideArea : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Override Sound")]
    [SerializeField] private SoundType overrideSoundType = SoundType.GlassFootstep;
    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 1. 충돌한 XR_Origin에서 연결고리(Linker)를 찾습니다.
        XROriginStepSoundLinker linker = other.GetComponent<XROriginStepSoundLinker>();
        if (linker == null || linker.networkSoundEmitter == null) return;

        // 2. 연결된 아바타의 발소리 스크립트를 가져옵니다.
        SoundEmitter_Network soundEmitter = linker.networkSoundEmitter;

        // 3. 내 로컬 캐릭터인 경우에만 오버라이드 상태를 변경합니다.
        if (soundEmitter.Runner != null && soundEmitter.Runner.IsServer)
        {
            soundEmitter.SetFootstepOverride(overrideSoundType);

            if (showDebugLog)
            {
                Debug.Log($"[Host Only] 잠입자가 구역 진입. 발소리 {overrideSoundType}로 오버라이드 됨.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        XROriginStepSoundLinker linker = other.GetComponent<XROriginStepSoundLinker>();
        if (linker == null || linker.networkSoundEmitter == null) return;

        SoundEmitter_Network soundEmitter = linker.networkSoundEmitter;

        if (soundEmitter.Runner != null && soundEmitter.Runner.IsServer)
        {
            soundEmitter.ClearFootstepOverride();

            if (showDebugLog)
            {
                Debug.Log("[Host Only] 잠입자가 구역 퇴장. 원래 발소리로 복구됨.");
            }
        }
    }
}