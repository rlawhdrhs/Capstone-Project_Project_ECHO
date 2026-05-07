using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpatialSoundPlayer : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1f;
        audioSource.spatializePostEffects = false;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        // 기본값. 실제 값은 Play()에서 SoundType별로 덮어씀.
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 30f;
    }

    public void Play(
        AudioClip clip,
        float volume = 1f,
        float minDistance = 2f,
        float maxDistance = 30f,
        AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic,
        float pitchMin = 0.95f,
        float pitchMax = 1.05f
    )
    {
        if (clip == null)
        {
            Destroy(gameObject);
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.rolloffMode = rolloffMode;
        audioSource.pitch = Random.Range(pitchMin, pitchMax);

        audioSource.Play();

        Destroy(gameObject, clip.length + 0.2f);
    }
}