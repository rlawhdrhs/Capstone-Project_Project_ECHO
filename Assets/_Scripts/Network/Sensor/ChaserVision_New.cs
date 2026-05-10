using Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 1. MonoBehaviour가 아닌 NetworkBehaviour를 상속받습니다.
public class ChaserVision_New : NetworkBehaviour
{
    [Header("시야 설정")]
    public float viewDistance = 50f;
    // 카메라는 이제 에디터에서 넣지 않고 코드가 알아서 찾습니다.
    private Camera localCamera;

    [Header("Volume 설정")]
    public Volume chaserGlobalVolume; // 추격자 화면을 바꿀 볼륨

    [Header("색상 설정")]
    public Color normalColor = new Color(0.3f, 0.3f, 1f, 1f);   // 평소 파랑
    public Color alertColor = new Color(1f, 0.15f, 0.15f, 1f); // 감지 시 빨강
    public float colorChangeSpeed = 6f;

    private ColorAdjustments colorAdjustments;
    private Color currentColor;
    private StealthController targetIntruder;

    // 2. Start() 대신 Fusion의 네트워크 스폰 함수인 Spawned()를 사용합니다.
    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            this.enabled = false;
            return;
        }

        // 3. 로컬 카메라를 자동으로 찾습니다.
        localCamera = LocalVRRig.Instance.avatarHead.GetComponent<Camera>();
        if (localCamera == null)
        {
            Debug.LogError("[ChaserVision] 현재 씬에서 MainCamera 태그가 달린 카메라를 찾을 수 없습니다!");
        }

        // Volume 설정
        if (chaserGlobalVolume != null && chaserGlobalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;
            currentColor = normalColor;
            colorAdjustments.colorFilter.value = currentColor;
        }
        else
        {
            Debug.LogWarning("[ChaserVision] Volume 설정이 잘못되었습니다.");
        }
    }

    void Update()
    {
        // 스크립트가 꺼져있으면 Update도 돌지 않으므로 안전합니다.

        bool hasDetectedPlayer = DetectIntruder();
        UpdateVolumeColor(hasDetectedPlayer);
    }

    bool DetectIntruder()
    {
        // 잠입자가 아직 스폰되지 않았다면 Instance가 null일 수 있으므로 계속 확인합니다.
        if (targetIntruder == null)
        {
            // 💡 무거운 FindObjectOfType 대신, static 변수를 0.0001초 만에 바로 가져옵니다!
            targetIntruder = StealthController.Instance;

            if (targetIntruder == null) return false; // 아직도 스폰 안 됐으면 감지 불가
        }

        bool detected = IsVisible(targetIntruder);
        targetIntruder.RPC_SetDetected(detected);
        return detected;
    }

    void UpdateVolumeColor(bool hasDetectedPlayer)
    {
        if (colorAdjustments == null) return;

        Color targetColor = hasDetectedPlayer ? alertColor : normalColor;
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * colorChangeSpeed);
        colorAdjustments.colorFilter.value = currentColor;
    }

    //bool IsVisible(StealthController target)
    //{
    //    if (localCamera == null) return false;

    //    Renderer rend = target.rend;
    //    if (rend == null) return false;

    //    Vector3 point = rend.bounds.center;
    //    Vector3 viewPos = localCamera.WorldToViewportPoint(point);

    //    bool inFront = viewPos.z > 0f;
    //    bool inView =
    //        viewPos.x >= 0f && viewPos.x <= 1f &&
    //        viewPos.y >= 0f && viewPos.y <= 1f;

    //    if (!inFront || !inView) return false;

    //    Vector3 origin = localCamera.transform.position;
    //    Vector3 dir = (point - origin).normalized;

    //    if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance))
    //    {
    //        return hit.transform == target.transform || hit.transform.IsChildOf(target.transform);
    //    }

    //    return false;
    //}
    bool IsVisible(StealthController target)
    {
        if (localCamera == null) return false;

        Renderer rend = target.rend;
        if (rend == null) return false;

        Vector3 point = rend.bounds.center;
        Vector3 viewPos = localCamera.WorldToViewportPoint(point);

        bool inFront = viewPos.z > 0f;
        bool inView =
            viewPos.x >= 0f && viewPos.x <= 1f &&
            viewPos.y >= 0f && viewPos.y <= 1f;

        if (!inFront || !inView) return false;

        Vector3 origin = localCamera.transform.position;
        Vector3 dir = (point - origin).normalized;

        // 💡 1. 씬(Scene) 뷰에서 내 눈에서 타겟으로 날아가는 초록색 레이저를 직접 눈으로 볼 수 있습니다!
        Debug.DrawRay(origin, dir * viewDistance, Color.green);

        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == target.transform || hit.transform.IsChildOf(target.transform))
            {
                // 💡 2. 감지 성공 시 로그를 띄우고 레이저를 빨간색으로 바꿉니다.
                Debug.Log("<color=red>[감지 성공]</color> 화면이 빨갛게 변해야 합니다!");
                Debug.DrawRay(origin, dir * hit.distance, Color.red);
                return true;
            }
            else
            {
                // 💡 3. 도대체 뭐가 레이저를 가로막았는지 이름을 콘솔창에 고발합니다!
                Debug.Log($"<color=orange>[가려짐]</color> 타겟을 향해 쐈는데, 중간에 엉뚱한 [ {hit.transform.name} ] 에 맞았습니다!");
                return false;
            }
        }

        // 💡 4. 레이저가 허공을 갈랐을 때
        Debug.Log("<color=yellow>[허공]</color> 레이저가 아무것도 맞추지 못했습니다. (타겟에 콜라이더가 없거나 거리가 멉니다)");
        return false;
    }
}