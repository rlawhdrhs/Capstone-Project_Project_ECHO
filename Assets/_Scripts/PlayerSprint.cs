using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerSprint : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionProperty sprintButton;

    [Header("Movement Provider")]
    public DynamicMoveProvider moveProvider;

    [Header("Speed Settings")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5.0f;

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

        bool isPressed = sprintButton.action != null && sprintButton.action.IsPressed();

        if (isPressed)
        {
            moveProvider.moveSpeed = sprintSpeed;
            Debug.Log("<color=cyan>🏃 달리기 버튼 인식됨! 현재 속도: 5.0</color>"); 
        }
        else
        {
            moveProvider.moveSpeed = walkSpeed;
        }
    }
}