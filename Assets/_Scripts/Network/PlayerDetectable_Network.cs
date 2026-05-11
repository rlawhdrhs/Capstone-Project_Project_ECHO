using Fusion;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class PlayerDetectable_Network : NetworkBehaviour
{
    private Renderer objectRenderer;
    private Material instanceMaterial;

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
    [Networked] public NetworkBool isDetected { get; set; }

    [Header("Removable Settings")]
    public float removableDuration = 2f;
    private float removableTimer = 0f;

    private bool canEnterRemovable = true;

    private TickTimer detectionTimer;

    [Header("Detect Points")]
    [SerializeField] private Transform detectPointsRoot;

    private Transform[] detectPoints;
    public Transform[] DetectPoints => detectPoints;

    public override void Spawned()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            instanceMaterial = objectRenderer.material;
            UpdateColor(); // 초기 색상 설정
        }

        if (detectPointsRoot != null)
        {
            int childCount = detectPointsRoot.childCount;
            detectPoints = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                detectPoints[i] = detectPointsRoot.GetChild(i);
            }
        }
    }

    void UpdateColor()
    {
        if (instanceMaterial == null || isRemoved) return;
        if (isRemovable) instanceMaterial.color = removableColor;
        else instanceMaterial.color = isDetected ? detectedColor : normalColor;
    }

    public override void Render()
    {
        if (instanceMaterial == null || isRemoved) return;

        if (isRemovable) instanceMaterial.color = removableColor;
        else instanceMaterial.color = isDetected ? detectedColor : normalColor;
    }

    public override void FixedUpdateNetwork()
    {
        if (isRemoved || !Object.HasStateAuthority) return;

        if (detectionTimer.Expired(Runner))
        {
            isDetected = false;
        }

        if (isDetected) detectionGauge += increaseSpeed * Runner.DeltaTime;
        else detectionGauge -= decreaseSpeed * Runner.DeltaTime;

        detectionGauge = Mathf.Clamp(detectionGauge, 0f, maxGauge);

        if (detectionGauge >= maxGauge)
        {
            isRemovable = true;
        }
        else if (detectionGauge <= 0f)
        {
            isRemovable = false; // 게이지가 다 깎이면 제거 가능 상태 해제
        }
    }

    public void NotifyDetected()
    {
        if (Object.HasStateAuthority)
        {
            isDetected = true;
            // 0.1초 동안 레이저가 안 들어오면 감지가 풀린 것으로 판정
            detectionTimer = TickTimer.CreateFromSeconds(Runner, 0.1f);
        }
    }

    public void RequestRemove()
    {
        if (isRemovable && !isRemoved) TryRemoveRpc();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void TryRemoveRpc()
    {
        isRemoved = true;
        Runner.Despawn(Object);
    }
}