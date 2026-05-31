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

    [Header("디버그용 순정 테스트")]
    public GameObject directTestPrefab;

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

        // =============== [에디터 디버그용 치트키 추가] ===============
        // 키보드 T 키를 누르면 양팔 벌리기 제스처를 무시하고 무조건 발사 함수를 실행합니다.
        if (Input.GetKeyDown(KeyCode.T))
    {

        if (directTestPrefab != null)
        {
            GameObject clone = Instantiate(directTestPrefab, transform.position, Quaternion.identity);
            clone.transform.localScale = new Vector3(shockRadius * 2f, shockRadius * 2f, shockRadius * 2f);
        }
        else
        {
        }
    }
        // ==========================================================
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
        Debug.Log("⚡ [추격자] 전기 충격 방출!");

        // 1. 컨트롤러 진동 및 사운드
        if (leftDevice.isValid) leftDevice.SendHapticImpulse(0, 1.0f, 0.5f);
        if (rightDevice.isValid) rightDevice.SendHapticImpulse(0, 1.0f, 0.5f);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(transform.position, 1.5f, SoundType.ElectricShock);
        }

        SensorSynchronizer droneSync = null;

        if (PossessionManager.Instance != null && PossessionManager.Instance.currentDrone != null)
        {
            droneSync = PossessionManager.Instance.currentDrone;
        }

        if (droneSync != null && droneSync.electricShockwavePrefab != null)
        {
            // 1. 내 화면 즉시 생성
            GameObject localFX = Instantiate(droneSync.electricShockwavePrefab, transform.position, Quaternion.identity);
            localFX.transform.localScale = new Vector3(shockRadius * 2f, shockRadius * 2f, shockRadius * 2f);
            Destroy(localFX, 3.0f);

            // 2. 항상 켜져있는 본체 스크립트를 통해 RPC 발사 (잠입자 화면 스크립트가 켜져있으므로 정상 수신)
            droneSync.RPC_PlayShockwaveVFX_Global(transform.position, shockRadius);
            Debug.Log("✨ 항상 켜져있는 본체 스크립트를 통해 VFX RPC 전달 완료!");
        }
        else
        {
            Debug.LogError("❌ [VFX 에러] 현재 조종 중인 드론에서 LaserDetector_Network를 찾을 수 없습니다!");
        }

        // 4. 잠입자 스턴 판정 로직
        if (NetworkManager.Instance != null && NetworkManager.Instance.InfiltratorObject != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, NetworkManager.Instance.InfiltratorObject.transform.position);
            if (distanceToTarget <= shockRadius)
            {
                SoundEmitter_Network infiltratorSound = NetworkManager.Instance.InfiltratorObject.GetComponent<SoundEmitter_Network>();
                if (infiltratorSound != null)
                {
                    infiltratorSound.RPC_RequestStunToMe(stunDuration);
                    Debug.Log("🎯 잠입자 타격 성공! 스턴 RPC 발사 완료!");
                }
            }
        }

        // 상태 리셋
        currentChargeTime = 0f;
        isCharged = false;
    }
}