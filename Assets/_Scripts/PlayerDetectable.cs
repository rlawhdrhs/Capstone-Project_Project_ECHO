using UnityEngine;

public class PlayerDetectable : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material instanceMaterial;

    [SerializeField] private Color normalColor = Color.cyan;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private Color removableColor = Color.yellow;

    [Header("Detection Gauge")]
    public float detectionGauge = 0f;
    public float maxGauge = 100f;
    public float increaseSpeed = 40f;
    public float decreaseSpeed = 20f;

    [Header("State")]
    public bool isRemovable = false;
    public bool isRemoved = false;

    [Header("Removable Settings")]
    public float removableDuration = 0.5f;
    private float removableTimer = 0f;

    // 제거 가능 상태 재진입 방지용
    private bool canEnterRemovable = true;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            instanceMaterial = objectRenderer.material;
            instanceMaterial.color = normalColor;
        }
    }

    public void SetDetected(bool detected)
    {
        if (instanceMaterial == null || isRemoved) return;

        if (isRemovable)
        {
            instanceMaterial.color = removableColor;
        }
        else
        {
            instanceMaterial.color = detected ? detectedColor : normalColor;
        }
    }

    public void UpdateGauge(bool detected)
    {
        if (isRemoved) return;

        // 제거 가능 상태일 때는 타이머만 동작
        if (isRemovable)
        {
            if(!detected){
                removableTimer -= Time.deltaTime;

                if (removableTimer <= 0f)
                {
                    ExitRemovableState();
                }
            }

            return;
        }

        // 레이저에서 완전히 벗어나야 다시 제거 가능 상태 진입 허용
        if (!detected)
        {
            canEnterRemovable = true;
        }

        // 일반 상태에서 게이지 증가/감소
        if (detected)
        {
            detectionGauge += increaseSpeed * Time.deltaTime;
        }
        else
        {
            detectionGauge -= decreaseSpeed * Time.deltaTime;
        }

        detectionGauge = Mathf.Clamp(detectionGauge, 0f, maxGauge);

        // 제거 가능 상태 재진입 조건
        if (detectionGauge >= maxGauge && canEnterRemovable)
        {
            EnterRemovableState();
        }
    }

    void EnterRemovableState()
    {
        isRemovable = true;
        canEnterRemovable = false;
        removableTimer = removableDuration;

        Debug.Log(gameObject.name + " 제거 가능 상태!");

        if (instanceMaterial != null)
        {
            instanceMaterial.color = removableColor;
        }
    }

    void ExitRemovableState()
    {
        isRemovable = false;

        Debug.Log(gameObject.name + " 제거 실패 → 다시 감소 시작");

        // 바로 다시 꽉 차지 않게 약간 깎아줌
        detectionGauge = maxGauge * 0.7f;
    }

    public void TryRemove()
    {
        Debug.Log("TryRemove 호출됨 | isRemovable: " + isRemovable + " | isRemoved: " + isRemoved);

        if (!isRemovable || isRemoved) return;

        isRemoved = true;
        Debug.Log(gameObject.name + " 제거됨!");
        gameObject.SetActive(false);
    }
}