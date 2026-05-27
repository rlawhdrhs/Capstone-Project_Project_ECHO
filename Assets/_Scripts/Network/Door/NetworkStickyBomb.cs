using System.Collections;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))]
public class NetworkStickyBomb : MonoBehaviour
{
    [Header("점착 설정")]
    public LayerMask stickableLayers;

    [Header("EMP 폭발 설정")]
    public float explosionDelay = 3.0f;
    public float empRadius = 3.0f;

    [Header("사운드 설정")]
    public SoundType explosionSoundType;

    private Rigidbody _rb;
    private Collider _collider;
    private bool _isStuck = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        // 🔴 [핵심 1] 손에 들려있는 동안 포톤 물리가 절대 간섭하지 못하도록 락을 겁니다.
        _rb.isKinematic = true;
        _rb.useGravity = false;

        // 🔴 [핵심 2] 유니티 물리 규칙상, Kinematic 물체가 정적인 벽에 부딪힐 때는 
        // 일반 충돌(Collision)이 아니라 트리거(Trigger) 모드여야 100% 한 번에 감지합니다.
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
    }

    // 손에 쥔 채로 벽이나 문에 닿았을 때 실행되는 함수
    void OnTriggerEnter(Collider other)
    {
        if (_isStuck) return;

        // 부딪힌 오브젝트의 레이어가 내가 지정한 벽/문 레이어인지 확인
        if ((stickableLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            PlantBombOnSurface(other);
        }
    }

    private void PlantBombOnSurface(Collider wallCollider)
    {
        _isStuck = true;

        // 1. 손(오른손 컨트롤러 부모)과의 연결을 끊고 세계에 독립시킵니다.
        transform.SetParent(null);

        // 2. 벽면에 이쁘게 정렬하기 위해 폭탄 중심에서 살짝 뒤쪽 방향으로 레이캐스트를 쏩니다.
        Vector3 rayStart = transform.position - transform.forward * 0.2f;
        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, 1.0f, stickableLayers))
        {
            // 레이가 부딪힌 정확한 벽 표면 좌표와 각도로 폭탄을 착 붙입니다.
            transform.position = hit.point;
            transform.up = hit.normal;
        }
        else
        {
            // 레이캐스트가 실패할 경우를 대비한 백업 (가장 가까운 벽 표면 좌표 획득)
            transform.position = wallCollider.ClosestPoint(transform.position);
        }

        // 3. 이제 벽에 고정되었으므로 트리거 모드를 끄고 일반 물리 벽으로 환원합니다.
        if (_collider != null)
        {
            _collider.isTrigger = false;
        }

        // 4. 움직이는 문일 수 있으므로 해당 벽/문의 자식으로 완전히 귀속시킵니다.
        transform.SetParent(wallCollider.transform);

        Debug.Log("<color=green>[폭탄 설치 완료] 벽에 직접 C4처럼 폭탄을 부착했습니다! 3초 후 터집니다.</color>");

        // 3초 카운트다운 루틴 시작
        StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(explosionDelay);

        if (SoundManager.Instance != null)
        {
            // 위치, 소리 데이터 유지 시간(초), 사운드 종류 전달
            // 폭발음 오디오 클립의 길이에 맞춰 대략 3.0f 초 정도 라이프타임을 줍니다.
            SoundManager.Instance.EmitSound(transform.position, 3.0f, explosionSoundType);
        }

        // 결과만 서버 RPC로 전송
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestCmdExplosion(transform.position, empRadius);
        }

        Debug.Log("<color=red>[폭탄 폭발] EMP 방출 및 로컬 폭탄 오브젝트 제거 완료</color>");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }
}