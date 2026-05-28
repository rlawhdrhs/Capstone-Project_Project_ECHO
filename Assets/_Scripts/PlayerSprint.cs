using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerSprint : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionProperty sprintButton;

    [Header("Movement Provider")]
    public DynamicMoveProvider moveProvider;

    [Header("Speed Settings (기본값)")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 4f;

    // ★ 역할별 속도가 초기화되었는지 확인하는 플래그
    private bool _isSpeedInitialized = false;

    void OnEnable()
    {
        if (sprintButton.action != null)
            sprintButton.action.Enable();
    }

    void OnDisable()
    {
        if (sprintButton.action != null)
            sprintButton.action.Disable();
    }

    void Update()
    {
        if (moveProvider == null) return;

        if (!_isSpeedInitialized && NetworkManager.Instance != null)
        {
            var runner = NetworkManager.Instance.GetComponent<Fusion.NetworkRunner>();

            if (runner != null && runner.IsRunning)
            {
                if (runner.IsServer) // 호스트 = 잠입자
                {
                    walkSpeed = 2.5f;
                    sprintSpeed = 4f;
                }
                else // 클라이언트 = 추격자
                {
                    walkSpeed = 3f;
                    sprintSpeed = 3f;
                }
                _isSpeedInitialized = true; // 초기화 완료
            }
        }

        bool isPressed = sprintButton.action != null && sprintButton.action.IsPressed();

        if (isPressed)
        {
            moveProvider.moveSpeed = sprintSpeed;
        }
        else
        {
            moveProvider.moveSpeed = walkSpeed;
        }
    }
}