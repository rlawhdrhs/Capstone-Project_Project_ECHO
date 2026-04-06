using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<SoundData> soundEvents = new List<SoundData>();

    private int nextSoundId = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        for (int i = soundEvents.Count - 1; i >= 0; i--)
        {
            soundEvents[i].lifetime -= Time.deltaTime;

            if (soundEvents[i].lifetime <= 0f)
            {
                soundEvents.RemoveAt(i);
            }
        }
    }

    public void RegisterSound(Vector3 position, float intensity, float lifetime, SoundType soundType)
    {
        SoundData newSound = new SoundData(nextSoundId, position, intensity, lifetime, soundType);
        nextSoundId++;

        soundEvents.Add(newSound);
    }
}