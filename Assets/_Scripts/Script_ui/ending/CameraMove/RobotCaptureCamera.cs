using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RobotCaptureCamera : MonoBehaviour
{
    [Header("Camera")]
    public Transform targetCamera;

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
        // 시작 검정
        SetFadeAlpha(1f);

        // 1. 검정 -> 투명
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // 카메라 이동 소리 시작
        PlayCameraMoveSound();

        // 2. 카메라 X 이동
        yield return StartCoroutine(MoveCameraX(cameraTargetX, cameraMoveXDuration));

        // 3. 카메라 오른쪽 90도 회전
        yield return StartCoroutine(RotateCameraY(cameraRightTurnY, cameraTurnDuration));

        // 4. 앞으로 이동 + 고개 내리기
        yield return StartCoroutine(MoveZAndRotateX(
            cameraForwardZDistance,
            cameraLookDownX,
            cameraForwardDuration
        ));

        // 소리 종료
        StopCameraMoveSound();
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
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    IEnumerator MoveCameraX(float targetX, float duration)
    {
        Vector3 start = targetCamera.position;
        Vector3 end = new Vector3(targetX, start.y, start.z);

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetCamera.position = Vector3.Lerp(start, end, time / duration);
            yield return null;
        }

        targetCamera.position = end;
    }

    IEnumerator RotateCameraY(float addY, float duration)
    {
        Vector3 startRot = targetCamera.eulerAngles;
        Vector3 endRot = startRot;
        endRot.y += addY;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            targetCamera.eulerAngles = Vector3.Lerp(startRot, endRot, time / duration);
            yield return null;
        }

        targetCamera.eulerAngles = endRot;
    }

    IEnumerator MoveZAndRotateX(float zDistance, float targetXRot, float duration)
    {
        Vector3 startPos = targetCamera.position;
        Vector3 endPos = startPos + new Vector3(0f, 0f, zDistance);

        Vector3 startRot = targetCamera.eulerAngles;
        Vector3 endRot = startRot;
        endRot.x = targetXRot;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            targetCamera.position = Vector3.Lerp(startPos, endPos, t);
            targetCamera.eulerAngles = Vector3.Lerp(startRot, endRot, t);

            yield return null;
        }

        targetCamera.position = endPos;
        targetCamera.eulerAngles = endRot;
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
        if (audioSource == null) return;

        audioSource.Stop();
    }
}