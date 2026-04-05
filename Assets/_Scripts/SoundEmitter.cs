using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public float emitInterval = 0.5f;
    public float soundIntensity = 1f;
    public GameObject soundWavePrefab;

    private Vector3 lastPosition;
    private float timer;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, lastPosition);

        if (distance > 0.001f)
        {
            timer += Time.deltaTime;

            if (timer >= emitInterval)
            {
                EmitSound();
                timer = 0f;
            }
        }
        else
        {
            timer = 0f;
        }

        lastPosition = transform.position;
    }

    void EmitSound()
    {
        Vector3 soundPos = transform.position + Vector3.down * 1f;

        Debug.Log("Sound emitted at: " + soundPos + " | Intensity: " + soundIntensity);

        if (soundWavePrefab != null)
        {
            Instantiate(soundWavePrefab, soundPos, Quaternion.identity);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSound(soundPos, soundIntensity);
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }
}