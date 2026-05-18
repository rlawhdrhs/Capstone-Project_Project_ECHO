using System.Collections;
using UnityEngine;

public class EndingClimbMove : MonoBehaviour
{
    [Header("Move")]
    public float startDelay = 0f;
    public float moveDuration = 4f;

    public float moveX = -5f;
    public float moveY = 2f;
    public float moveZ = 6f;

    [Header("Rotation")]
    public float rotateStartDelay = 5f;
    public float rotateDuration = 2f;
    public float targetYRotation = 140f;

    [Header("Footstep")]
    public AudioSource footstepAudio;
    public float footstepInterval = 0.4f;
    public float footstepStopTime = 6f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        StartCoroutine(MoveRoutine());
        StartCoroutine(RotateRoutine());
        StartCoroutine(FootstepRoutine());
    }

    IEnumerator MoveRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        Vector3 targetPosition =
            startPosition + new Vector3(moveX, moveY, moveZ);

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position =
                Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
    }

    IEnumerator RotateRoutine()
    {
        yield return new WaitForSeconds(rotateStartDelay);

        Quaternion targetRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            targetYRotation,
            transform.eulerAngles.z
        );

        float timer = 0f;

        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;

            float t = timer / rotateDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation =
                Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation;
    }

    IEnumerator FootstepRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        float timer = 0f;

        while (timer < footstepStopTime)
        {
            if (footstepAudio != null)
            {
                footstepAudio.Play();
            }

            yield return new WaitForSeconds(footstepInterval);

            timer += footstepInterval;
        }

        if (footstepAudio != null)
        {
            footstepAudio.Stop();
        }
    }
}