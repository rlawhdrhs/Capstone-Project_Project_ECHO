using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // XRI 3.x 네임스페이스

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class StickyBomb : MonoBehaviour
{
    [Header("점착 설정")]
    [Tooltip("폭탄이 붙을 수 있는 레이어를 선택하세요")]
    public LayerMask stickableLayers;
    
    [Tooltip("한 번 붙으면 다시 잡을 수 없게 할 것인지?")]
    public bool disableGrabAfterStick = true;

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
        // 1. 이미 어딘가에 붙어있다면 무시
        if (_isStuck) return;

        // 2. 플레이어가 현재 손에 쥐고 있는 상태라면 무시 (던진 후만 작동)
        if (_grabInteractable.isSelected) return;

        // 3. 충돌한 오브젝트가 '붙을 수 있는 레이어'인지 확인
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

        Debug.Log("<color=green>폭탄 점착 완료!</color>");
    }
}