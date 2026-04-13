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

    private bool canEnterRemovable = true;

    [Header("Detect Points")]
    [SerializeField] private Transform detectPointsRoot;

    private Transform[] detectPoints;
    public Transform[] DetectPoints => detectPoints;

    void Awake()
    {
        if (detectPointsRoot == null)
        {
            Transform found = transform.Find("DetectPoints");
            if (found != null)
                detectPointsRoot = found;
        }

        if (detectPointsRoot != null)
        {
            detectPoints = new Transform[detectPointsRoot.childCount];
            for (int i = 0; i < detectPointsRoot.childCount; i++)
            {
                detectPoints[i] = detectPointsRoot.GetChild(i);
            }
        }
        else
        {
            detectPoints = new Transform[0];
            Debug.LogWarning($"{gameObject.name}: DetectPoints 오브젝트를 찾지 못함");
        }
    }

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
            instanceMaterial.color = removableColor;
        else
            instanceMaterial.color = detected ? detectedColor : normalColor;
    }

    public void UpdateGauge(bool detected)
    {
        if (isRemoved) return;

        if (isRemovable)
        {
            if (!detected)
            {
                removableTimer -= Time.deltaTime;

                if (removableTimer <= 0f)
                    ExitRemovableState();
            }
            return;
        }

        if (!detected)
            canEnterRemovable = true;

        if (detected)
            detectionGauge += increaseSpeed * Time.deltaTime;
        else
            detectionGauge -= decreaseSpeed * Time.deltaTime;

        detectionGauge = Mathf.Clamp(detectionGauge, 0f, maxGauge);

        if (detectionGauge >= maxGauge && canEnterRemovable)
            EnterRemovableState();
    }

    void EnterRemovableState()
    {
        isRemovable = true;
        canEnterRemovable = false;
        removableTimer = removableDuration;

        Debug.Log($"{gameObject.name} 제거 가능 상태!");

        if (instanceMaterial != null)
            instanceMaterial.color = removableColor;
    }

    void ExitRemovableState()
    {
        isRemovable = false;
        Debug.Log($"{gameObject.name} 제거 실패 → 다시 감소 시작");
        detectionGauge = maxGauge * 0.7f;
    }

    public void TryRemove()
    {
        Debug.Log($"TryRemove 호출됨 | isRemovable: {isRemovable} | isRemoved: {isRemoved}");

        if (!isRemovable || isRemoved) return;

        isRemoved = true;
        Debug.Log($"{gameObject.name} 제거됨!");
        gameObject.SetActive(false);
    }
}