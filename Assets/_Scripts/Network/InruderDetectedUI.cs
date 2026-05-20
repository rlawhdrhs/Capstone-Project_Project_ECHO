using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class IntruderDetectedUI : MonoBehaviour
{
    public static IntruderDetectedUI Instance;

    [Header("UI 연결")]
    public GameObject warningTextObj; // TextMeshPro 오브젝트 전체를 끄기 위함
    public GameObject redVignetteObj; // 가장자리 빨간 이미지 오브젝트

    [Header("깜빡임 설정")]
    public float blinkSpeed = 0.2f; // 깜빡이는 속도 (0.2초마다)

    private Coroutine blinkCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작 시 완전히 꺼둡니다.
        if (warningTextObj != null) warningTextObj.SetActive(false);
        if (redVignetteObj != null) redVignetteObj.SetActive(false);
    }

    public void ShowWarning(bool isDetected)
    {
        if (isDetected)
        {
            // 들켰을 때 코루틴이 안 돌고 있다면 실행!
            if (blinkCoroutine == null)
            {
                blinkCoroutine = StartCoroutine(BlinkRoutine());
            }
        }
        else
        {
            // 감지가 풀리면 코루틴을 멈추고 UI를 끕니다.
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            if (warningTextObj != null) warningTextObj.SetActive(false);
            if (redVignetteObj != null) redVignetteObj.SetActive(false);
        }
    }
    
    // 미친 듯이 깜빡거리는 코루틴
    private IEnumerator BlinkRoutine()
    {
        bool toggle = false;
        while (true) // isDetected가 false가 되어 코루틴이 꺼질 때까지 무한 반복
        {
            toggle = !toggle;

            if (warningTextObj != null) warningTextObj.SetActive(toggle);
            if (redVignetteObj != null) redVignetteObj.SetActive(toggle);

            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}