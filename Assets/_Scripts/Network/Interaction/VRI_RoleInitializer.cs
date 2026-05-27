using UnityEngine;
using Fusion;
using System.Collections;

public class VRI_RoleInitializer : MonoBehaviour
{
    private ChaserElectricShock _shockScript;
    private ChaserDualRadar _radarScript;
    private RunawayStatus _runawayScript;
    private StealthDetector _stealthScript;

    void Awake()
    {
        // 내 XR Origin에 붙어있는 스크립트들 참조 선점 확보
        _shockScript = GetComponent<ChaserElectricShock>();
        _radarScript = GetComponent<ChaserDualRadar>();
        _runawayScript = GetComponent<RunawayStatus>();
        _stealthScript = GetComponent<StealthDetector>();

        // 네트워크가 켜지기 전까지는 에러 방지를 위해 일단 전부 꺼둡니다.
        if (_shockScript) _shockScript.enabled = false;
        if (_radarScript) _radarScript.enabled = false;
        if (_runawayScript) _runawayScript.enabled = false;
        // ★ 추가: 시작할 때 스텔스 디텍터도 기본적으로 꺼둡니다.
        if (_stealthScript) _stealthScript.enabled = false;
    }

    void Start()
    {
        // 포톤 런너가 완전히 준비될 때까지 안전하게 대기 후 역할 분배 시작
        StartCoroutine(WaitAndInitializeRole());
    }

    private IEnumerator WaitAndInitializeRole()
    {
        // NetworkManager가 세팅될 때까지 대기
        while (NetworkManager.Instance == null || NetworkManager.Instance.Runner == null || !NetworkManager.Instance.Runner.IsRunning)
        {
            yield return new WaitForSeconds(0.5f);
        }

        NetworkRunner runner = NetworkManager.Instance.Runner;

        if (runner.IsServer)
        {
            // [호스트 = 잠입자] 이동 제어 및 스텔스 모드 활성화
            Debug.Log("<color=cyan>[RoleInit] 로컬 플레이어는 '잠입자(Host)'입니다. RunawayStatus와 스텔스를 켭니다.</color>");
            if (_runawayScript) _runawayScript.enabled = true;
            if (_stealthScript) _stealthScript.enabled = true;
        }
        else if (runner.IsClient)
        {
            // [클라이언트 = 추격자] 공격 스킬 및 레이더만 활성화 (스텔스는 꺼진 상태 유지)
            Debug.Log("<color=orange>[RoleInit] 로컬 플레이어는 '추격자(Client)'입니다. 레이더 및 스킬을 켭니다.</color>");
            if (_shockScript) _shockScript.enabled = true;
            if (_radarScript) _radarScript.enabled = true;
            if (_stealthScript) _stealthScript.enabled = false;
        }
    }
}