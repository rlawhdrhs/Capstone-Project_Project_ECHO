using UnityEngine;
using UnityEngine.UI; // 임시 테스트용 레거시 UI
// using TMPro; // 나중에 TMP로 바꿀 때 사용

public class LobbyManager : MonoBehaviour
{
    // [방 생성] 버튼을 눌렀을 때 호출
    public void OnCreateRoomButtonPressed()
    {
        if (NetworkManager.Instance != null)
        {
            // 잠입자(Host)로 접속 시작
            NetworkManager.Instance.StartAsInfiltrator();
        }
        else
        {
            Debug.LogError("NetworkManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // [방 참가] 버튼을 눌렀을 때 호출
    public void OnJoinRoomButtonPressed()
    {
        if (NetworkManager.Instance != null)
        {
            // 추격자(Client)로 접속 시작
            NetworkManager.Instance.StartAsChaser();
        }
    }
}