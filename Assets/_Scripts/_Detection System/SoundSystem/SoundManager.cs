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
        public float detectionRadius = 8f;
    }

    [System.Serializable]
    public class SoundClipEntry
    {
        public SoundType soundType;
        public AudioClip[] clips;

        [Header("Playback")]
        public float volume = 1f;
        public float minDistance = 2f;
        public float maxDistance = 30f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Header("Random Pitch")]
        public float pitchMin = 0.95f;
        public float pitchMax = 1.05f;
    }

    

    [Header("Sound Intensity Settings")]
    [SerializeField] private List<SoundIntensityEntry> intensityTable;

    [Header("Sound Clip Settings")]
    [SerializeField] private List<SoundClipEntry> clipTable;

    [Header("Spatial Audio")]
    [SerializeField] private SpatialSoundPlayer spatialSoundPrefab;

    [Header("2D Local UI Audio")]
    [SerializeField] private AudioSource local2DAudioSource;

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

    public void EmitSound(Vector3 position, float lifetime, SoundType soundType)
    {
        PlaySpatialSound(position, soundType);
        RegisterSound(position, lifetime, soundType);
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

    public SoundIntensityEntry GetIntensityEntry(SoundType type)
    {
        foreach (var entry in intensityTable)
        {
            if (entry.soundType == type)
                return entry;
        }

        return null;
    }
    public void RegisterSound(Vector3 position, float lifetime, SoundType soundType)
    {
        SoundIntensityEntry entry = GetIntensityEntry(soundType);

        float intensity = entry != null ? entry.intensity : 0.5f;
        float detectionRadius = entry != null ? entry.detectionRadius : 6f;

        SoundData data = new SoundData(
            nextSoundId++,
            position,
            intensity,
            detectionRadius,
            lifetime,
            soundType
        );

        soundEvents.Add(data);
    }

    private void PlaySpatialSound(Vector3 position, SoundType soundType)
    {
        if (spatialSoundPrefab == null)
        {
            Debug.LogWarning("SpatialSoundPrefab이 SoundManager에 연결되지 않음");
            return;
        }

        SoundClipEntry entry = GetClipEntry(soundType);

        if (entry == null || entry.clips == null || entry.clips.Length == 0)
        {
            Debug.LogWarning($"SoundType {soundType}에 연결된 AudioClip이 없음");
            return;
        }

        AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];

        if (clip == null)
            return;

        SpatialSoundPlayer player = Instantiate(
            spatialSoundPrefab,
            position,
            Quaternion.identity
        );

        player.Play(
            clip,
            entry.volume,
            entry.minDistance,
            entry.maxDistance,
            entry.rolloffMode,
            entry.pitchMin,
            entry.pitchMax
        );
    }

    private SoundClipEntry GetClipEntry(SoundType type)
    {
        foreach (var entry in clipTable)
        {
            if (entry.soundType == type)
                return entry;
        }

        return null;
    }

    public SpatialSoundPlayer EmitLoopingSound(Vector3 position, SoundType soundType)
    {
        if (spatialSoundPrefab == null) return null;

        SoundClipEntry entry = GetClipEntry(soundType);
        if (entry == null || entry.clips == null || entry.clips.Length == 0) return null;

        AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];
        if (clip == null) return null;

        SpatialSoundPlayer player = Instantiate(spatialSoundPrefab, position, Quaternion.identity);

        player.Play_Loop(
            clip,
            entry.volume,
            entry.minDistance,
            entry.maxDistance,
            entry.rolloffMode,
            entry.pitchMin,
            entry.pitchMax
        );

        return player;
    }

    public void Play2DSound(AudioClip clip, float volume = 1f)
    {
        if (local2DAudioSource != null && clip != null)
        {
            local2DAudioSource.PlayOneShot(clip, volume);
        }
    }   
}