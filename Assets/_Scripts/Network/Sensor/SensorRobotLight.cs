using System.Collections;
using UnityEngine;

public class SensorRobotLight : MonoBehaviour
{
    public Light targetLight;

    [Header("진영별 빛 색상")]
    public Color chaserColor = Color.white;       // 추격자에게 보일 색 (흰색)
    public Color infiltratorColor = Color.red;    // 잠입자에게 보일 색 (빨간색)

    void Start() // ★ OnEnable 대신 Start를 사용하고 코루틴을 돌려 타이밍 문제를 해결합니다.
    {
        // ★ 버그 수정: 비어있을 때(== null)만 자동으로 찾도록 변경합니다.
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        StartCoroutine(WaitAndApplyColor());
    }

    private IEnumerator WaitAndApplyColor()
    {
        // ★ 포톤 네트워크 매니저가 완전히 켜지고 싱글톤 인스턴스가 잡힐 때까지 안전하게 대기
        while (NetworkManager.Instance == null)
        {
            yield return null;
        }

        // 유저님의 네트워크 매니저에 "초기화 완료"를 뜻하는 변수가 있다면 
        // 아래처럼 한 번 더 대기해 주는 것이 멀티플레이에서 가장 안전합니다.
        // while (!NetworkManager.Instance.IsInitialized) { yield return null; }

        ApplyLocalLightColor();
    }

    void ApplyLocalLightColor()
    {
        if (targetLight == null)
        {
            Debug.LogWarning("[SensorRobotLight] 변경할 targetLight 가 할당되지 않았습니다!");
            return;
        }

        // 싱글톤을 통해 내 로컬 컴퓨터의 플레이어가 추격자인지 최종 판단
        bool isChaser = NetworkManager.Instance.CheckIfLocalPlayerIsChaser();

        if (isChaser)
        {
            targetLight.color = chaserColor; // 내가 추격자면 흰색
            Debug.Log("<color=white>[Light] 내 화면은 '추격자'이므로 로봇 불빛을 흰색으로 변경합니다.</color>");
        }
        else
        {
            targetLight.color = infiltratorColor; // 내가 잠입자면 빨간색
            Debug.Log("<color=red>[Light] 내 화면은 '잠입자'이므로 로봇 불빛을 빨간색으로 변경합니다.</color>");
        }
    }
}