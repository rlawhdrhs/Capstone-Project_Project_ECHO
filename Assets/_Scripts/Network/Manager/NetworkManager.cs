using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    private InputAction rightAButton;
    private InputAction rightBButton;
    private InputAction leftXButton;
    private InputAction rightTrigger;
    private InputAction leftGrip;
    private InputAction rightGrip;

    public bool IsLeftGripPressed => leftGrip != null && leftGrip.IsPressed();
    public bool IsLeftGripDown => leftGrip != null && leftGrip.WasPressedThisFrame();
    public bool IsLeftGripUp => leftGrip != null && leftGrip.WasReleasedThisFrame();

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
        // New Input System의 하드웨어 경로(Path)를 코드로 직접 바인딩합니다.
        rightAButton = new InputAction(binding: "<XRController>{RightHand}/primaryButton");
        leftXButton = new InputAction(binding: "<XRController>{LeftHand}/primaryButton");
        rightTrigger = new InputAction(binding: "<XRController>{RightHand}/trigger");
        leftGrip = new InputAction(binding: "<XRController>{LeftHand}/grip");
        rightBButton = new InputAction(binding: "<XRController>{RightHand}/secondaryButton");
        rightGrip = new InputAction(binding: "<XRController>{RightHand}/grip");

        // 사용 가능하도록 활성화
        rightAButton.Enable();
        leftXButton.Enable();
        rightTrigger.Enable();
        leftGrip.Enable();
        rightBButton.Enable();
        rightGrip.Enable();
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
        if (LocalVRRig.Instance != null && !LocalVRRig.Instance.isOnlineMode) return;

        NetworkInputData data = new NetworkInputData();

        // .IsPressed()나 .ReadValue<float>()로 아주 간단하게 값을 가져옵니다.
        bool isRightAPressed = rightAButton.IsPressed();
        bool isRightBPressed = rightBButton.IsPressed() || Input.GetKey(KeyCode.B);
        bool isLeftAPressed = leftXButton.IsPressed();
        float rightTriggerValue = rightTrigger.ReadValue<float>();
        bool isLeftGripPressed = leftGrip.IsPressed();
        bool isRightGripPressed = rightGrip.IsPressed() || Input.GetKey(KeyCode.G);

        // 퓨전 데이터 매핑 (키보드 디버깅용 레거시 유지)
        data.rightTrigger = Input.GetKey(KeyCode.R) || rightTriggerValue > 0.1f;
        data.leftButtonA = Input.GetKey(KeyCode.X) || isLeftAPressed;
        data.jump = isLeftGripPressed;
        data.keySpace = Input.GetKey(KeyCode.Space) || isRightAPressed;
        data.rightButtonB = isRightBPressed;
        data.rightGripPressed = isRightGripPressed;
        data.leftClick = Input.GetMouseButton(0);

        // [이하 기존 위치/회전 동기화 로직 동일]
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
            data.moveX = Input.GetAxis("Horizontal");
            data.moveZ = Input.GetAxis("Vertical");
            data.crouch = LocalVRRig.Instance.currentCrouch;
        }
        if (PossessionManager.Instance != null)
        {
            data.isPossessingDrone = PossessionManager.Instance.currentDrone != null;
        }

        input.Set(data);
    }

    public void RequestCmdExplosion(Vector3 emitPosition, float radius)
    {
        if (_networkRunner != null && _networkRunner.IsRunning)
        {
            // 포톤 RPC 함수를 가동합니다.
            RPC_ExplodeAndOpenDoors(emitPosition, radius);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ExplodeAndOpenDoors(Vector3 emitPosition, float radius)
    {
        Debug.Log($"<color=cyan>[네트워크 RPC] {emitPosition} 좌표에서 EMP 폭발 수신! 주변 문을 확인합니다.</color>");

        // 모든 사람들의 화면에서 해당 좌표 주변의 문을 센싱해서 엽니다.
        Collider[] hitColliders = Physics.OverlapSphere(emitPosition, radius);

        foreach (var hit in hitColliders)
        {
            NetworkSplitSlidingDoor door = hit.GetComponentInParent<NetworkSplitSlidingDoor>();

            if (door != null && !door.IsOpen)
            {
                // 문을 제어하는 주권(State Authority)이 있는 사람(보통 Host/Server)만 진짜 문을 토글합니다.
                if (_networkRunner.IsServer)
                {
                    door.ToggleDoor();
                    Debug.Log($"<color=lime>[서버 판정] {door.gameObject.name} 문 열기 성공!</color>");
                }
            }
        }
    }

    public bool CheckIfLocalPlayerIsChaser()
    {
        // 런너가 꺼져있거나 작동 중이 아니라면 기본값(false) 리턴
        if (_networkRunner == null || !_networkRunner.IsRunning)
        {
            return false;
        }

        // ★ 핵심 판정: 이 프로젝트 구조상 Host는 잠입자이고 Client는 추격자입니다.
        // 따라서 현재 로컬 컴퓨터의 런너가 Client 모드라면 '추격자'인 상태(true)입니다.
        return _networkRunner.IsClient;
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