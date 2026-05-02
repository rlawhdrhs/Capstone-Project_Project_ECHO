using UnityEngine;
using UnityEngine.UI; // 임시 테스트용 레거시 UI
// using TMPro; // 나중에 TMP로 바꿀 때 사용

public class LobbyManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMenuPanel;      // [Play] 버튼이 있는 패널
    public GameObject lobbySelectionPanel; // [방 생성], [방 참가] 버튼이 있는 패널
    //public GameObject createRoomPanel;    // 방 이름을 입력하는 패널 (선택 사항)
    //public GameObject roomListPanel;      // 방 리스트가 뜨는 패널 (선택 사항)

    //[Header("Optional Input")]
    //public InputField roomNameInputField; // 방 이름을 직접 입력받고 싶을 때 사용

    private void Start()
    {
        // 초기화: 스타트 메뉴만 켜두고 나머지는 끈다.
        ShowPanel(startMenuPanel);
    }

    // 1. [Play] 버튼을 눌렀을 때 호출
    public void OnPlayButtonPressed()
    {
        ShowPanel(lobbySelectionPanel);
    }

    // 2. [방 생성] 버튼을 눌렀을 때 호출
    public void OnCreateRoomButtonPressed()
    {
        if (NetworkManager.Instance != null)
        {
            // 잠입자(Host)로 접속 시작
            NetworkManager.Instance.StartAsInfiltrator();
            ShowPanel(null);
        }
        else
        {
            Debug.LogError("NetworkManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    // 3. [방 참가] 버튼을 눌렀을 때 호출
    public void OnJoinRoomButtonPressed()
    {
        if (NetworkManager.Instance != null)
        {
            // 추격자(Client)로 접속 시작
            NetworkManager.Instance.StartAsChaser();

            ShowPanel(null);
        }
    }

    // [뒤로 가기] 버튼용
    public void OnBackButtonPressed()
    {
        ShowPanel(startMenuPanel);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        startMenuPanel.SetActive(startMenuPanel == panelToShow);
        lobbySelectionPanel.SetActive(lobbySelectionPanel == panelToShow);
        //if (createRoomPanel != null) createRoomPanel.SetActive(createRoomPanel == panelToShow);
        //if (roomListPanel != null) roomListPanel.SetActive(roomListPanel == panelToShow);
    }
}