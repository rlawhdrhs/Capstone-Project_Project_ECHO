using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class ConnectionManager : MonoBehaviour
{
    private NetworkRunner _runner;

    // 서버 접속 시도
    async void Start()
    {
        await StartConnection();
    }

    private async Task StartConnection()
    {
        // 1. NetworkRunner 컴포넌트 가져오기
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        // 2. 입력 처리 활성화
        _runner.ProvideInput = true;

        // 3. 공유 모드 접속 세팅
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "VR_Stealth_Room",
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        };
        Debug.Log("서버 접속 시도 중...");
        var result = await _runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log("공유 모드 방 접속 성공! 현재 세션: " + _runner.SessionInfo.Name);
        }
        else
        {
            Debug.LogError("접속 실패: " + result.ShutdownReason);
        }
    }
}