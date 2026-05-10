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
    public float walkSpeed = 2.5f; // 기본 걷기 속도
    public float sprintSpeed = 5.0f; // 달리기 속도

    void Update()
    {
        if (moveProvider == null) return;

        if (sprintButton.action.IsPressed())
        {
            moveProvider.moveSpeed = sprintSpeed;
        }
        else
        {
            moveProvider.moveSpeed = walkSpeed;
        }
    }
}