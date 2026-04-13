using UnityEngine;

public class StealthDetector : MonoBehaviour
{
    [Header("참조 설정")]
    public Transform mainCamera; // Main Camera를 여기에 넣으세요.
    public CharacterController characterController; // XR Origin을 여기에 넣으세요.

    [Header("설정값")]
    public float stealthThreshold = 1.2f; // 조용히 걷기 기준 높이
    
    [Header("상태 (확인용)")]
    public bool isStealthMode = false;

    void Update()
    {
        if (mainCamera == null || characterController == null) return;

        // 1. 카메라의 '로컬' Y 높이를 읽습니다 (바닥으로부터의 높이)
        float currentEyeHeight = mainCamera.localPosition.y;

        // 2. 높이가 1.2m 이하인지 체크
        if (currentEyeHeight <= stealthThreshold)
        {
            if (!isStealthMode) // 상태가 바뀔 때 한 번만 출력하고 싶다면
            {
                Debug.Log("<color=green>👣 조용히 걷기 중 (은신 활성화)</color>");
                isStealthMode = true;
            }
            
            // [추가 서비스!] 물리적 몸(Capsule)의 높이도 눈높이에 맞춰줍니다.
            characterController.height = currentEyeHeight;
            characterController.center = new Vector3(0, currentEyeHeight / 2f, 0);
        }
        else
        {
            if (isStealthMode)
            {
                Debug.Log("<color=white>🏃 일반 보행 중</color>");
                isStealthMode = false;
            }

            // 서 있을 때의 기본 높이로 복구 (예: 1.7m)
            characterController.height = 1.7f;
            characterController.center = new Vector3(0, 1.7f / 2f, 0);
        }
    }
}