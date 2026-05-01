using UnityEngine;

public abstract class MissionBase : MonoBehaviour
{
    [Header("Mission Info")]
    public string missionName;
    public int missionOrder;

    public bool IsActive { get; private set; }
    public bool IsCompleted { get; private set; }

    protected virtual void Awake()
    {
        SetupMission();
    }

    protected abstract void SetupMission();

    public virtual void StartMission()
    {
        IsActive = true;
        IsCompleted = false;
        Debug.Log($"[Mission] Start: {missionName}");
    }

    public virtual void CompleteMission()
    {
        if (IsCompleted) return;

        IsCompleted = true;
        IsActive = false;
        Debug.Log($"[Mission] Complete: {missionName}");
    }

    public virtual void ResetMission()
    {
        IsActive = false;
        IsCompleted = false;
    }

    public virtual bool CanInteract()
    {
        return IsActive && !IsCompleted;
    }
}