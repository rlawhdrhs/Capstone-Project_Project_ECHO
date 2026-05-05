using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class StealthDetector_XRI : MonoBehaviour
{
    [Header("참조 설정")]
    public Transform mainCamera; // XR Origin의 Main Camera
    public CharacterController characterController; // XR Origin 본체에 추가하세요
    public ContinuousMoveProvider moveProvider; // Locomotion/Move 오브젝트 연결

    [Header("설정값")]
    public float stealthThreshold = 1.2f; // 은신 기준 높이
    public float speedRatio = 0.7f; // 70% 감속

    private float _initialSpeed;
    public bool isStealthMode = false;

    void Start()
    {
        if (moveProvider != null)
        {
            _initialSpeed = moveProvider.moveSpeed; // XRI 기본 이동 속도 저장
        }
    }

    void Update()
    {
        if (mainCamera == null || characterController == null || moveProvider == null) return;

        float currentHeight = mainCamera.localPosition.y;

        if (currentHeight <= stealthThreshold)
        {
            if (!isStealthMode)
            {
                isStealthMode = true;
                moveProvider.moveSpeed = _initialSpeed * speedRatio;
                Debug.Log("<color=green>👣 XRI 은신 모드: 70% 감속</color>");
            }
            // 물리 콜라이더 조절
            characterController.height = currentHeight;
            characterController.center = new Vector3(0, currentHeight / 2f, 0);
        }
        else
        {
            if (isStealthMode)
            {
                isStealthMode = false;
                moveProvider.moveSpeed = _initialSpeed;
                Debug.Log("<color=white>🏃 XRI 일반 모드: 속도 복구</color>");
            }
            characterController.height = 1.7f; // 기본값
            characterController.center = new Vector3(0, 0.85f, 0);
        }
    }
}