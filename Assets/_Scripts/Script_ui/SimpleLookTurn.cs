using UnityEngine;

public class SimpleLookTurn : MonoBehaviour
{
    [Header("Turn Setting")]
    public float turnAngle = 30f;

    private Rigidbody rb;
    private float targetYaw;

    [Header("Camera Setting")]
    public Transform cameraPivot;
    public float lookSpeed = 100f;

    private float verticalRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            targetYaw = transform.eulerAngles.y;
        }
        else
        {
            Debug.LogError("Rigidbody 없음");
        }
    }

    void Update()
    {
        // 좌우 회전 (A, D)
        if (Input.GetKeyDown(KeyCode.A))
            targetYaw -= turnAngle;

        if (Input.GetKeyDown(KeyCode.D))
            targetYaw += turnAngle;

        // 카메라 상하 
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
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);
        rb.MoveRotation(targetRotation);
    }
}