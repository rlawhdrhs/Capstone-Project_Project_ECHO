using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRRobotCaptureCamera : MonoBehaviour
{
    [Header("VR Rig")]
    [Tooltip("Main Camera가 아닌 XR Origin 전체를 할당하세요.")]
    public Transform targetXROrigin;

    [Header("Fade")]
    public Image fadeImage;

    [Header("Camera Movement")]
    public float cameraTargetX = -135f;
    public float cameraMoveXDuration = 6f;

    public float cameraRightTurnY = 90f;
    public float cameraTurnDuration = 2f;

    public float cameraForwardZDistance = 5f;
    public float cameraLookDownX = 30f;
    public float cameraForwardDuration = 2f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip cameraMoveSound;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        SetFadeAlpha(1f);
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        PlayCameraMoveSound();

        yield return StartCoroutine(MoveXROriginX(cameraTargetX, cameraMoveXDuration));
        yield return StartCoroutine(RotateXROriginY(cameraRightTurnY, cameraTurnDuration));

        yield return StartCoroutine(MoveZAndRotateX(cameraForwardZDistance, cameraLookDownX, cameraForwardDuration));

        StopCameraMoveSound();
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
        if (fadeImage == null) return;
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    IEnumerator MoveXROriginX(float targetX, float duration)
    {
        Vector3 start = targetXROrigin.position;
        Vector3 end = new Vector3(targetX, start.y, start.z);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetXROrigin.position = Vector3.Lerp(start, end, time / duration);
            yield return null;
        }
        targetXROrigin.position = end;
    }

    IEnumerator RotateXROriginY(float addY, float duration)
    {
        Vector3 startRot = targetXROrigin.eulerAngles;
        Vector3 endRot = startRot;
        endRot.y += addY;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetXROrigin.eulerAngles = Vector3.Lerp(startRot, endRot, time / duration);
            yield return null;
        }
        targetXROrigin.eulerAngles = endRot;
    }

    IEnumerator MoveZAndRotateX(float zDistance, float targetXRot, float duration)
    {
        Vector3 startPos = targetXROrigin.position;
        Vector3 endPos = startPos + targetXROrigin.forward * zDistance; // 로컬 forward 기준 이동

        Vector3 startRot = targetXROrigin.eulerAngles;
        Vector3 endRot = startRot;
        endRot.x = targetXRot;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            targetXROrigin.position = Vector3.Lerp(startPos, endPos, t);
            targetXROrigin.eulerAngles = Vector3.Lerp(startRot, endRot, t);
            yield return null;
        }
        targetXROrigin.position = endPos;
        targetXROrigin.eulerAngles = endRot;
    }

    void PlayCameraMoveSound()
    {
        if (audioSource == null || cameraMoveSound == null) return;
        audioSource.clip = cameraMoveSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    void StopCameraMoveSound()
    {
        if (audioSource != null) audioSource.Stop();
    }
}