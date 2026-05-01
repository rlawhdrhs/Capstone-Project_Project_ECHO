using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundListener : MonoBehaviour
{
    public float hearingThreshold = 0.05f;
    public LayerMask obstacleMask;
    public float occlusionMultiplier = 0.6f;

    public Action<SoundData, float> OnSoundDetected;

    private HashSet<int> processedSoundIds = new HashSet<int>();

    void Update()
    {
        if (SoundManager.Instance == null) return;

        foreach (SoundData sound in SoundManager.Instance.soundEvents)
        {
            if (processedSoundIds.Contains(sound.id))
                continue;

            float distance = Vector3.Distance(transform.position, sound.position);

            float distanceFactor = 1f / (1f + distance * 0.5f);

            float occlusionFactor = 1f;
            if (Physics.Linecast(sound.position, transform.position, obstacleMask))
            {
                occlusionFactor = occlusionMultiplier;
            }

            float perceivedIntensity = sound.intensity * distanceFactor * occlusionFactor;

            if (perceivedIntensity >= hearingThreshold)
            {
                Debug.Log($"{gameObject.name} 소리 감지: {sound.soundType} / 위치: {sound.position} / 강도: {perceivedIntensity}");

                OnSoundDetected?.Invoke(sound, perceivedIntensity);

                processedSoundIds.Add(sound.id);
            }
        }
    }
}