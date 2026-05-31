using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

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

    [Header("하얀 화면 Fade (VR Optimized)")]
    // [변경] UI Image 대신 카메라 앞에 배치할 3D Quad의 MeshRenderer를 사용합니다.
    public MeshRenderer whiteFadeQuadRenderer;

    [Header("엔딩 씬")]
    public string endingSceneName = "EndingScene";

    private bool isOpening = false;
    private Material whiteFadeMaterial; // 런타임 제어용 머티리얼

    void Start()
    {
        canClickDoor = false;

        if (whiteLight != null)
            whiteLight.intensity = 0f;

        // [변경] 시작할 때 하얀 막을 완전히 투명하게(Alpha = 0) 초기화합니다.
        if (whiteFadeQuadRenderer != null)
        {
            whiteFadeMaterial = whiteFadeQuadRenderer.material;
            Color c = whiteFadeMaterial.color;
            c.a = 0f;
            whiteFadeMaterial.color = c;
        }
    }

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

            // [변경] 플레이어가 이동함에 따라 하얀 Quad의 알파값을 올려 화면을 하얗게 채웁니다.
            if (whiteFadeMaterial != null)
            {
                Color c = whiteFadeMaterial.color;
                c.a = Mathf.Lerp(0f, 1f, moveT);
                whiteFadeMaterial.color = c;
            }

            yield return null;
        }

        SceneManager.LoadScene(endingSceneName);
    }
}