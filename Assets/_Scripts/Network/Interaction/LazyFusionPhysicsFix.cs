using UnityEngine;

// 포톤 퓨전 컴포넌트가 전혀 없는 순수 유니티 스크립트입니다.
// 기존 하드웨어 왼손, 오른손 오브젝트에 그냥 추가만 하세요!
public class LocalHandPhysicsFix : MonoBehaviour
{
    private Rigidbody myRb;
    private Vector3 trackedPos;
    private Quaternion trackedRot;

    void Awake()
    {
        myRb = GetComponent<Rigidbody>();
        if (myRb != null)
        {
            myRb.isKinematic = true;
            myRb.useGravity = false;
        }
    }

    void Update()
    {
        // 1. XRIT가 Update에서 하드웨어 트래킹으로 이동시켜놓은 '진짜 손 위치'를 매 프레임 기억합니다.
        trackedPos = transform.position;
        trackedRot = transform.rotation;
    }

    void FixedUpdate()
    {
        // 2. 유니티 표준 FixedUpdate는 퓨전의 물리 시뮬레이션 직전에 안전하게 실행됩니다.
        // 이때 캐싱해둔 최신 트래킹 위치를 강제로 물리 좌표에 주입합니다.
        transform.position = trackedPos;
        transform.rotation = trackedRot;

        // 3. ⭐핵심⭐ 퓨전 물리 엔진이 연산을 시작하기 전에 "손 위치 바꿨으니 콜라이더 영역 당장 갱신해!"라고 유니티에 명령합니다.
        Physics.SyncTransforms();
    }
}