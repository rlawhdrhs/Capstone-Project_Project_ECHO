using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    public Image fadeImage;
    public float fadeDuration = 2f;

    private void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 시작 시 검정 -> 투명
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // 투명 -> 검정
        yield return StartCoroutine(FadeOut());

        /// 현재 씬 번호
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        //SceneManager.LoadScene("MainScene");

        // 다음 씬 로드
        SceneManager.LoadScene(currentScene + 1);

        yield return null;


        //yield return null;

        // 검정 -> 투명
        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = Mathf.Lerp(0, 1, time / fadeDuration);

            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = Mathf.Lerp(1, 0, time / fadeDuration);

            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }
    }
}


//1.Canvas 만들기
//Canvas
//Render Mode = Screen Space Overlay
//2. 검정 Image 만들기
//Anchor Stretch 전체
//색 = 검정
//Alpha = 1

//(게임 시작 시 검정 상태에서 시작)

//3. SceneFader 스크립트 연결
//fadeImage 칸에 검정 이미지 드래그