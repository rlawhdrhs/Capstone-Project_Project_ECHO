using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRGrabDebug : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null) return;

        // XRIT 이벤트 연결
        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
        grabInteractable.selectEntered.AddListener(OnSelectEnter);
        grabInteractable.selectExited.AddListener(OnSelectExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        Debug.Log($"[XRDebug] 손이 물체에 닿음 (Hover Enter) -> Interactor: {args.interactorObject.transform.name}");
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        Debug.Log($"[XRDebug] 손이 물체에서 떨어짐 (Hover Exit)");
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        Debug.Log($"[XRDebug] ★잡기 성공★ (Select Enter) -> Interactor: {args.interactorObject.transform.name}");
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        Debug.Log($"[XRDebug] 물체를 놓음 (Select Exit)");
    }
}