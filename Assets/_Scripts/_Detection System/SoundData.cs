using UnityEngine;

[System.Serializable]
public class SoundData
{
    public int id;
    public Vector3 position;
    public float intensity;
    public float lifetime;
    public SoundType soundType;

    public SoundData(int id, Vector3 position, float intensity, float lifetime, SoundType soundType)
    {
        this.id = id;
        this.position = position;
        this.intensity = intensity;
        this.lifetime = lifetime;
        this.soundType = soundType;
    }
}