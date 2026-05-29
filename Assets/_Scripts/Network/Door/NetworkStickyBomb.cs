using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkStickyBomb : MonoBehaviour
{
    [Header("점착 설정")]
    public LayerMask stickableLayers; // 일반 벽, 바닥 등 부착 가능한 레이어들
    public LayerMask doorLayers;      // 특별히 문으로 판정할 레이어들

    [Header("EMP 폭발 설정")]
    public float explosionDelay = 3.0f;
    public float empRadius = 3.0f;

    [Header("사운드 설정")]
    public SoundType explosionSoundType;
    public SoundType doorSoundType;

    private Rigidbody _rb;
    private Collider _collider;
    private bool _isStuck = false;

    private SoundType _finalSoundType;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _rb.isKinematic = true;
        _rb.useGravity = false;

        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
        _finalSoundType = explosionSoundType;
    }

    // 손에 쥔 채로 벽이나 문에 닿았을 때 실행되는 함수
    void OnTriggerEnter(Collider other)
    {
        if (_isStuck) return;

        int combinedMask = stickableLayers.value | doorLayers.value;

        if ((combinedMask & (1 << other.gameObject.layer)) != 0)
        {
            PlantBombOnSurface(other);
        }
    }

    private void PlantBombOnSurface(Collider wallCollider)
    {
        _isStuck = true;

        // 부딪힌 오브젝트가 문 레이어 그룹에 속해 있는지 단독 검사
        if ((doorLayers.value & (1 << wallCollider.gameObject.layer)) != 0)
        {
            _finalSoundType = doorSoundType;
            Debug.Log("🚪 [판정] 문에 부착됨 -> 문 전용 사운드로 세팅");
        }
        else
        {
            _finalSoundType = explosionSoundType;
            Debug.Log("🧱 [판정] 일반 벽에 부착됨 -> 일반 폭발 사운드로 세팅");
        }

        // 1. 부모 해제
        transform.SetParent(null);

        // 2. 레이캐스트 정렬
        Vector3 rayStart = transform.position - transform.forward * 0.2f;

        int combinedMask = stickableLayers.value | doorLayers.value;

        if (Physics.Raycast(rayStart, transform.forward, out RaycastHit hit, 1.0f, combinedMask))
        {
            transform.position = hit.point;
            transform.up = hit.normal;
        }
        else
        {
            transform.position = wallCollider.ClosestPoint(transform.position);
        }

        // 3. 트리거 원복
        if (_collider != null)
        {
            _collider.isTrigger = false;
        }

        // 4. 자식으로 귀속
        transform.SetParent(wallCollider.transform);

        // 카운트다운 시작
        StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(explosionDelay);

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.RPC_PlayGlobalSound(transform.position, 3.0f, _finalSoundType);
        }
        else
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.EmitSound(transform.position, 3.0f, _finalSoundType);
            }
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