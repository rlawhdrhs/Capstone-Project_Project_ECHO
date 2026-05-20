using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class ExitDoorController_VR : MonoBehaviour
{
    [Header("문 클릭 가능 여부")]
    public bool canClickDoor = false;

    [Header("문 오브젝트")]
    public Transform doorTop;
    public Transform doorBottom;

    public Vector3 topOpenOffset = new Vector3(0, 3f, 0);
    public Vector3 bottomOpenOffset = new Vector3(0, -3f, 0);
    public float doorOpenTime = 3f;

    [Header("하얀 빛")]
    public Light whiteLight;
    public float maxLightIntensity = 8f;

    [Header("플레이어 이동")]
    public Transform player; // 여기에 XR Origin을 넣으세요.
    public Transform exitPoint;
    public float moveTime = 3f;

    [Header("하얀 화면 Fade")]
    public Image whiteFadeImage;

    [Header("엔딩 씬")]
    public string endingSceneName = "EndingScene";

    private bool isOpening = false;

    // CharacterController 제어를 위한 변수
    private CharacterController playerCC;

    void Start()
    {
        canClickDoor = false;

        if (whiteLight != null)
            whiteLight.intensity = 0f;

        if (whiteFadeImage != null)
        {
            Color c = whiteFadeImage.color;
            c.a = 0f;
            whiteFadeImage.color = c;
        }

        // 플레이어의 CC를 미리 찾아둡니다.
        if (player != null)
        {
            playerCC = player.GetComponent<CharacterController>();
        }
    }

    public void TryOpenDoor()
    {
        if (!canClickDoor) return;
        if (isOpening) return;

        Debug.Log("문 열림 연출 시작");
        StartCoroutine(OpenDoorSequence());
    }

    public void EnableDoorClick()
    {
        canClickDoor = true;
        Debug.Log("이제 문을 클릭할 수 있음");
    }

    IEnumerator OpenDoorSequence()
    {
        isOpening = true;

        if (playerCC != null)
        {
            playerCC.enabled = false;
        }

        Vector3 topStart = doorTop.position;
        Vector3 bottomStart = doorBottom.position;
        Vector3 topEnd = topStart + topOpenOffset;
        Vector3 bottomEnd = bottomStart + bottomOpenOffset;

        Vector3 playerStart = player.position;
        Vector3 playerEnd = exitPoint.position;

        float totalTime = Mathf.Max(doorOpenTime, moveTime);
        float timer = 0f;

        while (timer < totalTime)
        {
            timer += Time.deltaTime;

            float doorT = Mathf.Clamp01(timer / doorOpenTime);
            float moveT = Mathf.Clamp01(timer / moveTime);

            doorT = Mathf.SmoothStep(0f, 1f, doorT);
            moveT = Mathf.SmoothStep(0f, 1f, moveT);

            doorTop.position = Vector3.Lerp(topStart, topEnd, doorT);
            doorBottom.position = Vector3.Lerp(bottomStart, bottomEnd, doorT);

            if (whiteLight != null)
                whiteLight.intensity = Mathf.Lerp(0f, maxLightIntensity, doorT);

            player.position = Vector3.Lerp(playerStart, playerEnd, moveT);

            if (whiteFadeImage != null)
            {
                Color c = whiteFadeImage.color;
                c.a = Mathf.Lerp(0f, 1f, moveT);
                whiteFadeImage.color = c;
            }

            yield return null;
        }

        SceneManager.LoadScene(endingSceneName);
    }
}