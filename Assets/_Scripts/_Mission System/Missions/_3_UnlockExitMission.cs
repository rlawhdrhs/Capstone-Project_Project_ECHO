using UnityEngine;

public class _3_UnlockExitMission : MissionBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void SetupMission()
    {
        missionName = "출구 잠금 해제";
        missionOrder = 2;
    }
}
