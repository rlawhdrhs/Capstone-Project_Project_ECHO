using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem; //

public class ExitDoorController : MonoBehaviour
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
    public Transform player;
    public Transform exitPoint;
    public float moveTime = 3f;

    [Header("하얀 화면 Fade")]
    public Image whiteFadeImage;

    //[Header("빛나는 벽")]
    //public GameObject whiteGlowWall;

    [Header("엔딩 씬")]
    public string endingSceneName = "EndingScene";

    private bool isOpening = false;

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

        //if (whiteGlowWall != null)
        //{
        //    whiteGlowWall.SetActive(false);
        //}
    }

    //void OnMouseDown()
    //{
    //    TryOpenDoor();
    //}

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Debug.Log("Ray가 맞은 오브젝트: " + hit.collider.gameObject.name);

                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("DoorClickArea 클릭됨");
                    TryOpenDoor();
                }
            }
            else
            {
                Debug.Log("Ray가 아무것도 못 맞춤");
            }
        }
    }

    public void EnableDoorClick()
    {
        canClickDoor = true;
        Debug.Log("이제 문을 클릭할 수 있음");
    }

    public void TryOpenDoor()
    {
        Debug.Log("TryOpenDoor 실행됨 / canClickDoor = " + canClickDoor);

        if (!canClickDoor)
        {
            Debug.Log("문 클릭 가능 상태가 아님");
            return;
        }

        if (isOpening)
        {
            Debug.Log("이미 문 열리는 중");
            return;
        }

        Debug.Log("문 열림 연출 시작");
        StartCoroutine(OpenDoorSequence());
    }



    //IEnumerator OpenDoorSequence()
    //{
    //    isOpening = true;

    //    //if (whiteGlowWall != null)
    //    //{
    //    //    whiteGlowWall.SetActive(true);
    //    //}

    //    Vector3 topStart = doorTop.position;
    //    Vector3 bottomStart = doorBottom.position;

    //    Vector3 topEnd = topStart + topOpenOffset;
    //    Vector3 bottomEnd = bottomStart + bottomOpenOffset;

    //    float timer = 0f;

    //    while (timer < doorOpenTime)
    //    {
    //        timer += Time.deltaTime;
    //        float t = timer / doorOpenTime;

    //        doorTop.position = Vector3.Lerp(topStart, topEnd, t);
    //        doorBottom.position = Vector3.Lerp(bottomStart, bottomEnd, t);

    //        if (whiteLight != null)
    //            whiteLight.intensity = Mathf.Lerp(0f, maxLightIntensity, t);

    //        yield return null;
    //    }

    //    yield return new WaitForSeconds(0.3f);

    //    Vector3 playerStart = player.position;
    //    Vector3 playerEnd = exitPoint.position;

    //    timer = 0f;

    //    while (timer < moveTime)
    //    {
    //        timer += Time.deltaTime;
    //        float t = timer / moveTime;

    //        player.position = Vector3.Lerp(playerStart, playerEnd, t);

    //        if (whiteFadeImage != null)
    //        {
    //            Color c = whiteFadeImage.color;
    //            c.a = Mathf.Lerp(0f, 1f, t);
    //            whiteFadeImage.color = c;
    //        }

    //        yield return null;
    //    }

    //    SceneManager.LoadScene(endingSceneName);
    //}

    IEnumerator OpenDoorSequence()
    {
        isOpening = true;

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