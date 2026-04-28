using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 1.5f;
    public float soundLifetime = 0.3f;

    [Header("Audio Clips")]
    public AudioClip walkClip;
    public AudioClip runClip;

    [Header("State")]
    public bool isRunning = false;

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float accumulatedDistance;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        lastPosition = transform.position;
        accumulatedDistance = 0f;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        float movedDistance = Vector3.Distance(currentPosition, lastPosition);

        if (movedDistance > 0.001f)
        {
            accumulatedDistance += movedDistance;

            while (accumulatedDistance >= stepDistance)
            {
                EmitFootstep(currentPosition);
                accumulatedDistance -= stepDistance;
            }
        }

        lastPosition = currentPosition;
    }

    void EmitFootstep(Vector3 currentPosition)
    {
        SoundType soundType = isRunning ? SoundType.RunFootstep : SoundType.WalkFootstep;
        EmitSound(currentPosition, soundType);
    }

    public void EmitSound(Vector3 position, SoundType soundType)
    {
        Vector3 soundPos = position + Vector3.down * 1f;

        PlayAudio(soundType);

        Debug.Log($"소리 생성됨: {soundPos} | 타입: {soundType}");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSound(
                soundPos,
                soundLifetime,
                soundType
            );
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }

    void PlayAudio(SoundType type)
    {
        if (audioSource == null) return;

        AudioClip clip = null;

        switch (type)
        {
            case SoundType.WalkFootstep:
                clip = walkClip;
                break;

            case SoundType.RunFootstep:
                clip = runClip;
                break;
        }

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}