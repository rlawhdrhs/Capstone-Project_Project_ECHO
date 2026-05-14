using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSceneLoader : MonoBehaviour
{
    public enum GameResult
    {
        IntruderWin,
        ChaserWin
    }

    [Header("¿£µù ¾À ÀÌ¸§")]
    public string escapeEndingSceneName = "EscapeEndingScene";
    public string captureEndingSceneName = "CaptureEndingScene";

    public void LoadEndingScene(GameResult result)
    {
        switch (result)
        {
            case GameResult.IntruderWin:
                SceneManager.LoadScene(escapeEndingSceneName);
                break;

            case GameResult.ChaserWin:
                SceneManager.LoadScene(captureEndingSceneName);
                break;
        }
    }

    public void LoadEscapeEnding()
    {
        SceneManager.LoadScene(escapeEndingSceneName);
    }

    public void LoadCaptureEnding()
    {
        SceneManager.LoadScene(captureEndingSceneName);
    }
}


//FindObjectOfType<EndingSceneLoader>().LoadEscapeEnding();

//FindObjectOfType<EndingSceneLoader>().LoadCaptureEnding();