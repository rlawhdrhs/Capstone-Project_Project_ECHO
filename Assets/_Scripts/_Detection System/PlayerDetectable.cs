using Fusion;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class PlayerDetectable : NetworkBehaviour
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

    //public void SetDetected(bool detected)
    //{
    //    if (instanceMaterial == null || isRemoved) return;

    //    if (isRemovable)
    //    {
    //        instanceMaterial.color = removableColor;
    //    }
    //    else
    //    {
    //        instanceMaterial.color = detected ? detectedColor : normalColor;
    //    }
    //}

    //public void UpdateGauge(bool detected)
    //{
    //    if (isRemoved) return;

    //    // 제거 가능 상태일 때는 타이머만 동작
    //    if (isRemovable)
    //    {
    //        if(!detected){
    //            removableTimer -= Time.deltaTime;

    //            if (removableTimer <= 0f)
    //            {
    //                ExitRemovableState();
    //            }
    //        }

    //        return;
    //    }

    //    // 레이저에서 완전히 벗어나야 다시 제거 가능 상태 진입 허용
    //    if (!detected)
    //    {
    //        canEnterRemovable = true;
    //    }

    //    // 일반 상태에서 게이지 증가/감소
    //    if (detected)
    //    {
    //        detectionGauge += increaseSpeed * Time.deltaTime;
    //    }
    //    else
    //    {
    //        detectionGauge -= decreaseSpeed * Time.deltaTime;
    //    }

    //    detectionGauge = Mathf.Clamp(detectionGauge, 0f, maxGauge);

    //    // 제거 가능 상태 재진입 조건
    //    if (detectionGauge >= maxGauge && canEnterRemovable)
    //    {
    //        EnterRemovableState();
    //    }
    //}

    //void EnterRemovableState()
    //{
    //    isRemovable = true;
    //    canEnterRemovable = false;
    //    removableTimer = removableDuration;

    //    Debug.Log(gameObject.name + " 제거 가능 상태!");

    //    if (instanceMaterial != null)
    //    {
    //        instanceMaterial.color = removableColor;
    //    }
    //}

    //void ExitRemovableState()
    //{
    //    isRemovable = false;

    //    Debug.Log(gameObject.name + " 제거 실패 → 다시 감소 시작");

    //    // 바로 다시 꽉 차지 않게 약간 깎아줌
    //    detectionGauge = maxGauge * 0.7f;
    //}

    //public void TryRemove()
    //{
    //    Debug.Log("TryRemove 호출됨 | isRemovable: " + isRemovable + " | isRemoved: " + isRemoved);

    //    if (!isRemovable || isRemoved) return;

    //    isRemoved = true;
    //    Debug.Log(gameObject.name + " 제거됨!");
    //    gameObject.SetActive(false);
    //}

    //void OnStateChanged()
    //{
    //    if (instanceMaterial == null) return;

    //    if (isRemovableNet)
    //    {
    //        instanceMaterial.color = removableColor;
    //    }
    //    else
    //    {
    //        instanceMaterial.color = isDetectedNet ? detectedColor : normalColor;
    //    }
    //}
}