using UnityEngine;

public class InfiltratorMove : MonoBehaviour
{
    //public float moveSpeed = 5f;

    //void Update()
    //{
    //    float x = 0f;
    //    float z = 0f;

    //    if (Input.GetKey(KeyCode.A))
    //        x = -1f;
    //    if (Input.GetKey(KeyCode.D))
    //        x = 1f;
    //    if (Input.GetKey(KeyCode.W))
    //        z = 1f;
    //    if (Input.GetKey(KeyCode.S))
    //        z = -1f;

    //    Vector3 move = new Vector3(x, 0f, z).normalized;
    //    transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    //}


    public float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A))
            x = -1f;
        if (Input.GetKey(KeyCode.D))
            x = 1f;
        if (Input.GetKey(KeyCode.W))
            z = 1f;
        if (Input.GetKey(KeyCode.S))
            z = -1f;

        moveInput = new Vector3(x, 0f, z).normalized;
    }

    void FixedUpdate()
    {
        Vector3 newPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}