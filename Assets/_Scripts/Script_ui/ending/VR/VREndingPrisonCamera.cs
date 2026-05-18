using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VREndingPrisonCamera : MonoBehaviour
{
    [Header("VR Rig")]
    [Tooltip("Main Camera가 아닌 XR Origin 전체를 할당하세요.")]
    public Transform targetXROrigin;

    [Header("Fade UI")]
    public Image fadeImage;

    [Header("Move Target")]
    public Transform moveTarget;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip dragSound;
    public AudioClip lockSound;
    public AudioClip hitSound;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        SetFadeAlpha(1f);

        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // ⚠️ VR 주의: XR Origin의 X축 회전은 바닥 전체를 기울게 합니다. 멀미 유발 가능성이 큽니다.
        yield return StartCoroutine(RotateX(30f, -40f, 3f));

        if (audioSource != null && dragSound != null)
        {
            audioSource.clip = dragSound;
            audioSource.loop = false;
            audioSource.Play();
        }

        yield return StartCoroutine(Fade(0f, 1f, 1f));

        if (audioSource != null) audioSource.Stop();
        if (audioSource != null && lockSound != null) audioSource.PlayOneShot(lockSound);

        yield return StartCoroutine(MoveXROrigin(moveTarget.position, 1f));

        yield return StartCoroutine(Fade(1f, 0f, 1f));

        yield return StartCoroutine(RotateX(GetCurrentX(), -10f, 2f));
        yield return StartCoroutine(RotateY(GetCurrentY(), -140f, 2f));

        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);

        yield return StartCoroutine(RotateX(GetCurrentX(), 30f, 0.15f));
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(startAlpha, endAlpha, time / duration));
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
            Vector3 rot = targetXROrigin.eulerAngles;
            rot.x = x;
            targetXROrigin.eulerAngles = rot;
            yield return null;
        }
        Vector3 finalRot = targetXROrigin.eulerAngles;
        finalRot.x = endX;
        targetXROrigin.eulerAngles = finalRot;
    }

    IEnumerator RotateY(float startY, float endY, float duration)
    {
        float time = 0f;
        startY = NormalizeAngle(startY);

        while (time < duration)
        {
            time += Time.deltaTime;
            float y = Mathf.Lerp(startY, endY, time / duration);
            Vector3 rot = targetXROrigin.eulerAngles;
            rot.y = y;
            targetXROrigin.eulerAngles = rot;
            yield return null;
        }
        Vector3 finalRot = targetXROrigin.eulerAngles;
        finalRot.y = endY;
        targetXROrigin.eulerAngles = finalRot;
    }

    IEnumerator MoveXROrigin(Vector3 targetPos, float duration)
    {
        float time = 0f;
        Vector3 startPos = targetXROrigin.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetXROrigin.position = Vector3.Lerp(startPos, targetPos, time / duration);
            yield return null;
        }
        targetXROrigin.position = targetPos;
    }

    float GetCurrentX() => NormalizeAngle(targetXROrigin.eulerAngles.x);
    float GetCurrentY() => NormalizeAngle(targetXROrigin.eulerAngles.y);

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}