using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CameraBGMPlayer : MonoBehaviour
{
    [Header("이 BGM을 들을 카메라")]
    public Camera targetCamera;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (targetCamera == null)
            return;

        // 카메라 활성화 상태 확인
        bool shouldPlay = targetCamera.gameObject.activeInHierarchy;

        if (shouldPlay)
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
    }
}