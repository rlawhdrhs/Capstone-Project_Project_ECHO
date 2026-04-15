using UnityEngine;

public class MissionKeyTest : MonoBehaviour
{
    private void Update()
    {
        if (MissionManager.Instance == null || MissionManager.Instance.CurrentMission == null)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
            TryClearMission(0, "전력 복구");

        if (Input.GetKeyDown(KeyCode.W))
            TryClearMission(1, "데이터 수집");

        if (Input.GetKeyDown(KeyCode.E))
            TryClearMission(2, "출구 잠금 해제");

        if (Input.GetKeyDown(KeyCode.R))
            TryClearMission(3, "탈출");
    }

    private void TryClearMission(int expectedOrder, string missionLabel)
    {
        if (MissionManager.Instance.IsCurrentMissionOrder(expectedOrder))
        {
            Debug.Log($"[MissionTest] {missionLabel} 키 입력 성공");
            MissionManager.Instance.CompleteCurrentMission();
        }
        else
        {
            Debug.Log($"[MissionTest] 지금은 {missionLabel} 단계가 아님");
        }
    }
}