using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ChaserElectricShock : MonoBehaviour
{
    [Header("컨트롤러 트랜스폼")]
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

    // New Input System 액션
    private InputAction leftGripAction;
    private InputAction rightGripAction;

    void Awake()
    {
        leftGripAction = new InputAction(binding: "<XRController>{LeftHand}/grip");
        rightGripAction = new InputAction(binding: "<XRController>{RightHand}/grip");
    }

    void OnEnable()
    {
        leftGripAction.Enable();
        rightGripAction.Enable();
    }

    void OnDisable()
    {
        leftGripAction.Disable();
        rightGripAction.Disable();
    }

    void Update()
    {
        HandleElectricShock();
    }

    private void HandleElectricShock()
    {
        bool leftGripPressed = leftGripAction.ReadValue<float>() > 0.1f;
        bool rightGripPressed = rightGripAction.ReadValue<float>() > 0.1f;

        UnityEngine.XR.InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        UnityEngine.XR.InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (leftGripPressed && rightGripPressed)
        {
            if (!isCharged)
            {
                currentChargeTime += Time.deltaTime;

                chargeHapticTimer -= Time.deltaTime;
                if (chargeHapticTimer <= 0)
                {
                    if (leftDevice.isValid) leftDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    if (rightDevice.isValid) rightDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    chargeHapticTimer = 0.05f;
                }

                if (currentChargeTime >= chargeTimeReq)
                {
                    isCharged = true;
                    initialArmDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);

                    if (leftDevice.isValid) leftDevice.SendHapticImpulse(0, 0.7f, 0.2f);
                    if (rightDevice.isValid) rightDevice.SendHapticImpulse(0, 0.7f, 0.2f);
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

    private void FireElectricShock(UnityEngine.XR.InputDevice leftDevice, UnityEngine.XR.InputDevice rightDevice)
    {
        Debug.Log("⚡ [추격자] 전기 충격 모션 감지! 방출 시도 !!! ⚡");
        if (leftDevice.isValid) leftDevice.SendHapticImpulse(0, 1.0f, 0.5f);
        if (rightDevice.isValid) rightDevice.SendHapticImpulse(0, 1.0f, 0.5f);

        if (SoundManager.Instance != null)
        {
            // 내 위치(드론 위치)에서 1.5초간 지속되는 전기 충격음 프리펩 생성
            SoundManager.Instance.EmitSound(transform.position, 1.5f, SoundType.ElectricShock);
        }

        if (NetworkManager.Instance != null && NetworkManager.Instance.InfiltratorObject != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, NetworkManager.Instance.InfiltratorObject.transform.position);

            Debug.Log($"[레이더 디버그] 계산된 거리: {distanceToTarget}m (공격 범위: {shockRadius}m)");

            if (distanceToTarget <= shockRadius)
            {
                // ★ 핵심 변경: 잠입자 오브젝트에서 네트워크 스크립트를 가져옵니다.
                SoundEmitter_Network infiltratorSound = NetworkManager.Instance.InfiltratorObject.GetComponent<SoundEmitter_Network>();

                if (infiltratorSound != null)
                {
                    // 아바타 고유의 NetworkBehaviour RPC를 직접 호출합니다!
                    infiltratorSound.RPC_RequestStunToMe(stunDuration);
                    Debug.Log("Target [추격자] 잠입자 아바타에 직접 스턴 RPC 발사 완료!");
                }
                else
                {
                    Debug.LogError("❌ [에러] 잠입자 오브젝트에서 SoundEmitter_Network를 찾을 수 없습니다.");
                }
            }
        }

        currentChargeTime = 0f;
        isCharged = false;
    }
}