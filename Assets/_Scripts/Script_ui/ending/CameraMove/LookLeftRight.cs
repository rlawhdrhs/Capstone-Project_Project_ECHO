using System.Collections;
using UnityEngine;

public class LookLeftRightOnce : MonoBehaviour
{
    public float leftAngle = -45f;
    public float rightAngle = 45f;
    public float totalDuration = 5f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
        StartCoroutine(LookRoutine());
    }

    IEnumerator LookRoutine()
    {
        float partDuration = totalDuration / 4f;

        // 원래 자리 → 왼쪽
        yield return RotateTo(startRotation * Quaternion.Euler(0f, leftAngle, 0f), partDuration);

        // 왼쪽 → 원래 자리
        yield return RotateTo(startRotation, partDuration);

        // 원래 자리 → 오른쪽
        yield return RotateTo(startRotation * Quaternion.Euler(0f, rightAngle, 0f), partDuration);

        // 오른쪽 → 원래 자리
        yield return RotateTo(startRotation, partDuration);
    }

    IEnumerator RotateTo(Quaternion targetRotation, float duration)
    {
        Quaternion fromRotation = transform.localRotation;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localRotation = Quaternion.Lerp(fromRotation, targetRotation, t);

            yield return null;
        }

        transform.localRotation = targetRotation;
    }
}