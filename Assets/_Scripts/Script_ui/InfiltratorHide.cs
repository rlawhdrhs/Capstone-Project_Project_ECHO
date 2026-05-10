using UnityEngine;

public class InfiltratorHide : MonoBehaviour
{
    public Camera chaserCamera;

    private int hiddenLayer;
    private int detectedLayer;

    private bool isHidden = false;
    private bool isDetectedByChaser = false;

    private Renderer rend;
    private Material mat;

    [Header("색상 설정")]
    public Color normalColor = Color.blue;
    public Color detectedColor = Color.red;
    public Color hiddenColor = new Color(0f, 0f, 1f, 0.3f);

    [Header("깜빡임 설정")]
    public bool useBlinkEffect = true;
    public float blinkSpeed = 0.2f;


    bool IsCurrentColorDetected()
    {
        if (mat == null) return false;

        float tolerance = 0.05f;

        return Mathf.Abs(mat.color.r - detectedColor.r) < tolerance &&
               Mathf.Abs(mat.color.g - detectedColor.g) < tolerance &&
               Mathf.Abs(mat.color.b - detectedColor.b) < tolerance;
    }

    void Start()
    {
        Debug.Log("InfiltratorHide Start");

        hiddenLayer = LayerMask.NameToLayer("HiddenPlayer");
        detectedLayer = LayerMask.NameToLayer("DetectedPlayer");

        rend = GetComponent<Renderer>();
        mat = rend.material;

        ApplyOpaqueMaterial();
        mat.color = normalColor;

        // 시작할 때는 기본적으로 카메라에 안 보이게
        gameObject.layer = hiddenLayer;
    }

    void Update()
    {
        CheckChaserView();

        bool hideInput = Input.GetKey(KeyCode.DownArrow);

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Debug.Log("아래 방향키 눌림");
        }

        if (isDetectedByChaser && hideInput)
        {
            Hide();
        }
        else
        {
            RevealOrDetect();
        }

        UpdateVisual();
    }

    void CheckChaserView()
    {
        if (chaserCamera == null || rend == null) return;

        Vector3 targetPoint = rend.bounds.center;
        Vector3 viewPos = chaserCamera.WorldToViewportPoint(targetPoint);

        bool isInFront = viewPos.z > 0f;
        bool isInsideView =
            viewPos.x >= 0f && viewPos.x <= 1f &&
            viewPos.y >= 0f && viewPos.y <= 1f;

        bool isVisible = false;

        if (isInFront && isInsideView)
        {
            Vector3 origin = chaserCamera.transform.position;
            Vector3 direction = (targetPoint - origin).normalized;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, 100f))
            {
                if (hit.transform == transform)
                {
                    isVisible = true;
                }
            }
        }

        isDetectedByChaser = isVisible;
    }

    void Hide()
    {
        if (isHidden) return;

        isHidden = true;
        gameObject.layer = hiddenLayer;
        Debug.Log("잠입자 숨음");
    }

    void RevealOrDetect()
    {
        if (isHidden)
        {
            isHidden = false;
            Debug.Log("잠입자 드러남");
        }

        // 감지 중이고, 현재 색이 detectedColor에 충분히 가까울 때만 보이게
        if (isDetectedByChaser && IsCurrentColorDetected())
        {
            gameObject.layer = detectedLayer;
        }
        else
        {
            gameObject.layer = hiddenLayer;
        }
    }

    void UpdateVisual()
    {
        if (mat == null) return;

        if (isHidden)
        {
            ApplyTransparentMaterial();
            mat.color = hiddenColor;
            return;
        }

        ApplyOpaqueMaterial();

        if (isDetectedByChaser)
        {
            if (useBlinkEffect)
            {
                float timer = Mathf.Repeat(Time.time, 1f);

                if (timer < 0.5f)
                    mat.color = detectedColor;   // 1초 빨강
                else
                    mat.color = normalColor;     // 1초 파랑
            }
            else
            {
                mat.color = detectedColor;
            }
        }
        else
        {
            mat.color = normalColor;
        }
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