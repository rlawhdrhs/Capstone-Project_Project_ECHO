using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [System.Serializable]
    public class SoundIntensityEntry
    {
        public SoundType soundType;
        public float intensity = 1f;
    }

    [Header("Sound Intensity Settings")]
    [SerializeField] private List<SoundIntensityEntry> intensityTable;

    private int nextSoundId = 0;
    public List<SoundData> soundEvents = new List<SoundData>();


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

    public float GetBaseIntensity(SoundType type)
    {
        foreach (var entry in intensityTable)
        {
            if (entry.soundType == type)
                return entry.intensity;
        }

        return 0.5f;
    }

    public void RegisterSound(Vector3 position, float lifetime, SoundType soundType)
    {
        float intensity = GetBaseIntensity(soundType);

        SoundData data = new SoundData(
            nextSoundId++,
            position,
            intensity,
            lifetime,
            soundType
        );
        
        soundEvents.Add(data);
    }
}