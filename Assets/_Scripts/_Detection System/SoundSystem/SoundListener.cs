using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundListener : MonoBehaviour
{
    [Header("Hearing")]
    public float hearingThreshold = 1.0f;

    [Header("Occlusion")]
    public LayerMask obstacleMask;
    public float occlusionMultiplier = 0.45f;

    public Action<SoundData, float> OnSoundDetected;

    private HashSet<int> processedSoundIds = new HashSet<int>();

    void Update()
    {
        if (SoundManager.Instance == null) return;

        foreach (SoundData sound in SoundManager.Instance.soundEvents)
        {
            if (processedSoundIds.Contains(sound.id))
                continue;

            TryDetectSound(sound);
        }
    }

    private void TryDetectSound(SoundData sound)
    {
        float distance = Vector3.Distance(transform.position, sound.position);

        if (distance > sound.detectionRadius)
            return;

        float distanceFactor = 1f - Mathf.Clamp01(distance / sound.detectionRadius);

        float occlusionFactor = 1f;

        if (Physics.Linecast(sound.position, transform.position, obstacleMask))
        {
            occlusionFactor = occlusionMultiplier;
        }

        float perceivedIntensity =
            sound.intensity *
            distanceFactor *
            occlusionFactor;

        if (perceivedIntensity >= hearingThreshold)
        {
            Debug.Log(
                $"{gameObject.name} 소리 감지: {sound.soundType}" +
                $" / 위치: {sound.position}" +
                $" / 거리: {distance:F2}" +
                $" / 거리감쇠: {distanceFactor:F2}" +
                $" / 차폐: {occlusionFactor:F2}" +
                $" / 최종강도: {perceivedIntensity:F2}"
            );

            OnSoundDetected?.Invoke(sound, perceivedIntensity);

            processedSoundIds.Add(sound.id);
        }
    }
}