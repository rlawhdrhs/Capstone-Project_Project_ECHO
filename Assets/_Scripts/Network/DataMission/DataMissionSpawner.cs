using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DataMissionSpawner : NetworkBehaviour
{
    [Header("Mission Spawner Settings")]
    public NetworkObject dataMissionPrefab;
    //스폰 위치 저장
    public Transform[] spawnPoints;
    //스폰 개수
    int spawnCount = 5;

    public UnityEngine.XR.Interaction.Toolkit.XRInteractionManager localXRManager;

    public void SpawnRandomMissions()
    {
        if (NetworkGameManager.Instance != null)
            spawnCount = NetworkGameManager.Instance.MaxDataNodes;

        if (dataMissionPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("미션 프리팹 또는 스폰 포인트가 인스펙터에 할당되지 않았습니다!");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int actualSpawnCount = Mathf.Min(spawnCount, availablePoints.Count);

        for (int i = 0; i < actualSpawnCount; i++)
        {
            // 하나를 무작위로 선택
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform selectedPoint = availablePoints[randomIndex];

            // 선택된 위치에 미션 프리팹을 네트워크 스폰합니다.
            NetworkObject spawnedNode = Runner.Spawn(dataMissionPrefab, selectedPoint.position, selectedPoint.rotation);

            // 4. 이미 미션을 스폰한 위치는 리스트에서 제거하여 중복 스폰을 막습니다.
            availablePoints.RemoveAt(randomIndex);

            XRSimpleInteractable interactable = spawnedNode.GetComponent<XRSimpleInteractable>();

            if (interactable != null && localXRManager != null)
            {
                interactable.interactionManager = localXRManager;
            }
        }
    }
}