using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketDebugger : MonoBehaviour
{
    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();

        // XRI 내부 이벤트에 디버그 로그 바인딩
        if (socket != null)
        {
            socket.hoverEntered.AddListener(OnXRIHoverEnter);
            socket.selectEntered.AddListener(OnXRISelectEnter);
        }
    }

    // [1단계 검증] 유니티 순수 물리 엔진이 충돌을 감지하는가?
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=cyan><b>[1단계 물리 체크]</b> {other.name} 가 소켓 범위에 들어옴! " +
                  $"\n-> 태그: {other.tag} | 레이어: {LayerMask.LayerToName(other.gameObject.layer)}</color>");
    }

    // [2단계 검증] XRI 소켓 시스템이 이 물체를 결합 대상(Hover)으로 인지하는가?
    private void OnXRIHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"<color=yellow><b>[2단계 XRI 호버]</b> 소켓이 {args.interactableObject.transform.name} 물체를 타겟으로 인식함!</color>");
    }

    // [3단계 검증] 최종 결합(Select)이 일어나는가?
    private void OnXRISelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"<color=green><b>[3단계 XRI 셀렉트]</b> {args.interactableObject.transform.name} 결합 최종 성공!</color>");
    }
}