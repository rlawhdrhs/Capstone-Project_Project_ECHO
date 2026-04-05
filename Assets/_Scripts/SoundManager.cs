using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public List<SoundEventData> soundEvents = new List<SoundEventData>();
    public float soundLifeTime = 0.3f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("SoundManager Awake 성공");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        soundEvents.RemoveAll(sound => Time.time - sound.timeCreated > soundLifeTime);
    }

    public void RegisterSound(Vector3 position, float intensity)
    {
        soundEvents.Add(new SoundEventData(position, intensity, Time.time));
        Debug.Log("SoundManager에 소리 등록됨: " + position);
    }
}

public struct SoundEventData
{
    public Vector3 position;
    public float intensity;
    public float timeCreated;

    public SoundEventData(Vector3 position, float intensity, float timeCreated)
    {
        this.position = position;
        this.intensity = intensity;
        this.timeCreated = timeCreated;
    }
}