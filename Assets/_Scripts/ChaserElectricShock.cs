using UnityEngine;
using UnityEngine.XR;

public class ChaserElectricShock : MonoBehaviour
{
    [Header("컨트롤러 설정")]
    public Transform leftControllerTransform;
    public Transform rightControllerTransform;

    [Header("스킬 설정")]
    public float chargeTimeReq = 1.0f; // 기 모으는 시간 (1초)
    public float armSpreadThreshold = 0.4f; // 팔을 벌려야 하는 거리 차이 (40cm)

    [Header("공격 판정 설정")]
    public float shockRadius = 3.0f; // 감전 반경 (3m)
    public LayerMask runawayLayer;   // 생존자 레이어
    public float stunDuration = 3.0f; // 기절 시간 (3초)

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
        // 양손 기기 정보 가져오기
        InputDevice leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // 양쪽 그립(중지) 버튼이 둘 다 눌려있는지 확인
        bool leftGripPressed = leftDevice.TryGetFeatureValue(CommonUsages.grip, out float leftGrip) && leftGrip > 0.1f;
        bool rightGripPressed = rightDevice.TryGetFeatureValue(CommonUsages.grip, out float rightGrip) && rightGrip > 0.1f;

        if (leftGripPressed && rightGripPressed)
        {
            if (!isCharged)
            {
                // 1단계: 기 모으기 진행
                currentChargeTime += Time.deltaTime;
                
                // 기 모으는 동안 약한 진동 계속 주기
                chargeHapticTimer -= Time.deltaTime;
                if (chargeHapticTimer <= 0)
                {
                    leftDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    rightDevice.SendHapticImpulse(0, 0.2f, 0.05f);
                    chargeHapticTimer = 0.05f;
                }

                Debug.Log($"⚡ 기 모으는 중... ({currentChargeTime:F1}초)");

                if (currentChargeTime >= chargeTimeReq)
                {
                    isCharged = true;
                    // 차징 완료 순간의 양손 사이 거리 기록
                    initialArmDistance = Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
                    
                    // 차징 완료 진동 (쿵!)
                    leftDevice.SendHapticImpulse(0, 0.7f, 0.2f);
                    rightDevice.SendHapticImpulse(0, 0.7f, 0.2f);
                    Debug.Log("★ 전기 충격 차징 완료! 양팔을 쫙 벌리세요!");
                }
            }
            else
            {
                // 2단계: 차징 완료 후 팔을 벌리는지 감지
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
            // 도중에 버튼을 하나라도 떼면 차징 취소
            if (currentChargeTime > 0)
            {
                Debug.Log("❌ 그립 버튼을 떼서 차징이 취소되었습니다.");
                currentChargeTime = 0f;
                isCharged = false;
            }
        }
    }

    private void FireElectricShock(InputDevice leftDevice, InputDevice rightDevice)
    {
        Debug.Log("⚡ 전기 충격 방출!!! ⚡");
        
        // 강력한 방출 진동 피드백
        leftDevice.SendHapticImpulse(0, 1.0f, 0.5f);
        rightDevice.SendHapticImpulse(0, 1.0f, 0.5f);

        // 내 주변 3미터 안의 생존자 찾기
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, shockRadius, runawayLayer);
        
        foreach (Collider hit in hitColliders)
        {
            // 생존자에게 달아둔 RunawayStatus 스크립트 호출
            RunawayStatus runaway = hit.GetComponentInParent<RunawayStatus>();
            if (runaway != null)
            {
                runaway.ApplyStun(stunDuration);
                Debug.Log("🎯 생존자 감전! 이동 불가!");
            }
        }

        // 상태 초기화
        currentChargeTime = 0f;
        isCharged = false;
    }
}