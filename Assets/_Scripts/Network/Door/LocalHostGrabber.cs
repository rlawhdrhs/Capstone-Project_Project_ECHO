using UnityEngine;

public class LocalHostGrabber : MonoBehaviour
{
    public LayerMask grabbableLayer;
    public float grabRadius = 0.15f;
    public Transform holdPoint; // 물체가 붙을 손 위치

    private GameObject hoveredObject;
    private GameObject grabbedObject;
    private Rigidbody grabbedRigidbody;

    void Update()
    {
        // 1. 이미 잡고 있는 상태라면 놓기 체크만 수행
        if (grabbedObject != null)
        {
            if (GetGrabButtonUp()) // 그랩 버튼을 뗐을 때
            {
                ReleaseObject();
            }
            return;
        }

        // 2. 표준 Update 타이밍에 주변 Grabbable 오브젝트 탐색 (퓨션 물리 루프의 영향을 받지 않음)
        Collider[] hits = Physics.OverlapSphere(transform.position, grabRadius, grabbableLayer);

        if (hits.Length > 0)
        {
            GameObject closest = hits[0].gameObject; // 가장 가까운 물체 선택 로직 추가 가능

            if (hoveredObject != closest)
            {
                UnactivateHover();
                hoveredObject = closest;
                ActivateHover();
            }

            // 3. Hover 상태에서 그랩 버튼을 누르면 즉시 획득
            if (GetGrabInputDown())
            {
                GrabObject(hoveredObject);
            }
        }
        else
        {
            // 주변에 아무것도 없으면 호버 해제
            if (hoveredObject != null)
            {
                UnactivateHover();
            }
        }
    }

    void ActivateHover()
    {
        // 호버 시작 시 시각적 효과 (예: 아웃라인 켜기)
        Debug.Log($"[Local] Hover Started: {hoveredObject.name}");
    }

    void UnactivateHover()
    {
        if (hoveredObject == null) return;
        Debug.Log($"[Local] Hover Ended: {hoveredObject.name}");
        hoveredObject = null;
    }

    void GrabObject(GameObject obj)
    {
        grabbedObject = obj;

        // [추가] 만약 잡으려는 물체가 레버(EscapeLever)라면 뜯어내지 않고 조작 시작
        EscapeLever lever = obj.GetComponent<EscapeLever>();
        if (lever != null)
        {
            lever.StartPull(holdPoint != null ? holdPoint : transform);
            UnactivateHover();
            return;
        }

        // 일반 오브젝트용 기존 로직 (그대로 유지)
        grabbedObject = obj;
        grabbedRigidbody = obj.GetComponent<Rigidbody>();
        if (grabbedRigidbody != null) grabbedRigidbody.isKinematic = true;

        grabbedObject.transform.SetParent(holdPoint != null ? holdPoint : transform);
        grabbedObject.transform.localPosition = Vector3.zero;
        grabbedObject.transform.localRotation = Quaternion.identity;

        UnactivateHover();
    }

    void ReleaseObject()
    {
        if (grabbedObject == null) return;

        // [추가] 레버를 놓고 마치는 경우
        EscapeLever lever = grabbedObject.GetComponent<EscapeLever>();
        if (lever != null)
        {
            lever.EndPull();
            TriggerNetworkEvent(grabbedObject);
            grabbedObject = null;
            return;
        }

        // 일반 오브젝트용 기존 로직 (그대로 유지)
        grabbedObject.transform.SetParent(null);
        if (grabbedRigidbody != null) grabbedRigidbody.isKinematic = false;

        TriggerNetworkEvent(grabbedObject);
        grabbedObject = null;
        grabbedRigidbody = null;
    }

    bool GetGrabInputDown()
    {
        // NetworkManager가 있고 XR 그립을 눌렀거나, PC에서 G키를 눌렀을 때
        if (NetworkManager.Instance != null)
        {
            return NetworkManager.Instance.IsLeftGripDown || Input.GetKeyDown(KeyCode.G);
        }
        return Input.GetKeyDown(KeyCode.G);
    }

    bool GetGrabButtonUp()
    {
        // NetworkManager가 있고 XR 그립을 뗐거나, PC에서 G키를 뗐을 때
        if (NetworkManager.Instance != null)
        {
            return NetworkManager.Instance.IsLeftGripUp || Input.GetKeyUp(KeyCode.G);
        }
        return Input.GetKeyUp(KeyCode.G);
    }

    void TriggerNetworkEvent(GameObject target)
    {
        // 여기서 Fusion Runner를 통해 RPC를 날려 상태 동기화 진행
        Debug.Log($"[Network] {target.name} 상호작용 완료 상태를 서버로 전송합니다.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}