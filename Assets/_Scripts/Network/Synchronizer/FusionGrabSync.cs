using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FusionXRIBridge : MonoBehaviour
{
    private XRGrabInteractable _interactable;
    private XRInteractionManager _interactionManager;

    [Header("그랩 인정 거리 (미터 단위)")]
    public float grabDistance = 0.25f;

    // 독립적으로 그랩 입력을 감지하기 위한 인풋 액션
    private InputAction _leftGripAction;
    private InputAction _rightGripAction;

    private void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
        _interactionManager = FindFirstObjectByType<XRInteractionManager>();

        // New Input System 기반 그랩 버튼 매핑
        _leftGripAction = new InputAction(binding: "<XRController>{LeftHand}/grip");
        _rightGripAction = new InputAction(binding: "<XRController>{RightHand}/grip");
    }

    private void OnEnable()
    {
        _leftGripAction.Enable();
        _rightGripAction.Enable();
    }

    private void OnDisable()
    {
        _leftGripAction.Disable();
        _rightGripAction.Disable();
    }

    void Update()
    {
        // 이미 무언가에 잡혀있는 상태라면 패스
        if (_interactable.isSelected) return;

        if (LocalVRRig.Instance == null || _interactionManager == null) return;

        // 1. 오른손 그랩 버튼을 누른 순간 체크
        if (_rightGripAction.WasPressedThisFrame())
        {
            TryManualGrab(LocalVRRig.Instance.hardwareRightHand);
        }

        // 2. 왼손 그랩 버튼을 누른 순간 체크
        if (_leftGripAction.WasPressedThisFrame())
        {
            TryManualGrab(LocalVRRig.Instance.hardwareLeftHand);
        }
    }

    private void TryManualGrab(Transform handTransform)
    {
        if (handTransform == null) return;

        // 퓨션 물리 엔진을 거치지 않고, 두 트랜스폼의 순수 중심 거리를 계산합니다.
        float distance = Vector3.Distance(handTransform.position, transform.position);

        // 설정한 거리보다 손이 가까이 있다면
        if (distance <= grabDistance)
        {
            // 손에 붙어있는 XRI 인터랙터(Direct Interactor 등)를 찾습니다.
            XRBaseInteractor interactor = handTransform.GetComponentInChildren<XRBaseInteractor>();

            if (interactor != null)
            {
                Debug.Log($"<color=lime>[XRI Bridge] 거리가 가까워 강제로 그랩을 성립시킵니다! 거리: {distance}</color>");

                // ⭐ XRI 공식 API: 트리거 이벤트를 건너뛰고 인터랙터에게 이 물건을 즉시 잡으라고 명령합니다.
                _interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)_interactable);
            }
        }
    }
}