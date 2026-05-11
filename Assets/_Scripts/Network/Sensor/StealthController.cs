using Fusion;
using UnityEngine;

// 1. NetworkBehaviour 상속
public class StealthController : NetworkBehaviour
{
    // 💡 ChaserVision이 바로 찾을 수 있도록 Static 변수 선언
    public static StealthController Instance;

    private int hiddenLayer;
    private int detectedLayer;
    public Renderer rend;
    private Material mat;

    // 2. [Networked] 속성을 달아주면 한쪽에서 값이 변할 때 양쪽 컴퓨터 모두 똑같이 변합니다!
    [Networked] public NetworkBool isHidden { get; set; }
    [Networked] public NetworkBool isDetectedByChaser { get; set; }

    [Header("Color")]
    public Color normalColor = Color.blue;
    public Color detectedColor = Color.red;
    public Color hiddenColor = new Color(0f, 0f, 1f, 0.3f);

    [Header("Blink")]
    public bool useBlinkEffect = true;
    public float blinkInterval = 0.5f;

    // Start 대신 Spawned 사용
    public override void Spawned()
    {
        // 씬에 스폰되면 누구나 이 잠입자를 찾을 수 있게 등록
        Instance = this;

        hiddenLayer = LayerMask.NameToLayer("Hidden");
        detectedLayer = LayerMask.NameToLayer("Detected");

        //rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Renderer가 없습니다.");
            return;
        }

        mat = rend.material;
        SetNormal();
    }

    // 3. Update 대신 Fusion의 네트워크 루프인 FixedUpdateNetwork 사용
    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            // 주의: 나중에 VR 컨트롤러 입력으로 꼭 바꾸셔야 합니다!
            bool hideInput = Input.GetKey(KeyCode.DownArrow);

            if (isDetectedByChaser && hideInput)
            {
                isHidden = true; // [Networked] 변수이므로 값이 변경되면 모두에게 전송됨
            }
            else
            {
                isHidden = false;
            }
        }
    }

    // 4. 시각적 변화는 매 프레임 자연스럽게 부르도록 Render() 사용
    public override void Render()
    {
        UpdateVisualAndLayer();
    }

    // 5. 🚨 추격자(Chaser)가 잠입자를 발견했을 때 부르는 네트워크 함수(RPC)
    // 추격자가 이 함수를 쏘면, 잠입자 본인의 컴퓨터에서 isDetectedByChaser 값을 바꿉니다.
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetDetected(bool detected)
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
                mat.color = detectedColor;
                gameObject.layer = detectedLayer;
            }
            else
            {
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