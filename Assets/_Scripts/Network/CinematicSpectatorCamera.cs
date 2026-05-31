using UnityEngine;

public class CinematicSpectatorCamera : MonoBehaviour
{
    [Header("이동 및 회전 속도")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2.5f; // Shift 누르면 속도 업
    public float lookSensitivity = 3f;
    public float smoothTime = 5f; // 카메라 관성 보정 (부드러운 연출용)

    [Header("촬영용 라이팅 (치트)")]
    public Light localNightVisionLight; // 어두운 맵을 촬영 전용으로 밝혀줄 조명

    private float _rotationX = 0f;
    private float _rotationY = 0f;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private bool _isMouseLocked = false;

    void Start()
    {
        _targetPosition = transform.position;
        _targetRotation = transform.rotation;

        // 마우스를 클릭하면 화면 중앙에 고정되어 비행 슈팅 게임처럼 조작 가능하게 설계
        SetMouseLock(true);

        // [자동 조명 생성] 만약 인스펙터에서 조명을 할당 안 했다면 자체적으로 태양광 생성
        if (localNightVisionLight == null)
        {
            GameObject lightObj = new GameObject("Spectator_LocalSun");
            lightObj.transform.SetParent(this.transform); // 장치 종속
            lightObj.transform.localRotation = Quaternion.Euler(45, 45, 0); // 사선으로 비추기

            localNightVisionLight = lightObj.AddComponent<Light>();
            localNightVisionLight.type = LightType.Directional;
            localNightVisionLight.intensity = 1.5f; // 밝기 수치
            localNightVisionLight.shadows = LightShadows.None; // 연출 최적화를 위해 그림자 해제
        }
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleExtraFeatures();
    }

    private void HandleMouseLook()
    {
        // ESC 키를 누르면 마우스 락 풀림 (유니티 창 제어용)
        if (Input.GetKeyDown(KeyCode.Escape)) SetMouseLock(!_isMouseLocked);
        if (Input.GetMouseButtonDown(0) && !_isMouseLocked) SetMouseLock(true);

        if (!_isMouseLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        _rotationY += mouseX;
        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -85f, 85f); // 위아래 꺾임 한계 걸기

        _targetRotation = Quaternion.Euler(_rotationX, _rotationY, 0f);

        // 관성을 주어 부드럽게 시선이 쫓아가도록 처리 (고급 방송 장비 느낌)
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * smoothTime);
    }

    private void HandleMovement()
    {
        // 휠 마우스 조작으로 기본 비행 스피드를 실시간 튜닝 가능하도록 편의 기능 가미
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            moveSpeed = Mathf.Clamp(moveSpeed + (scroll * 10f), 2f, 50f);
            Debug.Log($"[촬영캠] 현재 비행 속도: {moveSpeed:F1}");
        }

        // 기본 속도 정의
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift)) currentSpeed *= sprintMultiplier;

        // WASD 수평 이동 계산
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Q(상승), E(하강) 수직 이동 계산
        float moveY = 0f;
        if (Input.GetKey(KeyCode.Q)) moveY = 1f;
        if (Input.GetKey(KeyCode.E)) moveY = -1f;

        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveZ) + (Vector3.up * moveY);
        _targetPosition += moveDirection * currentSpeed * Time.deltaTime;

        // 위치값 부드럽게 Lerp 보정
        transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * smoothTime);
    }

    private void HandleExtraFeatures()
    {
        // [꿀기능 1] F1 누르면 내 화면 밝은 조명 On / Off 토글 (실제 인게임 어두운 연출과 대비 확인용)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (localNightVisionLight != null)
            {
                localNightVisionLight.enabled = !localNightVisionLight.enabled;
                Debug.Log($"[촬영캠] 로컬 야간투시경 조명 상태: {localNightVisionLight.enabled}");
            }
        }

        // [꿀기능 2] F2 누르면 순간적으로 가속 이동 좌표 초기화 (순간 정지 기능)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _targetPosition = transform.position;
        }
    }

    private void SetMouseLock(bool locked)
    {
        _isMouseLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}