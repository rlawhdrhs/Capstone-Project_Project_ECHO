using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSceneTransition : MonoBehaviour
{
    [Header("Fade")]
    public Image fadeImage;
    public float transitionDuration = 3f;

    [Header("Audio")]
    public AudioSource[] fadeOutSounds;
    public AudioSource doorSound;
    public AudioClip doorClip;

    [Header("Move Forward")]
    public Transform xrOrigin;
    public Transform moveTarget;

    [Header("Scene")]
    public string nextSceneName = "1.Tutorial";

    private bool isTransitioning = false;

    public void StartTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        Vector3 startPos = xrOrigin.position;
        Vector3 targetPos = moveTarget.position;

        float[] originalVolumes = new float[fadeOutSounds.Length];
        for (int i = 0; i < fadeOutSounds.Length; i++)
            originalVolumes[i] = fadeOutSounds[i].volume;

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // 앞으로 이동
            xrOrigin.position = Vector3.Lerp(startPos, targetPos, smoothT);

            // 화면 점점 검게
            Color color = fadeImage.color;
            color.a = smoothT;
            fadeImage.color = color;

            // 주변 사운드 점점 작아짐
            for (int i = 0; i < fadeOutSounds.Length; i++)
            {
                fadeOutSounds[i].volume = Mathf.Lerp(originalVolumes[i], 0f, smoothT);
            }

            yield return null;
        }

        if (doorSound != null && doorClip != null)
        {
            doorSound.PlayOneShot(doorClip);
        }


        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(nextSceneName);
    }
}