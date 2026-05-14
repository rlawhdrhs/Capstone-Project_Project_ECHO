using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingPrisonCamera : MonoBehaviour
{
    [Header("Camera")]
    public Transform targetCamera;

    [Header("Fade UI")]
    public Image fadeImage;

    [Header("Move Target")]
    public Transform moveTarget;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip dragSound; // 질질 끌리는 소리
    public AudioClip lockSound; // 열쇠 잠그는 소리
    public AudioClip hitSound;  // 마지막 툭 소리

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // 시작: 검정 화면
        SetFadeAlpha(1f);

        // 1. 검정 -> 투명 1초
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // 2. 카메라 Rotation X: 30 -> -40, 3초
        yield return StartCoroutine(RotateX(30f, -40f, 3f));

        // 3. 질질 끌리는 소리 재생
        if (audioSource != null && dragSound != null)
        {
            audioSource.clip = dragSound;
            audioSource.loop = false;
            audioSource.Play();
        }

        // 4. 화면 서서히 검정 1초
        yield return StartCoroutine(Fade(0f, 1f, 1f));

        // 질질 끌리는 소리 끊기
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 열쇠 잠그는 소리 재생
        if (audioSource != null && lockSound != null)
        {
            audioSource.PlayOneShot(lockSound);
        }

        // 5. 카메라 위치 이동 1초
        yield return StartCoroutine(MoveCamera(moveTarget.position, 1f));

        // 6. 화면 서서히 다시 투명 1초
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // 7. 카메라 Rotation X: 현재값 -> -10, 2초
        yield return StartCoroutine(RotateX(GetCurrentX(), -10f, 2f));

        // 8. 카메라 Rotation Y: 현재값 -> -140, 2초
        yield return StartCoroutine(RotateY(GetCurrentY(), -140f, 2f));

        // 9. 마지막 충격 소리
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 10. 카메라를 빠르게 위로 홱 돌림
        yield return StartCoroutine(RotateX(GetCurrentX(), 30f, 0.15f));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            SetFadeAlpha(alpha);
            yield return null;
        }

        SetFadeAlpha(endAlpha);
    }

    void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    IEnumerator RotateX(float startX, float endX, float duration)
    {
        float time = 0f;
        startX = NormalizeAngle(startX);

        while (time < duration)
        {
            time += Time.deltaTime;

            float x = Mathf.Lerp(startX, endX, time / duration);

            Vector3 rot = targetCamera.eulerAngles;
            rot.x = x;
            targetCamera.eulerAngles = rot;

            yield return null;
        }

        Vector3 finalRot = targetCamera.eulerAngles;
        finalRot.x = endX;
        targetCamera.eulerAngles = finalRot;
    }

    IEnumerator RotateY(float startY, float endY, float duration)
    {
        float time = 0f;
        startY = NormalizeAngle(startY);

        while (time < duration)
        {
            time += Time.deltaTime;

            float y = Mathf.Lerp(startY, endY, time / duration);

            Vector3 rot = targetCamera.eulerAngles;
            rot.y = y;
            targetCamera.eulerAngles = rot;

            yield return null;
        }

        Vector3 finalRot = targetCamera.eulerAngles;
        finalRot.y = endY;
        targetCamera.eulerAngles = finalRot;
    }

    IEnumerator MoveCamera(Vector3 targetPos, float duration)
    {
        float time = 0f;
        Vector3 startPos = targetCamera.position;

        while (time < duration)
        {
            time += Time.deltaTime;

            targetCamera.position = Vector3.Lerp(startPos, targetPos, time / duration);

            yield return null;
        }

        targetCamera.position = targetPos;
    }

    float GetCurrentX()
    {
        return NormalizeAngle(targetCamera.eulerAngles.x);
    }

    float GetCurrentY()
    {
        return NormalizeAngle(targetCamera.eulerAngles.y);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}