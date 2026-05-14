using UnityEngine;
using TMPro; // TextMeshPro 사용
using Fusion;

public class GameUIManager : MonoBehaviour
{
    [Header("UI 텍스트 연결")]
    public TextMeshProUGUI missionText;   // 현재 미션 내용
    public TextMeshProUGUI progressText;  // 0/5 진행도
    public TextMeshProUGUI timerText;     // 10:00 타이머

    void Start()
    {
        if (missionText != null) missionText.text = "Mission 1: Restore power to the dark room";
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (timerText != null) timerText.text = "10:00";
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