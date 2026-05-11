using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class StickyBomb : MonoBehaviour
{
    [Header("점착 설정")]
    public LayerMask stickableLayers;
    public bool disableGrabAfterStick = true;

    [Header("EMP 폭발 설정")]
    [Tooltip("벽에 붙은 후 터지기까지 걸리는 시간")]
    public float explosionDelay = 3.0f;
    [Tooltip("EMP 해킹이 미치는 반경 (미터)")]
    public float empRadius = 3.0f;

    private Rigidbody _rb;
    private XRGrabInteractable _grabInteractable;
    private bool _isStuck = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_isStuck) return;
        if (_grabInteractable.isSelected) return;

        if ((stickableLayers.value & (1 << collision.gameObject.layer)) != 0)
        {
            StickToSurface(collision);
        }
    }

    private void StickToSurface(Collision collision)
    {
        _isStuck = true;
        _rb.isKinematic = true;
        _rb.linearVelocity = Vector3.zero; 
        _rb.angularVelocity = Vector3.zero;

        ContactPoint contact = collision.contacts[0];
        transform.position = contact.point;
        transform.up = contact.normal; 

        transform.SetParent(collision.transform);

        if (disableGrabAfterStick)
        {
            _grabInteractable.enabled = false;
        }

        Debug.Log("<color=green>폭탄 점착 완료! 3초 후 폭발합니다.</color>");
        
        // 🔥 추가됨: 점착과 동시에 폭발 카운트다운 시작
        StartCoroutine(ExplosionRoutine());
    }

    IEnumerator ExplosionRoutine()
    {
        // 1. 설정된 시간만큼 대기
        yield return new WaitForSeconds(explosionDelay);

        // 2. 주변 물체 스캔 (보이지 않는 구체 레이더)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, empRadius);

        foreach (var hit in hitColliders)
        {
            // 근처에 EMPDoor 스크립트를 가진 오브젝트가 있는지 확인
            EMPDoor door = hit.GetComponent<EMPDoor>();
            if (door != null)
            {
                door.OpenDoor(); // 문 열기 함수 실행
            }
        }

        // 3. 폭탄 제거
        Debug.Log("<color=red>EMP 방출!</color>");
        Destroy(gameObject);
    }

    // 에디터에서 폭발 반경을 시각적으로 확인하기 위함
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }
}