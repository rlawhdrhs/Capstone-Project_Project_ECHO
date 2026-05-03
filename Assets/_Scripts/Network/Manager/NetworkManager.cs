using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    [Header("Scene Settings")]
    public int mainSceneBuildIndex = 1;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject lobbyUI;
    private NetworkRunner _networkRunner;

    [Header("비대칭 플레이어 프리팹")]
    public NetworkPrefabRef infiltratorPrefab; // 잠입자 (Host 전용)
    public NetworkPrefabRef chaserPrefab;      // 추격자 (Client 전용)

    public NetworkObject InfiltratorObject { get; private set; }
    public NetworkObject ChaserObject { get; private set; }

    public Vector3 SpawnPoint_intruder = new Vector3(3, 2, 0);
    public Vector3 SpawnPoint_chaser = new Vector3(0, 2, 0);

    //잠입자(Host) 서버 접속 함수
    public async void StartAsInfiltrator()
    {
        if (lobbyUI != null) lobbyUI.SetActive(false);
        await StartGame(GameMode.Host);
    }

    //추격자(Client) 서버 접속 함수
    public async void StartAsChaser()
    {
        if (lobbyUI != null) lobbyUI.SetActive(false);
        await StartGame(GameMode.Client);
    }

    //게임 시작 함수
    private async Task StartGame(GameMode mode)
    {
        _networkRunner = gameObject.GetComponent<NetworkRunner>();
        if (_networkRunner == null) _networkRunner = gameObject.AddComponent<NetworkRunner>();

        _networkRunner.ProvideInput = true;

        _networkRunner.AddCallbacks(this);

        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "ProjectECHO_Room",
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(mainSceneBuildIndex)
        };

        var result = await _networkRunner.StartGame(startGameArgs);
        if (result.Ok == false) // 접속에 실패했다면
        {
            //Debug.LogError($"접속 실패 원인: {result.ShutdownReason}");
            Debug.LogError($"접속 실패 원인: {result.ShutdownReason} / 상세 에러: {result.ErrorMessage}");
            Debug.Log($"현재 씬 인덱스: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex}");
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
                InfiltratorObject = runner.Spawn(infiltratorPrefab, SpawnPoint_intruder, Quaternion.identity, player);
            }
            else
            {
                Debug.Log("클라이언트 접속: 추격자 프리팹 생성 및 권한 부여");
                ChaserObject = runner.Spawn(chaserPrefab, SpawnPoint_chaser, Quaternion.identity, player);
            }
        }
    }

    // 플레이어 입력 데이터 전송
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // 1. 입력 데이터를 담을 구조체 생성
        NetworkInputData data = new NetworkInputData();

        // 2. WASD 키 입력 수집
        // 앞/뒤
        if (Input.GetKey(KeyCode.W)) data.moveZ = 1f;
        if (Input.GetKey(KeyCode.S)) data.moveZ = -1f;

        // 좌/우 회전 (A/D)
        if (Input.GetKey(KeyCode.A)) data.turnY = -1f;
        if (Input.GetKey(KeyCode.D)) data.turnY = 1f;

        float rightTrigger = Input.GetAxis("XRI_Right_Trigger");
        data.rightTrigger = Input.GetKey(KeyCode.R) || rightTrigger > 0.1f;

        // 마우스 좌클릭
        data.leftClick = Input.GetMouseButton(0);
        // 점프 (Space)
        data.jump = Input.GetKey(KeyCode.Space) || Input.GetButton("XRI_Left_GripButton");

        if (LocalVRRig.Instance != null)
        {
            data.headPosition = LocalVRRig.Instance.hardwareHead.position;
            data.headRotation = LocalVRRig.Instance.hardwareHead.rotation;

            data.leftHandPosition = LocalVRRig.Instance.hardwareLeftHand.position;
            data.leftHandRotation = LocalVRRig.Instance.hardwareLeftHand.rotation;

            data.rightHandPosition = LocalVRRig.Instance.hardwareRightHand.position;
            data.rightHandRotation = LocalVRRig.Instance.hardwareRightHand.rotation;

            data.rootPosition = LocalVRRig.Instance.transform.position;
            data.rootRotation = LocalVRRig.Instance.transform.rotation;

            // 애니메이션용 데이터 (기존 로직 활용)
            data.moveX = Input.GetAxis("Horizontal");
            data.moveZ = Input.GetAxis("Vertical");
            data.crouch = LocalVRRig.Instance.currentCrouch;
        }

        input.Set(data);
    }

    // =========================================================
    #region Unused Callbacks 
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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