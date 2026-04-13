using UnityEngine;

public class InfiltratorHide : MonoBehaviour
{
    public Camera chaserCamera;

    private int defaultLayer;
    private int hiddenLayer;

    private bool isHidden = false;
    private bool isDetectedByChaser = false;


    //
    private Renderer rend;
    private Material mat;

    [Header("색상 설정")]
    public Color normalColor = Color.blue;
    public Color detectedColor = Color.red;
    public Color hiddenColor = new Color(0f, 0f, 1f, 0.3f);

    [Header("깜빡임 설정")]
    public bool useBlinkEffect = true;
    public float blinkSpeed = 4f;



    void Start()
    {

        Debug.Log("InfiltratorHide Start");

        defaultLayer = LayerMask.NameToLayer("Default");
        hiddenLayer = LayerMask.NameToLayer("HiddenPlayer");

        //
        rend = GetComponent<Renderer>();
        mat = rend.material;

        ApplyOpaqueMaterial();
        mat.color = normalColor;
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
            Reveal();
        }

        //
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

        // 화면 안
        if (isInFront && isInsideView)
        {
            Vector3 origin = chaserCamera.transform.position;
            Vector3 direction = (targetPoint - origin).normalized;

            RaycastHit hit;

            //중간에 벽 있는지 검사
            if (Physics.Raycast(origin, direction, out hit, 100f))
            {
                //
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

        gameObject.layer = hiddenLayer;
        isHidden = true;
        Debug.Log("잠입자 숨음");
    }

    void Reveal()
    {
        if (!isHidden) return;

        gameObject.layer = defaultLayer;
        isHidden = false;
        Debug.Log("잠입자 드러남");
    }


    //
    void UpdateVisual()
    {
        if (mat == null) return;

        // 숨은 상태: 반투명 파란색
        if (isHidden)
        {
            ApplyTransparentMaterial();
            mat.color = hiddenColor;
            return;
        }

        // 드러난 상태: 감지 중이면 깜빡이는 빨강
        ApplyOpaqueMaterial();

        if (isDetectedByChaser)
        {
            if (useBlinkEffect)
            {
                float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                mat.color = Color.Lerp(normalColor, detectedColor, t);
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

        mat.SetFloat("_Surface", 0); // Opaque
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

        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}