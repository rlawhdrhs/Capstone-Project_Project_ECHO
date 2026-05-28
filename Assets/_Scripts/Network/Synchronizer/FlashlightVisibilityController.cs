using Fusion;
using UnityEngine;

public class FlashlightVisibilityController : NetworkBehaviour
{
    [Header("손전등 컴포넌트 연결")]
    public Light flashlightLight;       // 실제 빛을 발산하는 Light 컴포넌트
    public GameObject lightConeVFX;     // 빛의 원뿔 형태를 표현하는 메쉬나 파티클 (있다면 연결)

    public override void Spawned()
    {
        // 씬 로딩 후 매니저가 세팅될 시간을 위해 한 프레임 쉬고 실행합니다.
        Invoke(nameof(EvaluateLightVisibility), 0.1f);
    }

    private void EvaluateLightVisibility()
    {
        if (NetworkGameManager.Instance != null)
        {
            if (!NetworkGameManager.Instance.isLocalPlayerInfiltrator)
            {
                // 내 화면에 보이는 이 잠입자 아바타의 손전등 기능을 완전히 꺼버립니다.
                if (flashlightLight != null) flashlightLight.enabled = false;
                if (lightConeVFX != null) lightConeVFX.SetActive(false);

                Debug.Log($"🔦 [보안] 로컬 플레이어가 추격자이므로, {gameObject.name}의 불빛을 차단했습니다.");
            }
        }
        else
        {
            // 안전장치: 혹시 테스트 중 매니저가 없다면 네트워크 권한이 없는(남의) 불빛은 끕니다.
            if (!HasInputAuthority)
            {
                if (flashlightLight != null) flashlightLight.enabled = false;
                if (lightConeVFX != null) lightConeVFX.SetActive(false);
            }
        }
    }
}