using UnityEngine;

public class MissionCheatManager : MonoBehaviour
{
    void Update()
    {
        // 키보드 P 키를 누르면 실행
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 씬에 존재하는 모든 DataMissionInteractor 컴포넌트를 검색합니다.
            DataMissionInteractor[] missions = FindObjectsByType<DataMissionInteractor>(FindObjectsSortMode.None);

            bool cheatSuccess = false;

            foreach (var mission in missions)
            {
                // 아직 클리어되지 않은 미션을 찾았다면
                if (!mission.IsCleared)
                {
                    Debug.Log($"<color=cyan>[CHEAT] 아직 완료되지 않은 미션 발견({mission.name}). 즉시 강제 클리어합니다.</color>");
                    mission.ClearMission(); // 해당 미션 강제 클리어 권한 행사

                    cheatSuccess = true;
                    break; // ★ 중요: 하나만 깨고 루프를 탈출하여 한 번 누를 때 '하나씩만' 깨지도록 제한합니다.
                }
            }

            if (!cheatSuccess)
            {
                Debug.Log("<color=orange>[CHEAT] 씬 내에 더 이상 남아있는 미션이 없습니다!</color>");
            }
        }
    }
}