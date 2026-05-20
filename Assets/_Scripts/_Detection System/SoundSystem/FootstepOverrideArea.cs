using UnityEngine;

public class FootstepOverrideArea : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";

    [Header("Override Sound")]
    [SerializeField] private SoundType overrideSoundType = SoundType.GlassStep;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        SoundEmitter soundEmitter = other.GetComponent<SoundEmitter>();

        if (soundEmitter == null)
        {
            soundEmitter = other.GetComponentInParent<SoundEmitter>();
        }

        if (soundEmitter == null)
        {
            Debug.LogWarning("[FootstepOverrideArea] SoundEmitter를 찾을 수 없음");
            return;
        }

        soundEmitter.SetFootstepOverride(overrideSoundType);

        if (showDebugLog)
        {
            Debug.Log($"[FootstepOverrideArea] Entered. Override: {overrideSoundType}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        SoundEmitter soundEmitter = other.GetComponent<SoundEmitter>();

        if (soundEmitter == null)
        {
            soundEmitter = other.GetComponentInParent<SoundEmitter>();
        }

        if (soundEmitter == null)
        {
            return;
        }

        soundEmitter.ClearFootstepOverride();

        if (showDebugLog)
        {
            Debug.Log("[FootstepOverrideArea] Exited. Override cleared.");
        }
    }
}