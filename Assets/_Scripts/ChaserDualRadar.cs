using UnityEngine;
using UnityEngine.XR;
using Fusion;

public class ChaserDualRadar : MonoBehaviour
{
    [Header("탐지 대상 (네트워크 자동 할당됨)")]
    public Transform target;

    [Header("왼손 탐지기 설정")]
    public Transform leftControllerTransform;
    [Header("오른손 탐지기 설정")]
    public Transform rightControllerTransform;

    [Header("벽 체크 (장애물 레이어)")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("탐지 설정")]
    public float detectionAngle = 45f;
    public float detectionRadius = 15f;

    private float leftCooldown = 0f;
    private float rightCooldown = 0f;

    private NetworkRunner runner;


    void Update()
    {
        if (target == null && NetworkManager.Instance != null && NetworkManager.Instance.InfiltratorObject != null)
        {
            target = NetworkManager.Instance.InfiltratorObject.transform;
        }

        if (leftCooldown > 0) leftCooldown -= Time.deltaTime;
        if (rightCooldown > 0) rightCooldown -= Time.deltaTime;

        ProcessRawRadar(leftControllerTransform, XRNode.LeftHand, ref leftCooldown, "왼손");
        ProcessRawRadar(rightControllerTransform, XRNode.RightHand, ref rightCooldown, "오른손");
    }

    private void ProcessRawRadar(Transform controllerTransform, XRNode node, ref float cooldown, string handName)
    {
        if (target == null || controllerTransform == null) return;

        InputDevice device = InputDevices.GetDeviceAtXRNode(node);

        if (device.isValid)
        {
            if (device.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue > 0.1f)
            {
                // 1. 거리 계산
                float distance = Vector3.Distance(controllerTransform.position, target.position);
                if (distance > detectionRadius) return;

                // 2. 각도 계산
                Vector3 directionToTarget = (target.position - controllerTransform.position).normalized;
                float angle = Vector3.Angle(controllerTransform.forward, directionToTarget);

                if (angle < detectionAngle)
                {
                    // 3. 벽 체크
                    if (Physics.Linecast(controllerTransform.position, target.position, obstacleLayer)) return;

                    // 4. 진동 출력 (이 xr_origin을 쓰고 있는 로컬 플레이어 장비에만 울림)
                    if (cooldown <= 0f)
                    {
                        device.SendHapticImpulse(0, 0.7f, 0.1f);
                        cooldown = 0.1f;
                    }
                }
            }
        }
    }
}