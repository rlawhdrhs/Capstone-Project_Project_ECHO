using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class ConnectionManager : MonoBehaviour
{
    private NetworkRunner _runner;

    [Header("플레이어 프리팹 할당")]
    public NetworkPrefabRef chaserPrefab;      // 추격자 프리팹
    public NetworkPrefabRef infiltratorPrefab; // 잠입자 프리팹

    // 서버 접속 시도
    async void Start()
    {
        await StartConnection();
    }

    private async Task StartConnection()
    {
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        _runner.ProvideInput = true;

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
            SpawnPlayer(); // 접속 성공 시 스폰 함수 실행
        }
        else
        {
            Debug.LogError("접속 실패: " + result.ShutdownReason);
        }
    }

    // 접속 순서에 따라 비대칭 스폰을 진행하는 함수
    private void SpawnPlayer()
    {
        if (_runner.IsSharedModeMasterClient)
        {
            Debug.Log("추격자(AI) 스폰");
            // 추격자 소환 (위치: 0,0,0)
            _runner.Spawn(chaserPrefab, Vector3.zero, Quaternion.identity, _runner.LocalPlayer);
        }
        else
        {
            Debug.Log("잠입자(VR) 스폰");
            // 잠입자 소환 (위치: 약간 옆인 2,0,0에 소환해서 겹치지 않게 함)
            _runner.Spawn(infiltratorPrefab, new Vector3(2, 0, 0), Quaternion.identity, _runner.LocalPlayer);
        }
    }
}