using System.Collections.Generic;
using UnityEngine;

public class SoundListener : MonoBehaviour
{
    public float baseHearingRange = 2f;

    private HashSet<int> processedSoundIds = new HashSet<int>();

    void Update()
    {
        if (SoundManager.Instance == null) return;

        foreach (SoundData sound in SoundManager.Instance.soundEvents)
        {
            if (processedSoundIds.Contains(sound.id))
                continue;

            float distance = Vector3.Distance(transform.position, sound.position);
            float effectiveRange = baseHearingRange * sound.intensity;

            if (distance <= effectiveRange)
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