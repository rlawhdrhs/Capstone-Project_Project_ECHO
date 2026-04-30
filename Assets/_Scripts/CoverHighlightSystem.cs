using UnityEngine;

public class CoverHighlightSystem : MonoBehaviour
{
    [Header("시선 감지 설정")]
    [Tooltip("플레이어의 시선(카메라)")]
    public Camera headCamera;
    [Tooltip("엄폐물 감지 거리")]
    public float gazeDistance = 5.0f;
    [Tooltip("시선 판정 두께 (VR 떨림 방지용)")]
    public float gazeRadius = 0.3f; 
    public LayerMask coverLayer;

    [Header("하이라이트 시각 효과")]
    [Tooltip("바뀔 색상 (투명도 조절 가능)")]
    public Color highlightColor = new Color(0.2f, 0.6f, 1.0f, 1.0f); // 약간 푸르스름한 색
    
    // 유니티 최신 렌더링 파이프라인(URP/HDRP)에서 색상 속성 이름은 보통 "_BaseColor"를 씁니다.
    // 만약 Built-in 파이프라인의 Standard 셰이더를 쓴다면 "_Color"로 변경하세요.
    private readonly string colorPropertyName = "_BaseColor"; 

    private Renderer _currentRenderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        if (headCamera == null) headCamera = Camera.main;
        _propBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        // 1. 카메라 정면으로 두꺼운 레이(SphereCast) 발사
        bool isLookingAtCover = Physics.SphereCast(
            headCamera.transform.position, 
            gazeRadius, 
            headCamera.transform.forward, 
            out RaycastHit hit, 
            gazeDistance, 
            coverLayer
        );

        if (isLookingAtCover)
        {
            Renderer targetRenderer = hit.collider.GetComponent<Renderer>();
            
            // 2. 새로운 엄폐물을 쳐다보기 시작했을 때
            if (targetRenderer != null && targetRenderer != _currentRenderer)
            {
                RemoveHighlight(); // 이전 오브젝트 색상 원상복구
                ApplyHighlight(targetRenderer); // 새 오브젝트 색상 변경
            }
        }
        else
        {
            // 3. 아무것도 쳐다보지 않을 때
            RemoveHighlight();
        }
    }

    private void ApplyHighlight(Renderer renderer)
    {
        _currentRenderer = renderer;
        
        // 기존 렌더러의 속성을 블록에 가져온 뒤, 색상만 덮어씌우고 다시 적용
        _currentRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor(colorPropertyName, highlightColor);
        _currentRenderer.SetPropertyBlock(_propBlock);
    }

    private void RemoveHighlight()
    {
        if (_currentRenderer != null)
        {
            // PropertyBlock을 null로 설정하면 원래 머티리얼의 기본 색상으로 돌아감
            _currentRenderer.SetPropertyBlock(null);
            _currentRenderer = null;
        }
    }
}