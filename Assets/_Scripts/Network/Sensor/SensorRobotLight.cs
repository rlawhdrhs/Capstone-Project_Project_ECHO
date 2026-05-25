using UnityEngine;

public class SensorRobotLight : MonoBehaviour
{
    public Light targetLight;

    [Header("진영별 빛 색상")]
    public Color chaserColor = Color.white;       // 추격자에게 보일 색 (흰색)
    public Color infiltratorColor = Color.red;    // 잠입자에게 보일 색 (빨간색)

    void OnEnable()
    {
        if (targetLight != null)
            targetLight = GetComponent<Light>();
        ApplyLocalLightColor();
    }

    void ApplyLocalLightColor()
    {
        if (targetLight == null) return;

        // ★ NetworkManager의 싱글톤 인스턴스를 통해 내 진영을 판단합니다.
        if (NetworkManager.Instance != null)
        {
            bool isChaser = NetworkManager.Instance.CheckIfLocalPlayerIsChaser();

            if (isChaser)
            {
                targetLight.color = chaserColor; // 내가 추격자면 흰색
            }
            else
            {
                targetLight.color = infiltratorColor; // 내가 잠입자면 빨간색
            }
        }
    }
}