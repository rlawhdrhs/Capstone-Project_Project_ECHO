using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    // [방 생성] 버튼 (잠입자/Host)
    public void OnCreateRoomButtonPressed()
    {
        if (NetworkManager.Instance != null) NetworkManager.Instance.StartAsInfiltrator();
        else Debug.LogError("NetworkManager 인스턴스를 찾을 수 없습니다!");
    }

    // [방 참가] 버튼 (추격자/Client)
    public void OnJoinRoomButtonPressed()
    {
        if (NetworkManager.Instance != null) NetworkManager.Instance.StartAsChaser();
    }

    // [촬영감독 참가] 버튼을 UI에 만들고 이 함수를 연결해 줍니다.
    public void OnSpectatorButtonPressed()
    {
        if (NetworkManager.Instance != null) NetworkManager.Instance.StartAsSpectator();
    }
}