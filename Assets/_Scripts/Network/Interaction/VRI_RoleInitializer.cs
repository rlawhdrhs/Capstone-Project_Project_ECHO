using UnityEngine;
using Fusion;
using System.Collections;

public class VRI_RoleInitializer : MonoBehaviour
{
    private ChaserElectricShock _shockScript;
    private ChaserDualRadar _radarScript;
    private RunawayStatus _runawayScript;

    void Awake()
    {
        // 내 XR Origin에 붙어있는 스크립트들 참조 전석 확보
        _shockScript = GetComponent<ChaserElectricShock>();
        _radarScript = GetComponent<ChaserDualRadar>();
        _runawayScript = GetComponent<RunawayStatus>();

        // 네트워크가 켜지기 전까지는 에러 방지를 위해 일단 전부 꺼둡니다.
        if (_shockScript) _shockScript.enabled = false;
        if (_radarScript) _radarScript.enabled = false;
        if (_runawayScript) _runawayScript.enabled = false;
    }

    void Start()
    {
        // 포톤 런너가 완전히 준비될 때까지 안전하게 대기 후 역할 분배 시작
        StartCoroutine(WaitAndInitializeRole());
    }

    private IEnumerator WaitAndInitializeRole()
    {
        // NetworkGameManager나 NetworkManager가 세팅될 때까지 대기
        while (NetworkGameManager.Instance == null || NetworkGameManager.Instance.Runner == null || !NetworkGameManager.Instance.Runner.IsRunning)
        {
            yield return new WaitForSeconds(0.5f);
        }

        NetworkRunner runner = NetworkGameManager.Instance.Runner;

        if (runner.IsServer)
        {
            // [호스트 = 잠입자] 이동 제어 컴포넌트만 활성화
            Debug.Log("<color=cyan>[RoleInit] 로컬 플레이어는 '잠입자(Host)'입니다. RunawayStatus를 켭니다.</color>");
            if (_runawayScript) _runawayScript.enabled = true;
        }
        else if (runner.IsClient)
        {
            // [클라이언트 = 추격자] 공격 스킬 및 레이더 활성화
            Debug.Log("<color=orange>[RoleInit] 로컬 플레이어는 '추격자(Client)'입니다. 레이더 및 스킬을 켭니다.</color>");
            if (_shockScript) _shockScript.enabled = true;
            if (_radarScript) _radarScript.enabled = true;
        }
    }
}