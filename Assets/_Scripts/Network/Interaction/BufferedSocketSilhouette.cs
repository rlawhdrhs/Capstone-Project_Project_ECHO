using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BufferedSocketSilhouette : MonoBehaviour
{
    [Header("Silhouette Setup")]
    [Tooltip("소켓 자식으로 넣어둔 파란색 가짜 퓨즈 오브젝트를 등록하세요.")]
    [SerializeField] private GameObject fakeSilhouetteObject;

    [Tooltip("퓨즈 오브젝트가 사용하는 레이어를 선택하세요.")]
    [SerializeField] private LayerMask fuseLayer;

    [Tooltip("실루엣이 켜질 소켓 주변 감지 반경입니다. (미터 단위)")]
    [SerializeField] private float detectionRadius = 0.25f; // 25cm

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();

        if (fakeSilhouetteObject != null)
        {
            fakeSilhouetteObject.SetActive(false);
        }
    }

    void Update()
    {
        // 1. 만약 소켓에 퓨즈가 완전히 탁! 하고 꽂힌 상태라면 실루엣을 강제로 끄고 연산을 멈춥니다.
        if (socket != null && socket.hasSelection)
        {
            if (fakeSilhouetteObject != null) fakeSilhouetteObject.SetActive(false);
            return;
        }

        // 2. 소켓 위치 중심으로 지정한 반경(detectionRadius) 안에 퓨즈 레이어가 있는지 매 프레임 체크합니다.
        // XRI 이벤트나 트리거 시스템을 거치지 않는 순수 물리 연산입니다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, fuseLayer);

        if (colliders.Length > 0)
        {
            // 반경 안에 퓨즈가 단 하나라도 포착되면 무조건 실루엣 ON!
            if (fakeSilhouetteObject != null) fakeSilhouetteObject.SetActive(true);
        }
        else
        {
            // 반경 안에 아무것도 없으면 OFF
            if (fakeSilhouetteObject != null) fakeSilhouetteObject.SetActive(false);
        }
    }

    // 에디터 씬 뷰에서 감지 반경을 파란색 구체 선으로 시각화해 줍니다.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}