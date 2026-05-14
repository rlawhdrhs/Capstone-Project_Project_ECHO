using System.Collections;
using UnityEngine;

public class DoorMoveDown : MonoBehaviour
{
    [Header("Move")]
    public float startLocalY = 4f;
    public float targetLocalY = 1.3f;
    public float moveDuration = 5f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip moveSound;

    void Start()
    {
        StartCoroutine(MoveDoor());
    }

    IEnumerator MoveDoor()
    {
        Vector3 startPos = transform.localPosition;
        startPos.y = startLocalY;

        Vector3 targetPos = transform.localPosition;
        targetPos.y = targetLocalY;

        transform.localPosition = startPos;

        if (audioSource != null && moveSound != null)
        {
            audioSource.clip = moveSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.localPosition = targetPos;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        Debug.Log("문 최종 Local Y값: " + transform.localPosition.y);
    }
}