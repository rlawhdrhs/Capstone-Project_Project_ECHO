

using UnityEngine;

public class GreenMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 220f;

    private Rigidbody rb;
    private float moveZ;
    private float turnY;

    public float jumpForce = 5f;
    private bool isGrounded;

    [Header("Camera Setting")]
    public Transform cameraPivot;
    public float lookSpeed = 100f;

    private float verticalRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        moveZ = 0f;
        turnY = 0f;

        if (Input.GetKey(KeyCode.W))
            moveZ = 1f;
        if (Input.GetKey(KeyCode.S))
            moveZ = -1f;

        if (Input.GetKey(KeyCode.A))
            turnY = -1f;
        if (Input.GetKey(KeyCode.D))
            turnY = 1f;

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
        // 회전
        Quaternion targetRotation = rb.rotation * Quaternion.Euler(0f, turnY * turnSpeed * Time.fixedDeltaTime, 0f);
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
