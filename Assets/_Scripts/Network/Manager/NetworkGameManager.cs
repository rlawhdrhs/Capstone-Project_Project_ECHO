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
    [Networked] public int DataCollectionProgress { get; set; }
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
        }
        OnMissionChangedEvent?.Invoke(CurrentMissionIndex);
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
    }
}