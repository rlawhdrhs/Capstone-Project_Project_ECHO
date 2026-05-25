using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class bombDebug : MonoBehaviour
{
    private XRGrabInteractable _grabInteractable;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();

        if (_grabInteractable != null)
        {
            // XRI 시스템의 상호작용 이벤트를 코드로 직접 리스닝합니다.
            _grabInteractable.hoverEntered.AddListener(OnHoverEntered);
            _grabInteractable.hoverExited.AddListener(OnHoverExited);
            _grabInteractable.selectEntered.AddListener(OnSelectEntered);
            _grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        Debug.Log($"<color=yellow>[XRI 디버그] 🤝 손이 폭탄 범위 안에 들어옴! (감지된 손: {args.interactorObject.transform.name})</color>");
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        Debug.Log($"<color=white>[XRI 디버그] 👋 손이 폭탄 범위에서 나감.</color>");
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log($"<color=lime>[XRI 디버그] 🎉 ★그랩 성공!★ 손이 폭탄을 쥐었습니다!</color>");
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        Debug.Log($"<color=orange>[XRI 디버그] ❌ 그랩 해제 또는 강제 놓침 발생.</color>");
    }
}