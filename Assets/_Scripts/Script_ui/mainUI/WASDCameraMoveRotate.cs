using UnityEngine;

public class WASDCameraMoveRotate : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Rotate")]
    public float rotateAngle = 30f;
    public float rotateSpeed = 8f;

    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // W/S: 앞뒤 이동
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * moveSpeed * Time.deltaTime;
        }

        // A/D: 한 번 누를 때마다 30도 회전
        if (Input.GetKeyDown(KeyCode.A))
        {
            targetRotation *= Quaternion.Euler(0f, -rotateAngle, 0f);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            targetRotation *= Quaternion.Euler(0f, rotateAngle, 0f);
        }

        // 부드럽게 회전
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
}