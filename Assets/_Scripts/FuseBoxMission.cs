using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FuseBoxMission : MonoBehaviour
{
    [Header("퓨즈 소켓들")]
    public XRSocketInteractor socketA;
    public XRSocketInteractor socketB;
    public XRSocketInteractor socketC;

    [Header("클리어 시 실행할 이벤트")]
    public UnityEvent onMissionClear;

    private bool isCleared = false;

    // 소켓에 물건이 들어올 때마다 호출될 함수
    public void CheckFuses()
    {
        if (isCleared) return;

        // 3개의 소켓 모두에 물건(퓨즈)이 끼워져 있는지 확인
        if (socketA.hasSelection && socketB.hasSelection && socketC.hasSelection)
        {
            isCleared = true;
            Debug.Log("퓨즈 박스 미션 클리어!");
            onMissionClear.Invoke();
        }
    }

    public void LockFuse(SelectEnterEventArgs args)
    {
        // 1. 들어온 퓨즈의 콜라이더 끄기 (기존 코드)
        Collider fuseCollider = args.interactableObject.transform.GetComponent<Collider>();
        if (fuseCollider != null) fuseCollider.enabled = false;
        Collider socketCollider = args.interactorObject.transform.GetComponent<Collider>();
        if (socketCollider != null)
        {
            socketCollider.enabled = false;
            Debug.Log($"[FuseBox] {gameObject.name}의 소켓 콜라이더가 봉인되었습니다. (간섭 방지)");
        }
    }
}