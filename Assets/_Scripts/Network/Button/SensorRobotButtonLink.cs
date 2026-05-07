using UnityEngine;

public class SensorRobotButtonLink : MonoBehaviour
{
    [Header("이 버튼을 누르면 빙의할 타겟 드론(센서 로봇)")]
    // 맵 씬에 미리 배치되어 있는 드론
    public SensorSynchronizer targetDrone;

    // VRButton의 onClick UnityEvent에서 이 함수를 호출
    public void TryPossessRobot()
    {
        if (PossessionManager.Instance != null)
        {
            if (targetDrone != null)
            {
                PossessionManager.Instance.PossessDrone(targetDrone);
                Debug.Log($"{targetDrone.gameObject.name} 드론으로 빙의 요청 성공!");
            }
            else
            {
                Debug.LogWarning("타겟 드론이 할당되지 않았습니다! 인스펙터를 확인해주세요.");
            }
        }
        else
        {
            Debug.LogWarning("PossessionManager(추격자)를 씬에서 찾을 수 없습니다.");
        }
    }
}