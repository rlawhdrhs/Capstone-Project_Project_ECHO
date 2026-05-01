using System.Collections;
using UnityEngine;

public class WakeUpTransition : MonoBehaviour
{
    public RectTransform topLid;
    public RectTransform bottomLid;

    public float holdTime = 0.6f;
    public float openTime = 2.0f;
    public float openOffset = 1000f;
    public float closedY = 300f;

    private bool isPlaying = false;

    public void PlayWakeUp(System.Action middleAction)
    {
        Debug.Log("WakeUpTransition ½ÇÇàµÊ");

        if (!isPlaying)
        {
            StartCoroutine(WakeUpRoutine(middleAction));
        }
    }

    IEnumerator WakeUpRoutine(System.Action middleAction)
    {
        isPlaying = true;

        topLid.gameObject.SetActive(true);
        bottomLid.gameObject.SetActive(true);

        topLid.anchoredPosition = new Vector2(0f, closedY);
        bottomLid.anchoredPosition = new Vector2(0f, -closedY);

        Debug.Log("´« °¨±è À§Ä¡ ¼¼ÆÃ ¿Ï·á");

        yield return new WaitForSeconds(holdTime);

        middleAction?.Invoke();

        float t = 0f;

        Vector2 topStart = new Vector2(0f, closedY);
        Vector2 bottomStart = new Vector2(0f, -closedY);

        Vector2 topEnd = new Vector2(0f, closedY + openOffset);
        Vector2 bottomEnd = new Vector2(0f, -closedY - openOffset);

        while (t < openTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, t / openTime);

            topLid.anchoredPosition = Vector2.Lerp(topStart, topEnd, progress);
            bottomLid.anchoredPosition = Vector2.Lerp(bottomStart, bottomEnd, progress);

            t += Time.deltaTime;
            yield return null;
        }

        topLid.anchoredPosition = topEnd;
        bottomLid.anchoredPosition = bottomEnd;

        Debug.Log("´« ¶ß±â ¿Ï·á");

        topLid.gameObject.SetActive(false);
        bottomLid.gameObject.SetActive(false);

        isPlaying = false;
    }
}