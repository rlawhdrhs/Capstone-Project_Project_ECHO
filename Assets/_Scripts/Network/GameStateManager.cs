using Fusion;
using UnityEngine;

public class GameStateManager : NetworkSingleton<GameStateManager>
{
    // 미션 상태 동기화
    [Networked] public NetworkBool IsPowerRestored { get; set; }
    [Networked] public float DataCollectionProgress { get; set; }
    [Networked] public NetworkBool IsExitOpen { get; set; }

    // 시간 동기화
    [Networked] public TickTimer EscapeTimer { get; set; }

    public void StartEscapeTimer(float duration)
    {
        if (Runner.IsServer) // 시간 설정은 호스트만 가능
        {
            EscapeTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
    }

    public float GetRemainingTime()
    {
        // 남은 시간을 초 단위로 반환
        return EscapeTimer.RemainingTime(Runner) ?? 0;
    }


    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void Rpc_AddDataProgress(float amount)
    {
        DataCollectionProgress = Mathf.Clamp01(DataCollectionProgress + amount);

        if (DataCollectionProgress >= 1f)
        {
            IsExitOpen = true;
            Debug.Log("탈출 문 개방");
        }
    }
}