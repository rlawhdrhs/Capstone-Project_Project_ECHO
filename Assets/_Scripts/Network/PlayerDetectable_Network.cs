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

    public override void Spawned()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            instanceMaterial = objectRenderer.material;
            UpdateColor(); // 초기 색상 설정
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

        if (isDetected)
        {
            detectionGauge += increaseSpeed * Runner.DeltaTime;
        }
        else
        {
            detectionGauge -= decreaseSpeed * Runner.DeltaTime;
        }

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