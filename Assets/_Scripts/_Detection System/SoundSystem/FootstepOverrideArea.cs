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

        SoundEmitter_Network networkSoundEmitter = other.GetComponent<SoundEmitter_Network>();

        if (networkSoundEmitter == null)
        {
            networkSoundEmitter = other.GetComponentInParent<SoundEmitter_Network>();
        }

        if (networkSoundEmitter == null)
        {
            Debug.LogWarning("[FootstepOverrideArea] 플레이어에게서 SoundEmitter_Network를 찾을 수 없습니다.");
            return;
        }

        // 새롭게 구현한 변경 함수 호출
        networkSoundEmitter.SetFootstepOverride(overrideSoundType);

        if (showDebugLog)
        {
            Debug.Log($"[FootstepOverrideArea] Entered. 발소리가 {overrideSoundType}로 오버라이드 됨.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        SoundEmitter_Network networkSoundEmitter = other.GetComponent<SoundEmitter_Network>();

        if (networkSoundEmitter == null)
        {
            networkSoundEmitter = other.GetComponentInParent<SoundEmitter_Network>();
        }

        if (networkSoundEmitter == null) return;

        // 오버라이드 해제 함수 호출
        networkSoundEmitter.ClearFootstepOverride();

        if (showDebugLog)
        {
            Debug.Log("[FootstepOverrideArea] Exited. 원래 발소리로 복구됨.");
        }
    }
}