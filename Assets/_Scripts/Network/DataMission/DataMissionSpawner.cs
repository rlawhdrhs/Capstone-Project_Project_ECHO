using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DataMissionSpawner : NetworkBehaviour
{
    [Header("Mission Spawner Settings")]
    public NetworkObject dataMissionPrefab;
    public Transform[] spawnPoints;
    public int spawnCount = 5;

    public UnityEngine.XR.Interaction.Toolkit.XRInteractionManager localXRManager;

    // ★ [추가] 퓨전 네트워크가 시작되는 시점에 호출되는 초기화 관문
    public override void Spawned()
    {
        // 게임 시작 시 모든 스폰 후보지의 미니맵 표시를 자동으로 꺼둡니다.
        HideAllMinimapObjects();
    }

    private void HideAllMinimapObjects()
    {
        if (spawnPoints == null) return;

        foreach (var point in spawnPoints)
        {
            if (point != null && point.parent != null)
            {
                // 스폰 포인트의 부모(미션 부모 오브젝트)의 자식들을 탐색
                foreach (Transform child in point.parent)
                {
                    // 자기 자신(스폰포인트)과 라이트가 아닌 미니맵 오브젝트를 찾아서 비활성화
                    if (child != point && (child.name.Contains("미니맵") || child.name.ToLower().Contains("minimap")))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }
        Debug.Log("<color=yellow>[DataMissionSpawner] 모든 스폰 후보지의 미니맵 아이콘을 초기화(숨김) 했습니다.</color>");
    }

    public void SpawnRandomMissions()
    {
        if (!Object.HasStateAuthority) return;

        if (dataMissionPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("미션 프리팹 또는 스폰 포인트가 인스펙터에 할당되지 않았습니다!");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        int actualSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            // 선택된 위치에 미션 프리팹을 네트워크 스폰합니다.
            NetworkObject spawnedNode = Runner.Spawn(dataMissionPrefab, selectedPoint.position, selectedPoint.rotation);

            availablePoints.RemoveAt(randomIndex);

            XRSimpleInteractable interactable = spawnedNode.GetComponent<XRSimpleInteractable>();
            if (interactable != null && localXRManager != null)
            {
                interactable.interactionManager = localXRManager;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestSpawnNextMission()
    {
        SpawnRandomMissions();
    }
}