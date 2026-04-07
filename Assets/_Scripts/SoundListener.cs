using System.Collections.Generic;
using UnityEngine;

public class SoundListener : MonoBehaviour
{
    public float hearingThreshold = 0.2f;
    public LayerMask obstacleMask;
    public float occlusionMultiplier = 0.6f;

    private HashSet<int> processedSoundIds = new HashSet<int>();

    void Update()
    {
        if (SoundManager.Instance == null) return;

        foreach (SoundData sound in SoundManager.Instance.soundEvents)
        {
            if (processedSoundIds.Contains(sound.id))
                continue;

            float distance = Vector3.Distance(transform.position, sound.position);

            // 거리 감쇠
            float distanceFactor = 1f / (1f + distance * 0.3f);

            // 차폐 감쇠
            float occlusionFactor = 1f;
            if (Physics.Linecast(sound.position, transform.position, obstacleMask))
            {
                occlusionFactor = occlusionMultiplier;
            }

            // 최종 들리는 강도
            float perceivedIntensity = sound.intensity * distanceFactor * occlusionFactor;

            if (perceivedIntensity >= hearingThreshold)
            {
                if (sound.soundType == SoundType.Footstep)
                {
                    Debug.Log(gameObject.name + " 발소리 감지됨: " + sound.position + " | Sound ID: " + sound.id);
                }
                else if (sound.soundType == SoundType.Collision)
                {
                    Debug.Log("충돌음 감지");
                }

                processedSoundIds.Add(sound.id);
            }
        }
    }
}