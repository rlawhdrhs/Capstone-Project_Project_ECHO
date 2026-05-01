using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    private readonly List<MissionBase> missions = new List<MissionBase>();
    private int currentMissionIndex = -1;

    public MissionBase CurrentMission
    {
        get
        {
            if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count)
                return null;

            return missions[currentMissionIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CollectMissionsInScene();
        SortMissions();
        ResetAllMissions();

        Debug.Log($"[Mission] Registered mission count: {missions.Count}");

        if (missions.Count == 0)
        {
            Debug.LogWarning("[Mission] No missions found in scene.");
            return;
        }

        StartNextMission();
    }

    private void CollectMissionsInScene()
    {
        missions.Clear();

        MissionBase[] foundMissions = FindObjectsByType<MissionBase>(FindObjectsSortMode.None);

        foreach (var mission in foundMissions)
        {
            if (!missions.Contains(mission))
            {
                missions.Add(mission);
            }
        }
    }

    private void SortMissions()
    {
        missions.Sort((a, b) => a.missionOrder.CompareTo(b.missionOrder));
    }

    private void ResetAllMissions()
    {
        foreach (var mission in missions)
        {
            mission.ResetMission();
        }
    }

    public void StartNextMission()
    {
        currentMissionIndex++;

        if (currentMissionIndex >= missions.Count)
        {
            Debug.Log("[Mission] All missions completed.");
            OnAllMissionsCompleted();
            return;
        }

        missions[currentMissionIndex].StartMission();
    }

    public void CompleteCurrentMission()
    {
        if (CurrentMission == null) return;

        CurrentMission.CompleteMission();
        StartNextMission();
    }

    public bool IsCurrentMission(MissionBase mission)
    {
        return mission != null && mission == CurrentMission;
    }

    private void OnAllMissionsCompleted()
    {
        Debug.Log("[Mission] Game Clear");
    }

    public bool IsCurrentMissionOrder(int order)
    {
        if (CurrentMission == null) return false;
        return CurrentMission.missionOrder == order;
    }
}