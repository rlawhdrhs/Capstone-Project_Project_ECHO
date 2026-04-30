using System.Collections;
using UnityEngine;

public class WakeUpTransition : MonoBehaviour
{
   // public WakeUpTransition wakeUpTransition;

    public RectTransform topLid;
    public RectTransform bottomLid;

    public float holdTime = 0.4f;
    public float openTime = 1.5f;
    public float openOffset = 1000f;

    private bool isPlaying = false;

    void Start()
    {
        CloseEyes();
    }

    public void PlayWakeUp(System.Action middleAction)
    {
        if (!isPlaying)
        {
            StartCoroutine(WakeUpRoutine(middleAction));
        }
    }

    IEnumerator WakeUpRoutine(System.Action middleAction)
    {
        isPlaying = true;

        CloseEyes();

        middleAction?.Invoke();

        yield return new WaitForSeconds(holdTime);

        yield return OpenEyes();

        isPlaying = false;
    }

    void CloseEyes()
    {
        topLid.anchoredPosition = Vector2.zero;
        bottomLid.anchoredPosition = Vector2.zero;
    }

    IEnumerator OpenEyes()
    {
        float t = 0f;

        while (t < openTime)
        {
            float progress = t / openTime;
            progress = Mathf.SmoothStep(0f, 1f, progress);

            float offset = Mathf.Lerp(0f, openOffset, progress);

            topLid.anchoredPosition = new Vector2(0f, offset);
            bottomLid.anchoredPosition = new Vector2(0f, -offset);

            t += Time.deltaTime;
            yield return null;
        }

        topLid.anchoredPosition = new Vector2(0f, openOffset);
        bottomLid.anchoredPosition = new Vector2(0f, -openOffset);
    }
}