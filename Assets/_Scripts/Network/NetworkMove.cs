using UnityEngine;
using Fusion;

// 네트워크로 주고받을 입력 데이터 상자
public struct NetworkInputData : INetworkInput
{
    //이동 값
    public float moveX;
    public float moveZ;
    public float turnY;
    //점프
    public NetworkBool jump;
    //키보드
    public bool keyR;
    public bool keySpace;
    //마우스 클릭 이벤트
    public bool leftClick;
    //컨트롤러 이벤트
    public bool rightTrigger;
    public bool leftButtonA;
    //XR OIGIN  위치 및 아바타 위치
    public Vector3 rootPosition;
    public Quaternion rootRotation;
    public Vector3 headPosition;
    public Quaternion headRotation;
    public Vector3 leftHandPosition;
    public Quaternion leftHandRotation;
    public Vector3 rightHandPosition;
    public Quaternion rightHandRotation;
    //쪼그려 앉기
    public float crouch;
    //센서 로봇 조종
    public NetworkBool isPossessingDrone;
}

public class NetworkMove : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 220f;
    public float jumpForce = 5f;

    [Header("Camera Setting")]
    public Transform cameraPivot;
    public float lookSpeed = 100f;

    private Rigidbody rb;
    private float verticalRotation = 0f;
    private bool isGrounded;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Render()
    {
        if (HasInputAuthority && cameraPivot != null)
        {
            float lookX = 0f;
            if (Input.GetKey(KeyCode.UpArrow)) lookX = -1f;
            if (Input.GetKey(KeyCode.DownArrow)) lookX = 1f;

            verticalRotation += lookX * lookSpeed * Time.deltaTime;
            verticalRotation = Mathf.Clamp(verticalRotation, -60f, 60f);
            cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 좌우 회전 (A, D)
            Quaternion targetRotation = rb.rotation * Quaternion.Euler(0f, data.turnY * turnSpeed * Runner.DeltaTime, 0f);
            rb.MoveRotation(targetRotation);

            // 앞뒤 이동 (W, S)
            Vector3 move = transform.forward * data.moveZ * moveSpeed * Runner.DeltaTime;
            rb.MovePosition(rb.position + move);

            // 점프 (Space)
            if (data.jump && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }
    }

    // 바닥 체크 로직 (기존과 동일)
    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}