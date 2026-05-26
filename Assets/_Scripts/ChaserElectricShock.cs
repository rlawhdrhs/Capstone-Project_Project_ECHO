using UnityEngine;
using UnityEngine.XR;

public class ChaserElectricShock : MonoBehaviour
{
    [Header("컨트롤러 설정")]
    public Transform leftControllerTransform;
    public Transform rightControllerTransform;

    [Header("스킬 설정")]
    public float chargeTimeReq = 1.0f;
    public float armSpreadThreshold = 0.4f;

    [Header("공격 판정 설정")]
    public float shockRadius = 3.0f;
    public float stunDuration = 3.0f;

    private float currentChargeTime = 0f;
    private bool isCharged = false;
    private float initialArmDistance = 0f;
    private float chargeHapticTimer = 0f;

    void Update()
    {
        HandleElectricShock();
    }

    private void HandleElectricShock()
    {
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool leftGripPressed = leftDevice.TryGetFeatureValue(CommonUsages.grip, out float leftGrip) && leftGrip > 0.1f;
        bool rightGripPressed = rightDevice.TryGetFeatureValue(CommonUsages.grip, out float rightGrip) && rightGrip > 0.1f;

        if (leftGripPressed && rightGripPressed)
        {
            if (!isCharged)
            {
                currentChargeTime += Time.deltaTime;

                chargeHapticTimer -= Time.deltaTime;
                if (chargeHapticTimer <= 0)
                {
                    leftDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    rightDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    chargeHapticTimer = 0.05f;
                }

                if (currentChargeTime >= chargeTimeReq)
                {
                    isCharged = true;
                    initialArmDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);

                    leftDevice.SendHapticImpulse(0, 0.7f, 0.2f);
                    rightDevice.SendHapticImpulse(0, 0.7f, 0.2f);
                    Debug.Log("★ 전기 충격 차징 완료! 양팔을 쫙 벌리세요!");
                }
            }
            else
            {
                float currentDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
                float spread = currentDistance - initialArmDistance;

                if (spread > armSpreadThreshold)
                {
                    FireElectricShock(leftDevice, rightDevice);
                }
            }
        }
        else
        {
            if (currentChargeTime > 0)
            {
                currentChargeTime = 0f;
                isCharged = false;
            }
        }
    }

    private void FireElectricShock(InputDevice leftDevice, InputDevice rightDevice)
    {
        Debug.Log("⚡ 전기 충격 방출!!! ⚡");
        leftDevice.SendHapticImpulse(0, 1.0f, 0.5f);
        rightDevice.SendHapticImpulse(0, 1.0f, 0.5f);

        // [★ 멀티플레이 핵심 개편] 
        // 1v1 구도이므로 NetworkManager가 관리하는 잠입자(InfiltratorObject) 가 존재하는지 확인합니다.
        if (NetworkManager.Instance != null && NetworkManager.Instance.InfiltratorObject != null)
        {
            // 추격자인 내 위치와 원격에 있는 잠입자 캐릭터 사이의 순수 거리 측정
            float distanceToTarget = Vector3.Distance(transform.position, NetworkManager.Instance.InfiltratorObject.transform.position);

            if (distanceToTarget <= shockRadius)
            {
                // 범위 안에 있다면 RPC를 날려 호스트 컴퓨터에게 직접 스턴 상태를 주입합니다.
                NetworkManager.Instance.RequestStunToIntruder(stunDuration);
                Debug.Log("🎯 잠입자 탐지 성공! 네트워크 RPC 스턴 요청 송신.");
            }
        }

        currentChargeTime = 0f;
        isCharged = false;
    }
}