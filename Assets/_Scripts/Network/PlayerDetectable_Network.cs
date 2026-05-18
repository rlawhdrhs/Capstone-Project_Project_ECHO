using Fusion;
using UnityEngine;

public class PlayerDetectable_Network : NetworkBehaviour
{
    private Renderer[] objectRenderers;
    private Material[] instanceMaterials;

    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private Color removableColor = Color.yellow;

    [Header("Detection Gauge")]
    [Networked] public float detectionGauge { get; set; }
    public float maxGauge = 100f;
    public float increaseSpeed = 40f;
    public float decreaseSpeed = 20f;

    [Header("State")]
    [Networked] public NetworkBool isRemovable { get; set; }
    [Networked] public NetworkBool isRemoved { get; set; }
    [Networked, OnChangedRender(nameof(OnDetectedChanged))]
    public NetworkBool isDetected { get; set; }
    [Networked] private NetworkBool canEnterRemovable { get; set; }

    [Header("Removable Settings")]
    public float removableDuration = 0.5f;
    [Networked] private float removableTimer { get; set; }

    private TickTimer detectionTimer;
    private float lastRpcTime; // RPC 트래픽 폭주 방지용

    [Header("Detect Points")]
    [SerializeField] private Transform detectPointsRoot;
    private Transform[] detectPoints;
    public Transform[] DetectPoints => detectPoints;

    [Header("Silhouette")]
    public Material redSilhouetteMaterial;

    // 원래 아바타가 가지고 있던 머티리얼 원본을 기억할 배열
    private Material[][] originalMaterials;

    public override void Spawned()
    {
        SetupDetectPoints();
        SetupRenderers();
        canEnterRemovable = true;

        if (Object.HasInputAuthority)
        {
            if (NetworkGameManager.Instance != null)
            {
                NetworkGameManager.Instance.isLocalPlayerInfiltrator = true;
            }
        }
    }

    private void SetupDetectPoints()
    {
        if (detectPointsRoot == null)
        {
            Transform found = transform.Find("DetectPoints");
            if (found != null) detectPointsRoot = found;
        }

        if (detectPointsRoot != null)
        {
            detectPoints = new Transform[detectPointsRoot.childCount];
            for (int i = 0; i < detectPointsRoot.childCount; i++)
            {
                detectPoints[i] = detectPointsRoot.GetChild(i);
            }
        }
    }

    private void SetupRenderers()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
        if (objectRenderers == null || objectRenderers.Length == 0) return;

        originalMaterials = new Material[objectRenderers.Length][];
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            originalMaterials[i] = objectRenderers[i].materials;
        }
    }

    public override void Render()
    {
        if (objectRenderers == null || isRemoved) return;

        bool isMyAvatar = Object.HasInputAuthority;
        if (Runner.IsServer && Object.HasStateAuthority) isMyAvatar = true;

        if (isMyAvatar)
        {
            // 잠입자 본인 화면
            EnableRenderers(true);
            RestoreOriginalMaterials(); // 항상 본모습 유지
        }
        else
        {
            // 추격자 화면
            if (isDetected)
            {
                EnableRenderers(true);
                ApplyRedSilhouette(); // 붉은 아우라
            }
            else
            {
                EnableRenderers(false); // 평소엔 투명
            }
        }
    }
    private void ApplyRedSilhouette()
    {
        if (redSilhouetteMaterial == null) return;

        for (int i = 0; i < objectRenderers.Length; i++)
        {
            if (objectRenderers[i] != null)
            {
                // 원본 머티리얼 개수와 똑같은 크기의 배열을 만들고, 전부 빨간색으로 채웁니다.
                Material[] redMats = new Material[originalMaterials[i].Length];
                for (int j = 0; j < redMats.Length; j++)
                {
                    redMats[j] = redSilhouetteMaterial;
                }
                objectRenderers[i].materials = redMats;
            }
        }
    }
    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            // 현재 머티리얼이 원본과 다를 때만 덮어씌워서 퍼포먼스 낭비를 줄입니다.
            if (objectRenderers[i] != null && objectRenderers[i].materials[0] != originalMaterials[i][0])
            {
                objectRenderers[i].materials = originalMaterials[i];
            }
        }
    }

    private void EnableRenderers(bool isVisible)
    {
        foreach (var r in objectRenderers)
        {
            if (r != null && r.enabled != isVisible)
                r.enabled = isVisible;
        }
    }

    private void UpdateColors(Color color)
    {
        for (int i = 0; i < instanceMaterials.Length; i++)
        {
            if (instanceMaterials[i] != null)
                instanceMaterials[i].color = color;
        }
    }

    void OnDetectedChanged()
    {
        // 내 아바타일 때만 내 화면(UI)에 경고를 띄움!
        if (Object.HasInputAuthority || (Runner.IsServer && Object.HasStateAuthority))
        {
            if (IntruderDetectedUI.Instance != null)
            {
                IntruderDetectedUI.Instance.ShowWarning(isDetected);
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        // 권한(State Authority)이 없는 클라이언트는 여기서 리턴되므로 연산하지 않음
        if (isRemoved || !Object.HasStateAuthority) return;

        // 0.1초 동안 감지 RPC/신호가 안 오면 감지가 풀린 것으로 판정
        if (isDetected && detectionTimer.Expired(Runner))
        {
            isDetected = false;
        }

        UpdateGaugeLogic();
    }

    private void UpdateGaugeLogic()
    {
        if (isRemovable)
        {
            if (!isDetected)
            {
                removableTimer -= Runner.DeltaTime;
                if (removableTimer <= 0f)
                {
                    isRemovable = false;
                    detectionGauge = maxGauge * 0.7f;
                }
            }
            return;
        }

        if (!isDetected) canEnterRemovable = true;

        if (isDetected) detectionGauge += increaseSpeed * Runner.DeltaTime;
        else detectionGauge -= decreaseSpeed * Runner.DeltaTime;

        detectionGauge = Mathf.Clamp(detectionGauge, 0f, maxGauge);

        if (detectionGauge >= maxGauge && canEnterRemovable)
        {
            isRemovable = true;
            canEnterRemovable = false;
            removableTimer = removableDuration;
        }
    }

    // --- 수정된 부분: 권한 불일치 해결 ---
    public void NotifyDetected()
    {
        if (Object.HasStateAuthority)
        {
            // 자신이 권한을 가지고 있으면 즉시 적용
            isDetected = true;
            detectionTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }
        else
        {
            // 상대방(추격자)이 나를 봤다면, 나(잠입자)에게 RPC를 보내서 감지되었다고 알려줌
            // (초당 60번씩 RPC를 보내면 네트워크가 터지므로 0.05초 쿨타임 적용)
            if (Time.time - lastRpcTime > 0.05f)
            {
                lastRpcTime = Time.time;
                Rpc_NotifyDetected();
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_NotifyDetected()
    {
        // 이 함수는 잠입자의 권한을 가진 쪽(주로 클라이언트)에서만 실행됨
        isDetected = true;
        detectionTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
    }

    public void RequestRemove()
    {
        if (isRemovable && !isRemoved) TryRemoveRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void TryRemoveRpc()
    {
        isRemoved = true;

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.TriggerChaserWin();
        }
        Runner.Despawn(Object);
    }
}