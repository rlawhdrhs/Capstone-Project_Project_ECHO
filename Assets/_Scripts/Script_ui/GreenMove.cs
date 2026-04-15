using UnityEngine;

public class GreenMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Turn Setting")]
    public float turnAngle = 30f;

    private Rigidbody rb;
    private float moveZ;

    public float jumpForce = 5f;
    private bool isGrounded;

    [Header("Camera Setting")]
    public Transform cameraPivot;
    public float lookSpeed = 100f;

    private float verticalRotation = 0f;

    private float targetYaw;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        moveZ = 0f;

        if (Input.GetKey(KeyCode.W))
            moveZ = 1f;
        if (Input.GetKey(KeyCode.S))
            moveZ = -1f;

        // A, D를 누른 순간에만 30도 회전 목표값 변경
        if (Input.GetKeyDown(KeyCode.A))
            targetYaw -= turnAngle;

        if (Input.GetKeyDown(KeyCode.D))
            targetYaw += turnAngle;

        float lookX = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            lookX = -1f;
        if (Input.GetKey(KeyCode.DownArrow))
            lookX = 1f;

        verticalRotation += lookX * lookSpeed * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);

        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // 회전: 목표 각도로 즉시 맞춤
        Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
        rb.MoveRotation(targetRotation);

        // 이동
        Vector3 move = transform.forward * moveZ * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}