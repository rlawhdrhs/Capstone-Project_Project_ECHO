using UnityEngine;

public class Infiltrator : MonoBehaviour
{
    private int hiddenLayer;
    private int detectedLayer;

    private Renderer rend;
    private Material mat;

    private bool isHidden = false;
    private bool isDetectedByChaser = false;

    [Header("Color")]
    public Color normalColor = Color.blue;
    public Color detectedColor = Color.red;
    public Color hiddenColor = new Color(0f, 0f, 1f, 0.3f);

    [Header("Blink")]
    public bool useBlinkEffect = true;
    public float blinkInterval = 0.5f; // 0.5초 빨강, 0.5초 파랑

    void Start()
    {
        hiddenLayer = LayerMask.NameToLayer("HiddenPlayer");
        detectedLayer = LayerMask.NameToLayer("DetectedPlayer");

        rend = GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError("Renderer가 없습니다.");
            return;
        }

        mat = rend.material;
        SetNormal();
    }

    void Update()
    {
        bool hideInput = Input.GetKey(KeyCode.DownArrow);

        if (isDetectedByChaser && hideInput)
        {
            Hide();
        }
        else if (isHidden)
        {
            Reveal();
        }

        UpdateVisualAndLayer();
    }

    // Chaser가 호출
    public void SetDetected(bool detected)
    {
        isDetectedByChaser = detected;
    }

    void UpdateVisualAndLayer()
    {
        if (mat == null) return;

        // 숨은 상태
        if (isHidden)
        {
            ApplyTransparentMaterial();
            mat.color = hiddenColor;
            gameObject.layer = hiddenLayer;
            return;
        }

        ApplyOpaqueMaterial();

        // 감지 안 됨
        if (!isDetectedByChaser)
        {
            mat.color = normalColor;
            gameObject.layer = hiddenLayer;
            return;
        }

        // 감지 중
        if (useBlinkEffect)
        {
            float timer = Mathf.Repeat(Time.time, blinkInterval * 2f);

            if (timer < blinkInterval)
            {
                // 빨강 구간
                mat.color = detectedColor;
                gameObject.layer = detectedLayer;
            }
            else
            {
                // 파랑 구간
                mat.color = normalColor;
                gameObject.layer = hiddenLayer;
            }
        }
        else
        {
            mat.color = detectedColor;
            gameObject.layer = detectedLayer;
        }
    }

    public void Hide()
    {
        isHidden = true;
    }

    public void Reveal()
    {
        isHidden = false;
    }

    void SetNormal()
    {
        ApplyOpaqueMaterial();
        mat.color = normalColor;
        gameObject.layer = hiddenLayer;
    }

    void ApplyOpaqueMaterial()
    {
        if (mat == null) return;

        mat.SetFloat("_Surface", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }

    void ApplyTransparentMaterial()
    {
        if (mat == null) return;

        mat.SetFloat("_Surface", 1);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}