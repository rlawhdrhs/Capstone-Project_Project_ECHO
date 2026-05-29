using UnityEngine;
using UnityEngine.InputSystem; // ★ New Input System 필수

public class SettingsMenuManager : MonoBehaviour
{
    [Header("토글할 실제 UI 판넬 (하위 Panel 오브젝트 추천)")]
    [SerializeField] private GameObject uiPanelObject;

    [Header("VR 정렬 설정 (카메라 앞 배치 거리)")]
    [SerializeField] private float distanceFromCamera = 1.2f;

    private InputAction _localLeftMenuAction;

    void Awake()
    {
        // 스스로 왼손 메뉴 버튼을 감지하도록 세팅
        _localLeftMenuAction = new InputAction(binding: "<XRController>{LeftHand}/menu");
        _localLeftMenuAction.Enable();
    }

    void OnDestroy()
    {
        if (_localLeftMenuAction != null)
        {
            _localLeftMenuAction.Disable();
        }
    }

    void Start()
    {
        // 만약 인스펙터에서 지정을 안 했다면 자식 오브젝트를 자동으로 타겟팅
        if (uiPanelObject == null)
        {
            if (transform.childCount > 0)
                uiPanelObject = transform.GetChild(0).gameObject;
            else
                uiPanelObject = this.gameObject;
        }

        // [중요] 이 스크립트가 붙은 오브젝트(부모)는 켜두고, 실제 UI 내용물(자식 판넬)만 끕니다.
        if (uiPanelObject != this.gameObject)
        {
            uiPanelObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[SettingsMenuManager] uiPanelObject가 자기 자신으로 되어있으면 꺼진 후 다시 켜지지 않습니다! 하위 Panel을 연결해주세요.");
        }
    }

    void Update()
    {
        // 1. New Input System 방식으로 PC ESC 키 체크
        bool isEscPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        // 2. VR 왼손 메뉴 버튼 체크
        bool isVRMenuPressed = _localLeftMenuAction != null && _localLeftMenuAction.WasPressedThisFrame();

        // 둘 중 하나라도 눌렸다면 토글
        if (isVRMenuPressed || isEscPressed)
        {
            ToggleSettingsMenu();
        }
    }

    public void ToggleSettingsMenu()
    {
        if (uiPanelObject == null) return;

        bool isActive = !uiPanelObject.activeSelf;
        uiPanelObject.SetActive(isActive);

        // 창이 켜지는 순간에만 내 눈앞(메인 카메라 정면)으로 소환
        if (isActive)
        {
            ArrangeMenuInFrontOfCamera();
        }
    }

    // 메인 카메라 정면에 UI를 직관적으로 정렬해주는 VR 표준 함수
    private void ArrangeMenuInFrontOfCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // 1. 카메라의 현재 위치에서 바라보는 정면 방향으로 거리만큼 떨어진 좌표 계산
        Vector3 targetPosition = mainCam.transform.position + (mainCam.transform.forward * distanceFromCamera);
        transform.position = targetPosition;

        // 2. UI 판넬이 카메라(플레이어 눈)를 똑바로 바라보도록 회전 회전 (글씨 뒤집힘 방지 +180도)
        transform.LookAt(mainCam.transform.position);
        transform.Rotate(0, 180, 0);
    }
}