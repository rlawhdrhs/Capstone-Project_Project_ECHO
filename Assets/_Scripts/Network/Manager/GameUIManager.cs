using UnityEngine;
using TMPro; // TextMeshPro 사용
using Fusion;

public class GameUIManager : MonoBehaviour
{
    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI missionText;   // 현재 미션 내용
    public TextMeshProUGUI progressText;  // 0/5 진행도
    public TextMeshProUGUI timerText;     // 10:00 타이머

    [Header("미니맵 설정")]
    public GameObject minimapObject;

    [Header("스텔스 UI 설정")]
    public GameObject stealthModeUI;

    private bool _isMinimapInitialized = false;

    void Start()
    {
        if (missionText != null) missionText.text = "Mission 1: Restore power to the dark room";
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (timerText != null) timerText.text = "10:00";

        if (stealthModeUI != null) stealthModeUI.SetActive(false);

        // 미션이 바뀔 때마다 OnUpdateMissionText 함수가 실행되도록 구독
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnMissionChangedEvent += OnUpdateMissionText;
        }
    }

    void OnDestroy()
    {
        // 씬이 넘어갈 때 에러 방지를 위해 구독 해제
        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.OnMissionChangedEvent -= OnUpdateMissionText;
        }
    }

    void Update()
    {
        if (NetworkGameManager.Instance == null) return;
        if (NetworkGameManager.Instance.Object == null || !NetworkGameManager.Instance.Object.IsValid) return;

        if (!_isMinimapInitialized)
        {
            SetupMinimapRoleVisibility();
        }

        HandleMinimapInput();

        if (stealthModeUI != null)
        {
            bool shouldShowStealthUI = StealthDetector.Instance != null && StealthDetector.Instance.isStealthMode;

            // 매 프레임 SetActive를 호출하는 것보다 상태가 바뀔 때만 호출되도록 방어 코드 추가
            if (stealthModeUI.activeSelf != shouldShowStealthUI)
            {
                stealthModeUI.SetActive(shouldShowStealthUI);
            }
        }

        // UI 연결이 안 되어 있어도 에러가 나지 않도록 방어
        if (timerText == null || progressText == null) return;

        // 1. 남은 시간(타이머) 업데이트
        TickTimer timer = NetworkGameManager.Instance.GlobalGameTimer;
        if (timer.IsRunning)
        {
            float? remainingTime = timer.RemainingTime(NetworkGameManager.Instance.Runner);
            if (remainingTime.HasValue)
            {
                int minutes = Mathf.FloorToInt(remainingTime.Value / 60f);
                int seconds = Mathf.FloorToInt(remainingTime.Value % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); // 10:00 포맷
            }
        }
        else if (timer.Expired(NetworkGameManager.Instance.Runner))
        {
            timerText.text = "00:00";
            timerText.color = Color.red; // 시간이 다 되면 빨간색으로 변경
            // TODO: 시간 초과 게임 오버 로직
        }

        // 2. 데이터 전송 미션(1번)일 때만 진행도 업데이트
        if (NetworkGameManager.Instance.CurrentMissionIndex == 1)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = $"Data Mission Progress: {NetworkGameManager.Instance.DataCollectionProgress} / {NetworkGameManager.Instance.MaxDataNodes}";
        }
        else
        {
            // 다른 미션일 때는 진행도 텍스트를 숨김
            progressText.gameObject.SetActive(false);
        }
    }

    private void HandleMinimapInput()
    {
        if (minimapObject == null) return;

        // 1. 오직 호스트(Server)인 경우에만 작동 가능
        if (NetworkGameManager.Instance.Runner.IsServer)
        {
            // 2. NetworkManager에서 왼쪽 Y버튼이 눌렸는지 확인 (PC 키보드 테스트시 'Y' 키도 지원되게 연동)
            if (NetworkManager.Instance != null && (NetworkManager.Instance.IsLeftYDown || Input.GetKeyDown(KeyCode.Y)))
            {
                // 켜질 예정인지 끔 상태가 될 예정인지 미리 계산
                bool willBeActive = !minimapObject.activeSelf;

                // 태블릿 껐다 켰다(Toggle) 처리
                minimapObject.SetActive(willBeActive);

                if (willBeActive)
                {
                    OnUpdateMissionText(NetworkGameManager.Instance.CurrentMissionIndex);
                }
            }
        }
    }

    private void SetupMinimapRoleVisibility()
    {
        if (minimapObject == null) return;

        bool isHost = NetworkGameManager.Instance.Runner.IsServer;

        minimapObject.SetActive(false);

        _isMinimapInitialized = true;
    }

    // 미션 단계가 바뀔 때 한 번만 호출됨
    public void OnUpdateMissionText(int missionIndex)
    {
        switch (missionIndex)
        {
            case 0:
                missionText.text = "Mission 1: Restore power to the dark room";
                break;
            case 1:
                missionText.text = "Mission 2: Find the data table and transfer data 5 times";
                break;
            case 2:
            case 3:
                missionText.text = "Final Mission: Find the exit and escape";
                break;
        }
    }
}