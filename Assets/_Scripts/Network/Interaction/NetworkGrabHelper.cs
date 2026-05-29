using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NetworkGrabHelper : NetworkBehaviour
{
    private XRGrabInteractable _grabInteractable;
    private bool _lastStateAuthority;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _lastStateAuthority = HasStateAuthority;
    }

    public override void Spawned()
    {
        Debug.Log($"<color=cyan>[NetworkGrab Debug] {gameObject.name} 스폰 완료. 현재 소유권(StateAuthority): {HasStateAuthority}</color>");

        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(OnGrabEntered);
            _grabInteractable.selectExited.AddListener(OnGrabExited);
        }
        else
        {
            Debug.LogError($"<color=red>[NetworkGrab Debug] {gameObject.name}에 XRGrabInteractable 컴포넌트가 없습니다!</color>");
        }
    }

    private void Update()
    {
        // 매 프레임 소유권 상태 변화를 감시해서 로그를 찍습니다.
        if (_lastStateAuthority != HasStateAuthority)
        {
            Debug.Log($"<color=lime>[NetworkGrab Debug] {gameObject.name}의 소유권 상태 변경됨! 이전: {_lastStateAuthority} -> 현재: {HasStateAuthority}</color>");
            _lastStateAuthority = HasStateAuthority;
        }
    }

    private void OnGrabEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"<color=yellow>[NetworkGrab Debug] ★ XRIT 그랩 이벤트 발생! 잡은 사람(Interactor): {args.interactorObject.transform.name}</color>");
        Debug.Log($"[NetworkGrab Debug] 그랩 직전 소유권 상태 -> 나에게 소유권이 있는가?: {HasStateAuthority}");

        if (Object != null && !HasStateAuthority)
        {
            Debug.Log($"<color=orange>[NetworkGrab Debug] 서버에 소유권(State Authority) 오버라이드를 요청합니다...</color>");
            Object.RequestStateAuthority();
        }
    }

    private void OnGrabExited(SelectExitEventArgs args)
    {
        Debug.Log($"<color=magenta>[NetworkGrab Debug] ❌ XRIT 그랩 해제(놓침) 이벤트 발생! 놓친 이유: {args.isCanceled}</color>");
    }

    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabEntered);
            _grabInteractable.selectExited.RemoveListener(OnGrabExited);
        }
    }
}