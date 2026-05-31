using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 추가
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }
    public DataMissionSpawner DataMissionSpawner;
    public UnityEngine.XR.Interaction.Toolkit.XRInteractionManager localXRManager;

    // --- 승패 및 역할 구분을 위한 변수 추가 ---
    public enum GameWinner { None, Infiltrator, Chaser }

    [Header("Ending Scenes")]
    public string infiltratorWinScene = "Infiltrator_Win";   // 잠입자 승리 씬
    public string infiltratorLoseScene = "Infiltrator_Lose"; // 잠입자 패배 씬
    public string chaserWinScene = "Chaser_Win";             // 추격자 승리 씬
    public string chaserLoseScene = "Chaser_Lose";           // 추격자 패배 씬

    // 내(로컬)가 잠입자인지 확인하는 플래그 (플레이어 스폰 시 결정됨)
    public bool isLocalPlayerInfiltrator = false;
    private bool isGameOver = false;

    // --- 동기화되는 게임 상태 ---
    [Networked, OnChangedRender(nameof(OnMissionIndexChanged))]
    public int CurrentMissionIndex { get; set; } = 0;
    [Networked, OnChangedRender(nameof(OnDataProgressChanged))]
    public int DataCollectionProgress { get; set; }
    public int MaxDataNodes = 5;
    [Networked] public NetworkBool IsExitOpen { get; set; }
    [Networked] public TickTimer GlobalGameTimer { get; set; }
    public System.Action<int> OnMissionChangedEvent;
    public GameObject exitObject;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            GlobalGameTimer = TickTimer.CreateFromSeconds(Runner, 600f);

            SetAllDoorsState(true);
        }
        OnMissionChangedEvent?.Invoke(CurrentMissionIndex);
        UpdateMissionUI_Local();

        if (!Runner.IsServer && NetworkManager.Instance != null)
        {
            if (NetworkManager.Instance.LocalPlayerRole == "Chaser")
            {
                Debug.Log("<color=cyan>[NetworkGameManager] 네트워크 준비 완료! 안전하게 추격자 스폰 RPC를 발사합니다.</color>");
                RPC_RequestChaserSpawn(Runner.LocalPlayer);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_CompletePowerRestore()
    {
        if (CurrentMissionIndex == 0)
        {
            CurrentMissionIndex = 1;
            if (DataMissionSpawner != null) DataMissionSpawner.SpawnRandomMissions();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_AddDataProgress()
    {
        if (CurrentMissionIndex != 1) return;
        DataCollectionProgress++;
        if (DataCollectionProgress >= MaxDataNodes)
        {
            CurrentMissionIndex = 2;
            OpenExit();
        }
    }

    private void OpenExit()
    {
        IsExitOpen = true;
        RPC_ActivateExitObject();
        CurrentMissionIndex = 3;

        SetAllDoorsState(false);
        Debug.Log("<color=red>[시스템] 탈출구 개방! 기지 전체 문을 폐쇄합니다.</color>");
    }

    private void SetAllDoorsState(bool open)
    {
        // 네트워크 변수를 바꾸는 연산이므로, 오직 서버(State Authority)에서만 실행되도록 방어합니다.
        if (!HasStateAuthority) return;

        // 씬에 배치된 모든 NetworkSplitSlidingDoor 컴포넌트를 탐색합니다.
        NetworkSplitSlidingDoor[] allDoors = FindObjectsByType<NetworkSplitSlidingDoor>(FindObjectsSortMode.None);

        foreach (var door in allDoors)
        {
            if (door != null && door.Object != null)
            {
                // 서버가 해당 문의 권한을 가지고 있다면 변수를 직접 변경합니다.
                // (일반적인 씬 배치 오브젝트들은 방장 서버가 자동으로 권한을 가집니다)
                if (door.Object.HasStateAuthority)
                {
                    door.IsOpen = open;
                }
                else
                {
                    // 혹시라도 클라이언트가 임시 권한을 쥐고 있는 문이 있다면, 
                    // 문 스크립트에 만들어 둔 Rpc_RequestToggleDoor 혹은 강제 상태 동기화 로직을 활용할 수 있습니다.
                    door.IsOpen = open;
                }
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ActivateExitObject()
    {
        if (exitObject != null) exitObject.SetActive(true);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_EscapeSuccess()
    {
        if (CurrentMissionIndex == 3 && IsExitOpen && !isGameOver)
        {
            Debug.Log("[NetworkGameManager] 잠입자 탈출 성공!");
            isGameOver = true;
            RPC_BroadcastGameOver(GameWinner.Infiltrator);
        }
    }

    public void TriggerChaserWin()
    {
        if (HasStateAuthority && !isGameOver)
        {
            Debug.Log("[NetworkGameManager] 추격자 잠입자 제거 성공!");
            isGameOver = true;
            // 서버가 모든 클라이언트에게 "추격자가 이겼다!"고 방송
            RPC_BroadcastGameOver(GameWinner.Chaser);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastGameOver(GameWinner winner)
    {
        string sceneToLoad = "";

        // 각자의 컴퓨터(클라이언트)에서 자신이 어떤 역할인지에 따라 씬 분기
        if (isLocalPlayerInfiltrator)
        {
            // 내가 잠입자일 때
            sceneToLoad = (winner == GameWinner.Infiltrator) ? infiltratorWinScene : infiltratorLoseScene;
        }
        else
        {
            // 내가 추격자일 때
            sceneToLoad = (winner == GameWinner.Chaser) ? chaserWinScene : chaserLoseScene;
        }

        Debug.Log($"게임 종료! 승자: {winner}, 로드할 씬: {sceneToLoad}");

        StartCoroutine(ShutdownAndLoadScene(sceneToLoad));
    }
    private System.Collections.IEnumerator ShutdownAndLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(0.5f);

        if (Runner != null)
        {
            Runner.Shutdown();
        }

        SceneManager.LoadScene(sceneName);
    }
    void OnMissionIndexChanged()
    {
        OnMissionChangedEvent?.Invoke(CurrentMissionIndex);
        UpdateMissionUI_Local();
    }

    void OnDataProgressChanged()
    {
        UpdateMissionUI_Local(); // 데이터 수집도 수치 변할 때 UI 새로고침
    }

    private void UpdateMissionUI_Local()
    {
        if (IntruderStatusUIManager.Instance == null) return;

        // 1. 미션 1 (전력 복구) 완료 여부: 인덱스가 1 이상이 되었을 때
        bool isM1Cleared = CurrentMissionIndex >= 1;

        // 2. 미션 2 (데이터 수집) 진행 비율 (0.0f ~ 1.0f 사잇값 변환)
        float m2ProgressFraction = MaxDataNodes > 0 ? (float)DataCollectionProgress / MaxDataNodes : 0f;

        // 3. 미션 2 완료 여부: 인덱스가 탈출 단계(2 또는 3)로 넘어갔을 때
        bool isM2Cleared = CurrentMissionIndex >= 2;

        // 4. 미션 3 (탈출 완료) 여부: 게임 종료 여부와 연동
        bool isM3Cleared = isGameOver;

        // 이쁘게 가공된 데이터를 아까 만든 UI 매니저로 토스!
        IntruderStatusUIManager.Instance.UpdateMissionUI(isM1Cleared, m2ProgressFraction, isM2Cleared, isM3Cleared);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_PlayGlobalSound(Vector3 position, float lifetime, SoundType soundType)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(position, lifetime, soundType);
            Debug.Log($"🔊 [글로벌 사운드 RPC 수신] 종류: {soundType}, 위치: {position}");
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 null이라 RPC 사운드를 재생할 수 없습니다.");
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestChaserSpawn(PlayerRef player)
    {
        Debug.Log($"[서버 수신] 클라이언트({player.PlayerId})의 요청으로 추격자 프리팹을 안전하게 서버 권한으로 스폰합니다.");

        if (NetworkManager.Instance != null)
        {
            NetworkObject spawnedChaser = Runner.Spawn(
                NetworkManager.Instance.chaserPrefab,
                NetworkManager.Instance.SpawnPoint_chaser,
                Quaternion.identity,
                player // 입력 권한 부여
            );

            NetworkManager.Instance.ChaserObject = spawnedChaser;
        }
    }
}