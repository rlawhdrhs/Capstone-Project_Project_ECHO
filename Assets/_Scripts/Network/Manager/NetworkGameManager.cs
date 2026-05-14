using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }
    public DataMissionSpawner DataMissionSpawner;
    public UnityEngine.XR.Interaction.Toolkit.XRInteractionManager localXRManager;

    // --- 동기화되는 게임 상태 ---
    // 현재 진행 중인 미션 번호 (0: 전력 복구, 1: 데이터 수집, 2: 출구 해제, 3: 탈출)
    [Networked, OnChangedRender(nameof(OnMissionIndexChanged))]
    public int CurrentMissionIndex { get; set; } = 0;

    [Networked] public int DataCollectionProgress { get; set; } // 완료된 데이터 노드 개수
    public int MaxDataNodes = 5; // 총 해야할 데이터 미션 개수

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
    // --- 1. 전력 복구 (불 켜기) 완료 요청 ---
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_CompletePowerRestore()
    {
        // 방장만 이 코드를 실행함
        if (CurrentMissionIndex == 0)
        {
            CurrentMissionIndex = 1; // 다음 미션(데이터 수집)으로 이동
            Debug.Log("[NetworkGameManager] 전력 복구 완료! 데이터 수집 미션 시작.");

            // 데이터 노드 스폰
            if (DataMissionSpawner != null)
            {
                DataMissionSpawner.SpawnRandomMissions();
            }
        }
    }

    // --- 2. 데이터 수집 진행 요청 ---
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_AddDataProgress()
    {
        if (CurrentMissionIndex != 1) return; // 데이터 수집 단계가 아니면 무시

        DataCollectionProgress++;
        Debug.Log($"[NetworkGameManager] 데이터 수집 진행도: {DataCollectionProgress}/{MaxDataNodes}");

        // 5개를 모두 수집했다면?
        if (DataCollectionProgress >= MaxDataNodes)
        {
            CurrentMissionIndex = 2; // 출구 잠금 해제 미션으로 이동
            OpenExit();
        }
    }

    // --- 3. 출구 개방 ---
    private void OpenExit()
    {
        IsExitOpen = true;
        //EscapeTimer = TickTimer.CreateFromSeconds(Runner, 120f);
        Debug.Log("[NetworkGameManager] 출구가 개방되었습니다!");

        RPC_ActivateExitObject();

        CurrentMissionIndex = 3; // 탈출 미션 활성화
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ActivateExitObject()
    {
        if (exitObject != null)
        {
            exitObject.SetActive(true); // 각자의 로컬 환경에서 오브젝트를 켬
            Debug.Log("출구 오브젝트가 활성화되었습니다!");
        }
    }

    // --- 4. 최종 탈출 요청 ---
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_EscapeSuccess()
    {
        if (CurrentMissionIndex == 3 && IsExitOpen)
        {
            Debug.Log("[NetworkGameManager] 잠입자 탈출 성공! 게임 종료.");
            // TODO: 엔딩 씬으로 이동
        }
    }

    // --- 미션 변경 시 클라이언트에서 호출되는 콜백 ---
    void OnMissionIndexChanged()
    {
        // 로컬 UI를 업데이트
        OnMissionChangedEvent?.Invoke(CurrentMissionIndex);
    }
}