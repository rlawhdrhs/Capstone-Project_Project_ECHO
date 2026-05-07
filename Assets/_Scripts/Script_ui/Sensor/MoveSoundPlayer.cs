using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MoveSoundPlayer : MonoBehaviour
{
    [Header("이동 감지 최소 거리")]
    public float moveThreshold = 0.05f;

    private Vector3 lastPosition;
    private AudioSource audioSource;

    void Start()
    {
        lastPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;

        // Y 제외하고 X,Z만 비교
        Vector2 lastXZ = new Vector2(lastPosition.x, lastPosition.z);
        Vector2 currentXZ = new Vector2(currentPosition.x, currentPosition.z);

        float distance = Vector2.Distance(lastXZ, currentXZ);

        // 일정 거리 이상 움직였을 때
        if (distance > moveThreshold)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        lastPosition = currentPosition;
    }
}