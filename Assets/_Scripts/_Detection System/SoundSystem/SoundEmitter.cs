using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public float stepDistance = 1.5f;
    public float baseSoundIntensity = 1f;
    public float soundLifetime = 0.3f;

    private Vector3 lastPosition;
    private float accumulatedDistance;

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
                EmitSound(currentPosition, movedDistance);
                accumulatedDistance -= stepDistance;
            }
        }

        lastPosition = currentPosition;
    }

    void EmitSound(Vector3 currentPosition, float movedDistance)
    {
        Vector3 soundPos = currentPosition + Vector3.down * 1f;

        float speed = movedDistance / Time.deltaTime;
        float soundIntensity = baseSoundIntensity + speed * 0.1f;

        Debug.Log("소리 생성됨: " + soundPos + " | 강도: " + soundIntensity);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.RegisterSound(soundPos, soundIntensity, soundLifetime, SoundType.Footstep);
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null임");
        }
    }
}