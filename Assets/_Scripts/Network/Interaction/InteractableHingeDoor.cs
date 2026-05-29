using UnityEngine;

[RequireComponent(typeof(HingeJoint))]
public class InteractableHingeDoor : MonoBehaviour
{
    private HingeJoint hingeJoint;
    private Rigidbody rb;
    private Transform pullingHand;
    private bool isPulling = false;

    [Header("문 물리 스프링 설정")]
    [Tooltip("손을 따라오는 힘의 세기입니다. 문이 무거우면 값을 올려주세요.")]
    public float springForce = 500f;
    public float damperForce = 15f;

    [Header("로컬 미션 완료 조건")]
    [Tooltip("문이 몇 도 이상 열렸을 때 서버로 완료 신호를 보낼지 결정합니다.")]
    public float openThresholdAngle = 75f;
    private bool isNetworkTriggered = false;

    void Awake()
    {
        hingeJoint = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();

        // 처음에는 유저가 잡기 전까지 스프링 기능을 꺼둡니다.
        if (hingeJoint != null) hingeJoint.useSpring = false;
    }

    public void StartPull(Transform hand)
    {
        pullingHand = hand;
        isPulling = true;
        isNetworkTriggered = false;

        if (hingeJoint != null) hingeJoint.useSpring = true;

        // 잡는 순간 물리 속도를 초기화하여 튕김 현상을 방지합니다.
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void EndPull()
    {
        isPulling = false;
        pullingHand = null;

        if (hingeJoint != null)
        {
            // 손을 놓으면 문이 그 자리에 부드럽게 멈추거나 힘이 빠지도록 스프링을 끕니다.
            hingeJoint.useSpring = false;
        }
    }

    void FixedUpdate()
    {
        if (!isPulling || pullingHand == null || hingeJoint == null) return;

        // 1. 힌지 축의 세계 좌표 기준점을 가져옵니다.
        Vector3 hingeWorldPos = transform.TransformPoint(hingeJoint.anchor);

        // 2. 힌지 중심점에서 현재 내 VR 손까지의 방향 벡터를 구합니다.
        Vector3 dirToHand = pullingHand.position - hingeWorldPos;

        // 3. 문의 부모(벽이나 프레임) 기준으로 방향 벡터를 로컬 변환합니다. (Y축 회전 기준)
        Vector3 localDir = transform.parent != null ?
            transform.parent.InverseTransformDirection(dirToHand) : dirToHand;

        // 4. 삼각함수(Atan2)로 손이 위치한 곳의 목표 각도(Degree)를 계산합니다.
        // ★ 모델의 앞뒤 방향에 따라 x, z가 바뀔 수 있으므로 팁을 참고하세요.
        float targetAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        // 5. 이미 세팅해두신 HingeJoint의Limits(최대/최소 열림 각도) 범위 밖으로 나가지 못하게 제한합니다.
        if (hingeJoint.useLimits)
        {
            targetAngle = Mathf.Clamp(targetAngle, hingeJoint.limits.min, hingeJoint.limits.max);
        }

        // 6. Hinge Joint의 스프링 목표치를 내 손 각도로 갱신하여 물리 엔진이 문을 회전시키게 만듭니다.
        JointSpring spring = hingeJoint.spring;
        spring.targetPosition = targetAngle;
        spring.spring = springForce;
        spring.damper = damperForce;
        hingeJoint.spring = spring;

        // 7. [로컬 미션 체크] 문이 특정 각도 이상 활짝 열렸다면 서버로 RPC를 딱 한 번 전송합니다.
        if (!isNetworkTriggered && Mathf.Abs(hingeJoint.angle) >= openThresholdAngle)
        {
            isNetworkTriggered = true;
            SendDoorOpenMissionComplete();
        }
    }

    private void SendDoorOpenMissionComplete()
    {
        Debug.Log($"<color=lime>[Local Mission] 문이 {hingeJoint.angle:F1}도 열려 탈출구 개방 조건을 만족했습니다!</color>");

        // 유저분이 구현해두신 NetworkManager의 RPC를 발동시킵니다.
        if (NetworkManager.Instance != null)
        {
            // 예시: 문이 열린 좌표를 기반으로 EMP나 알림을 방 전체에 동기화
            NetworkManager.Instance.RequestCmdExplosion(transform.position, 3f);
        }
    }
}