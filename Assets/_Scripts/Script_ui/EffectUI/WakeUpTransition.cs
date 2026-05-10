using System.Collections;
using UnityEngine;

public class WakeUpTransition : MonoBehaviour
{
    public RectTransform topLid;
    public RectTransform bottomLid;

    public float holdTime = 3.0f;
    public float openTime = 10.0f;
    public float openOffset = 1000f;
    public float closedY = 300f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip wakeUpSound;

    private bool isPlaying = false;

    public void ResetLids()
    {
        topLid.gameObject.SetActive(true);
        bottomLid.gameObject.SetActive(true);

        topLid.anchoredPosition = new Vector2(0f, closedY);
        bottomLid.anchoredPosition = new Vector2(0f, -closedY-200);

        isPlaying = false;
    }


    public void PlayWakeUp(System.Action middleAction)
    {
        StopAllCoroutines();

        topLid.gameObject.SetActive(true);
        bottomLid.gameObject.SetActive(true);

        topLid.anchoredPosition = new Vector2(0f, closedY);
        bottomLid.anchoredPosition = new Vector2(0f, -closedY-200);

        isPlaying = false;

        StartCoroutine(WakeUpRoutine(middleAction));
    }

    IEnumerator WakeUpRoutine(System.Action middleAction)
    {
        isPlaying = true;

        if (audioSource != null && wakeUpSound != null)
            audioSource.PlayOneShot(wakeUpSound);

        topLid.gameObject.SetActive(true);
        bottomLid.gameObject.SetActive(true);

        Vector2 topStart = new Vector2(0f, closedY);
        Vector2 bottomStart = new Vector2(0f, -closedY-200);
        Vector2 topEnd = topStart + Vector2.up * openOffset;
        Vector2 bottomEnd = bottomStart + Vector2.down * openOffset;

        topLid.anchoredPosition = topStart;
        bottomLid.anchoredPosition = bottomStart;

        yield return new WaitForSeconds(holdTime);

        middleAction?.Invoke();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openTime; // 시간을 비율로 변환

            // 부드러운 가속/감속 효과 (SmoothStep)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            topLid.anchoredPosition = Vector2.Lerp(topStart, topEnd, smoothT);
            bottomLid.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, smoothT);

            yield return null;
        }

        // 최종 위치 확정
        topLid.anchoredPosition = topEnd;
        bottomLid.anchoredPosition = bottomEnd;

        //topLid.gameObject.SetActive(false);
        //bottomLid.gameObject.SetActive(false);

        isPlaying = false;
    }
}