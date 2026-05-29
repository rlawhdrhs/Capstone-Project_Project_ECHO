using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody))]
public class StickyBomb : MonoBehaviour
{
    [Header("점착 설정")]
    public LayerMask stickableLayers;
    public bool disableGrabAfterStick = true;

    [Header("EMP 폭발 설정")]
    public float explosionDelay = 3.0f;
    public float empRadius = 3.0f;

    private Rigidbody _rb;
    private XRGrabInteractable _grabInteractable;
    private bool _isStuck = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // 1. 본인 또는 자식 오브젝트에서 그랩 컴포넌트 획득
        _grabInteractable = GetComponent<XRGrabInteractable>();
        if (_grabInteractable == null)
        {
            _grabInteractable = GetComponentInChildren<XRGrabInteractable>();
        }

        // 🔥 [핵심] 스폰되자마자 물리를 얼려버립니다.
        // 이렇게 하면 포톤 퓨전 시뮬레이션이 이 폭탄을 건드리지 못하므로 500번 튕기는 현상이 원천 차단됩니다.
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // Kinematic일 때의 권장 모드
    }

    void Start()
    {
        // 로컬 인터랙션 매니저 연결
        if (_grabInteractable != null && NetworkGameManager.Instance != null && NetworkGameManager.Instance.localXRManager != null)
        {
            _grabInteractable.interactionManager = NetworkGameManager.Instance.localXRManager;
        }

        // 그랩 시작/해제 이벤트 리스닝
        if (_grabInteractable != null)
        {
            _grabInteractable.selectExited.AddListener(OnLocalSelectExited);
        }
    }

    private void OnLocalSelectExited(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        // 🔥 [핵심 2] 손에서 폭탄을 놓는(던지는) 순간 물리를 켜줍니다!
        // 이제 일반 리지드바디처럼 중력을 받고 날아가 벽에 부딪힐 수 있게 됩니다.
        if (!_isStuck)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 동적 물리에 맞는 모드로 전환
            Debug.Log("<color=orange>[로컬] 폭탄 투척! 물리 연산 활성화.</color>");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 이미 붙었거나, 손에 쥐고 있는 상태라면 통과
        if (_isStuck) return;
        if (_grabInteractable == null) return;
        if (_grabInteractable.isSelected) return;

        // 던져진 폭탄이 설정한 레이어(벽/문)에 부딪히면 점착 처리
        if ((stickableLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            StickToSurface(collision);
        }
    }

    private void StickToSurface(Collision collision)
    {
        _isStuck = true;

        // 벽에 붙었으므로 다시 물리를 끄고 완전 고정
        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point;
        transform.up = contact.normal;

        transform.SetParent(collision.transform);

        if (disableGrabAfterStick && _grabInteractable != null)
        {
            _grabInteractable.enabled = false;
        }

        Debug.Log("<color=green>[로컬] 폭탄 점착 완료! 3초 후 서버에 폭발을 요청합니다.</color>");
        StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(explosionDelay);

        // 결과만 서버 RPC로 전송 (구현해두신 문 열기 로직 호출)
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestCmdExplosion(transform.position, empRadius);
        }

        Debug.Log("<color=red>[로컬] EMP 방출 및 폭탄 오브젝트 제거</color>");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectExited.RemoveListener(OnLocalSelectExited);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }
}