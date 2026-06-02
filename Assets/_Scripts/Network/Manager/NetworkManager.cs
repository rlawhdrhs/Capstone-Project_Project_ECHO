using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef grabObjectPrefab;
    public Vector3 SpawnPoint_grabObject = new Vector3(-15, 5f, 20f);

    public static NetworkManager Instance;

    private InputAction rightAButton;
    private InputAction rightBButton;
    private InputAction rightTrigger;
    private InputAction rightGrip;
    private InputAction leftXButton;
    private InputAction leftYButton;
    private InputAction leftMenuButton;
    private InputAction leftGrip;

    public bool IsLeftGripPressed => leftGrip != null && leftGrip.IsPressed();
    public bool IsLeftGripDown => leftGrip != null && leftGrip.WasPressedThisFrame();
    public bool IsLeftGripUp => leftGrip != null && leftGrip.WasReleasedThisFrame();
    public bool IsLeftYDown => leftYButton != null && leftYButton.WasPressedThisFrame();
    public bool IsLeftMenuDown => leftMenuButton != null && leftMenuButton.WasPressedThisFrame();

    [Header("Scene Settings")]
    public int mainSceneBuildIndex = 1;

    [Header("Lobby & Prefabs")]
    public GameObject lobbyUI;
    private NetworkRunner _networkRunner;

    public NetworkPrefabRef infiltratorPrefab; // 잠입자 (Host 전용)
    public NetworkPrefabRef chaserPrefab;      // 추격자 (Client 전용)
    public GameObject spectatorCameraPrefab;  // [추가] 촬영용 카메라 프리랩 (일반 게임오브젝트)

    // [추가] 내 로컬 컴퓨터의 역할을 저장하는 변수 ("Infiltrator", "Chaser", "Spectator")
    private string _localPlayerRole = "Infiltrator";

    public NetworkObject InfiltratorObject { get; private set; }
    public NetworkObject ChaserObject { get; set; }
    public NetworkRunner Runner => _networkRunner;
    public string LocalPlayerRole => _localPlayerRole;

    public Vector3 SpawnPoint_intruder = new Vector3(3, 2, 0);
    public Vector3 SpawnPoint_chaser = new Vector3(0, 2, 0);

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
            return;
        }

        // New Input System 바인딩 코드 동일
        rightAButton = new InputAction(binding: "<XRController>{RightHand}/primaryButton");
        leftXButton = new InputAction(binding: "<XRController>{LeftHand}/primaryButton");
        leftYButton = new InputAction(binding: "<XRController>{LeftHand}/secondaryButton");
        leftMenuButton = new InputAction(binding: "<XRController>{LeftHand}/menu");
        rightTrigger = new InputAction(binding: "<XRController>{RightHand}/trigger");
        leftGrip = new InputAction(binding: "<XRController>{LeftHand}/grip");
        rightBButton = new InputAction(binding: "<XRController>{RightHand}/secondaryButton");
        rightGrip = new InputAction(binding: "<XRController>{RightHand}/grip");

        rightAButton.Enable(); leftXButton.Enable(); leftYButton.Enable(); leftMenuButton.Enable();
        rightTrigger.Enable(); leftGrip.Enable(); rightBButton.Enable(); rightGrip.Enable();
    }

    public async void StartAsInfiltrator()
    {
        _localPlayerRole = "Infiltrator";
        if (lobbyUI != null) lobbyUI.SetActive(false);
        await StartGame(GameMode.Host);
    }

    public async void StartAsChaser()
    {
        _localPlayerRole = "Chaser";
        if (lobbyUI != null) lobbyUI.SetActive(false);
        await StartGame(GameMode.Client);
    }

    // [추가] 촬영감독(Spectator) 모드로 서버에 접속하는 함수
    public async void StartAsSpectator()
    {
        _localPlayerRole = "Spectator";
        if (lobbyUI != null) lobbyUI.SetActive(false);
        await StartGame(GameMode.Client); // 네트워크상에서는 클라이언트로 참여합니다.
    }

    private async Task StartGame(GameMode mode)
    {
        _networkRunner = gameObject.GetComponent<NetworkRunner>();
        if (_networkRunner == null) _networkRunner = gameObject.AddComponent<NetworkRunner>();

        _networkRunner.ProvideInput = true;
        _networkRunner.AddCallbacks(this);

        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "ProjectECHO_Room",
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(mainSceneBuildIndex)
        };

        var result = await _networkRunner.StartGame(startGameArgs);
        if (result.Ok == false)
        {
            Debug.LogError($"접속 실패 원인: {result.ShutdownReason} / 상세 에러: {result.ErrorMessage}");
        }
    }

    // [수정] 무조건 추격자를 뽑던 구조에서 호스트(잠입자)만 즉시 생성하도록 변경
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (player == runner.LocalPlayer)
            {
                Debug.Log("호스트 스폰: 잠입자 프리팹 생성");
                InfiltratorObject = runner.Spawn(infiltratorPrefab, SpawnPoint_intruder, Quaternion.identity, player);

                if (grabObjectPrefab.IsValid)
                {
                    runner.Spawn(grabObjectPrefab, SpawnPoint_grabObject, Quaternion.identity, player);
                    Debug.Log("<color=yellow>[Fusion] 그랩 오브젝트가 호스트(서버) 소유권으로 정상 스폰되었습니다!</color>");
                }
            }
            // 일반 클라이언트(추격자, 촬영팀)들은 씬 로드가 끝난 시점에 각자 역할을 서버에 요청하게 됩니다.
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        UnityEngine.SceneManagement.Scene targetMainScene = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(mainSceneBuildIndex);

        if (targetMainScene.isLoaded)
        {
            // 2. [추가] 유니티의 활성화된 씬 자체를 메인 씬으로 변경합니다. (이래야 물리 세계가 합쳐집니다)
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(targetMainScene);
        }
        else
        {
            Debug.LogError($"[Fusion Fix] 메인 씬(인덱스:{mainSceneBuildIndex})을 찾을 수 없거나 로드되지 않았습니다.");
        }

        if (LocalVRRig.Instance != null)
        {
            // 3. [수정] 이제 함정 카드가 아닌, 진짜 목적지인 메인 씬으로 VR Rig를 이동시킵니다.
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(
                LocalVRRig.Instance.gameObject,
                targetMainScene
            );
            Debug.Log("<color=lime><b>[물리 대통합] 로컬 VR Rig를 진짜 메인 네트워크 물리 씬으로 안전하게 이전했습니다!</b></color>");

            // XRI 오작동 방지용 손 리프레시 코드 유지
            if (LocalVRRig.Instance.hardwareLeftHand != null)
            {
                LocalVRRig.Instance.hardwareLeftHand.gameObject.SetActive(false);
                LocalVRRig.Instance.hardwareLeftHand.gameObject.SetActive(true);
            }
            if (LocalVRRig.Instance.hardwareRightHand != null)
            {
                LocalVRRig.Instance.hardwareRightHand.gameObject.SetActive(false);
                LocalVRRig.Instance.hardwareRightHand.gameObject.SetActive(true);
            }
        }

        if (!runner.IsServer)
        {
            if (_localPlayerRole == "Spectator")
            {
                if (spectatorCameraPrefab != null)
                {
                    Instantiate(spectatorCameraPrefab, new Vector3(0, 10, 0), Quaternion.identity);
                }
            }
        }
    }

    // [추가] 추격자 전용 스폰 요청 RPC
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestChaserSpawn(PlayerRef player)
    {
        Debug.Log("클라이언트 요청 수신: 추격자 프리팹 생성 및 권한 부여");
        ChaserObject = Runner.Spawn(chaserPrefab, SpawnPoint_chaser, Quaternion.identity, player);
    }

    // [이하 기존 OnInput 및 RPC, 가상 함수 인터페이스 로직 동일]
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (LocalVRRig.Instance != null && !LocalVRRig.Instance.isOnlineMode) return;
        NetworkInputData data = new NetworkInputData();

        bool isRightAPressed = rightAButton.IsPressed();
        bool isRightBPressed = rightBButton.IsPressed() || Input.GetKey(KeyCode.B);
        bool isLeftAPressed = leftXButton.IsPressed();
        bool isLeftBPressed = leftYButton.IsPressed() || Input.GetKey(KeyCode.Y);
        float rightTriggerValue = rightTrigger.ReadValue<float>();
        bool isLeftGripPressed = leftGrip.IsPressed();
        bool isRightGripPressed = rightGrip.IsPressed() || Input.GetKey(KeyCode.G);

        data.rightTrigger = Input.GetKey(KeyCode.R) || rightTriggerValue > 0.1f;
        data.leftButtonA = Input.GetKey(KeyCode.X) || isLeftAPressed;
        data.jump = isLeftGripPressed;
        data.keySpace = Input.GetKey(KeyCode.Space) || isRightAPressed;
        data.rightButtonB = isRightBPressed;
        data.rightGripPressed = isRightGripPressed;
        data.leftClick = Input.GetMouseButton(0);

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

    public void RequestCmdExplosion(Vector3 emitPosition, float radius) { if (_networkRunner != null && _networkRunner.IsRunning) RPC_ExplodeAndOpenDoors(emitPosition, radius); }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_ExplodeAndOpenDoors(Vector3 emitPosition, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(emitPosition, radius);
        foreach (var hit in hitColliders)
        {
            NetworkSplitSlidingDoor door = hit.GetComponentInParent<NetworkSplitSlidingDoor>();
            if (door != null && !door.IsOpen)
            {
                if (_networkRunner.IsServer) { door.ToggleDoor(); }
            }
        }
    }

    public bool CheckIfLocalPlayerIsChaser()
    {
        if (_networkRunner == null || !_networkRunner.IsRunning) return false;
        return _networkRunner.IsClient && _localPlayerRole == "Chaser";
    }

    public void RegisterInfiltrator(NetworkObject infiltrator) { InfiltratorObject = infiltrator; }

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
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}