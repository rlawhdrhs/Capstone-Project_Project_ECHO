using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlinkTransition : MonoBehaviour
{
    public RectTransform topLid;
    public RectTransform bottomLid;

    public float closeTime = 0.2f;
    public float holdTime = 0.4f;
    public float openTime = 0.5f;

    private bool isPlaying = false;

    public void PlayBlink(System.Action middleAction)
    {
        if (!isPlaying)
        {
            StartCoroutine(BlinkRoutine(middleAction));
        }
    }

    IEnumerator BlinkRoutine(System.Action middleAction)
    {
        isPlaying = true;

        yield return MoveLids(1f, 0f, closeTime);

        middleAction?.Invoke();

        yield return new WaitForSeconds(holdTime);

        yield return MoveLids(0f, 1f, openTime);

        isPlaying = false;
    }

    IEnumerator MoveLids(float openAmountStart, float openAmountEnd, float time)
    {
        float t = 0f;

        while (t < time)
        {
            float value = Mathf.Lerp(openAmountStart, openAmountEnd, t / time);
            SetLids(value);

            t += Time.deltaTime;
            yield return null;
        }

        SetLids(openAmountEnd);
    }

    void SetLids(float openAmount)
    {
        float offset = 600f * openAmount;

        topLid.anchoredPosition = new Vector2(0, offset);
        bottomLid.anchoredPosition = new Vector2(0, -offset);
    }

    void Start()
    {
        Debug.Log("BlinkTransition Start ½ÇÇàµÊ");
        SetLids(1f);
    }
}