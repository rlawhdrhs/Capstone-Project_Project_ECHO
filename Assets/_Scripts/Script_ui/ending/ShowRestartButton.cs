using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShowRestartButton : MonoBehaviour
{
    [Header("3D Button")]
    public GameObject restartButtonCube;

    [Header("UI")]
    public GameObject endingTextObject;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 3f;

    void Start()
    {
        restartButtonCube.SetActive(false);
        endingTextObject.SetActive(false);

        StartCoroutine(EndingSequence());
    }

    IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(15f);

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);

            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, 1f);

        // 18초쯤 엔딩 문구 등장
        endingTextObject.transform.SetAsLastSibling();
        endingTextObject.SetActive(true);

        // 20초쯤 3D 큐브 버튼 등장
        yield return new WaitForSeconds(2f);

        restartButtonCube.SetActive(true);
    }
}