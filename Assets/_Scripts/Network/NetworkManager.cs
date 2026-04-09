using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [Header("비대칭 플레이어 프리팹")]
    public NetworkPrefabRef infiltratorPrefab; // 잠입자 (Host 전용)
    public NetworkPrefabRef chaserPrefab;      // 추격자 (Client 전용)

    //잠입자(Host) 서버 접속 함수
    public async void StartAsInfiltrator()
    {
        Debug.Log("잠입자(Host)로 서버 시작 중...");
        await StartGame(GameMode.Host);
    }

    //추격자(Client) 서버 접속 함수
    public async void StartAsChaser()
    {
        Debug.Log("추격자(Client)로 서버 접속 중...");
        await StartGame(GameMode.Client);
    }

    //게임 시작 함수
    private async Task StartGame(GameMode mode)
    {
        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();

        _runner.ProvideInput = true;

        _runner.AddCallbacks(this);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "ProjectECHO_Room",
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        var result = await _runner.StartGame(startGameArgs);

        if (!result.Ok)
        {
            Debug.LogError("접속 실패: " + result.ShutdownReason);
        }
    }

    //플레이어 서버 접속 및 스폰 함수
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (player == runner.LocalPlayer)
            {
                Debug.Log("호스트 스폰: 잠입자 프리팹 생성");
                runner.Spawn(infiltratorPrefab, Vector3.zero, Quaternion.identity, player);
            }
            else
            {
                Debug.Log("클라이언트 접속: 추격자 프리팹 생성 및 권한 부여");
                runner.Spawn(chaserPrefab, new Vector3(2, 0, 0), Quaternion.identity, player);
            }
        }
    }

    // =========================================================
    #region Unused Callbacks 
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason info) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}