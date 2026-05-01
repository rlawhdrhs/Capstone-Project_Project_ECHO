using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public enum CoverState
{
    None,
    ApproachingCover,
    InCoverAssist
}

[RequireComponent(typeof(XROrigin))]
public class CoverAssistSystem : MonoBehaviour
{
    [Header("핵심 변수 (기획서 명세)")]
    public float coverDetectRadius = 1.0f;     // Cover layer 찾는 반경
    public float desiredCoverOffset = 0.3f;    // 엄폐물 표면에서 떨어져 있는 거리
    public float assistSpeed = 0.2f;           // 이동 보조 속도 (SmoothDamp의 소요 시간)
    public float maxAssistDistance = 1.5f;     // 너무 멀리 있는 플레이어를 억지로 끌어오는 것 방지

    [Header("엄폐물 판정 조건")]
    public LayerMask coverLayer;
    public float minCoverHeight = 1.0f;        // 가려질 만큼 충분히 큰가?
    public float rayWidthOffset = 0.3f;        // 좌우 레이를 쏠 때의 간격

    [Header("입력 및 해제 설정")]
    [Tooltip("플레이어의 이동 방향을 파악하기 위한 조이스틱 입력 (XRI Input Action)")]
    public InputActionProperty moveInput;
    [Tooltip("엄폐 해제를 위한 강한 반대 방향 입력 기준값")]
    public float exitInputThreshold = 0.6f;

    private XROrigin _xrOrigin;
    private CoverState _currentState = CoverState.None;
    private Collider _currentTargetCover;

    // 보정 처리를 위한 내부 변수
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;
    private Vector3 _velocity = Vector3.zero;

    void Awake()
    {
        _xrOrigin = GetComponent<XROrigin>();
    }

    void Update()
    {
        // 기획서 기반 FSM (유한 상태 머신)
        switch (_currentState)
        {
            case CoverState.None:
                SearchForCover();
                break;
            case CoverState.ApproachingCover:
                ProcessApproachingCover();
                break;
            case CoverState.InCoverAssist:
                ProcessInCoverAssist();
                break;
        }
    }

    private void SearchForCover()
    {
        Vector3 headPos = _xrOrigin.Camera.transform.position;

        // 1. 플레이어 위치를 중심으로 Cover layer 체크
        Collider[] hits = Physics.OverlapSphere(headPos, coverDetectRadius, coverLayer);
        
        Collider closestCover = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            // 2. 판정: 몸이 가려질 만큼 큰가? 물체가 고정된(Static) 상태인가?
            if (hit.bounds.size.y < minCoverHeight) continue;
            if (!hit.gameObject.isStatic) continue;

            float dist = Vector3.Distance(headPos, hit.ClosestPoint(headPos));
            if (dist < minDistance && dist <= maxAssistDistance)
            {
                minDistance = dist;
                closestCover = hit;
            }
        }

        // 가장 가까운 오브젝트를 CurrentTargetCover로 설정
        if (closestCover != null)
        {
            _currentTargetCover = closestCover;
            _currentState = CoverState.ApproachingCover;
            Debug.Log("👀 [엄폐] 주변 엄폐물 발견! 밀착 시도 중...");
        }
    }

    private void ProcessApproachingCover()
    {
        if (_currentTargetCover == null)
        {
            _currentState = CoverState.None;
            return;
        }

        // 플레이어 이동 방향 추출
        Vector2 inputDir = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
        if (inputDir.sqrMagnitude < 0.05f) return; // 이동하지 않으면 대기

        Vector3 moveDir = _xrOrigin.Camera.transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y));
        moveDir.y = 0; 
        moveDir.Normalize();

        Vector3 headPos = _xrOrigin.Camera.transform.position;
        Vector3 rightOffset = Vector3.Cross(Vector3.up, moveDir).normalized * rayWidthOffset;

        // 3. 이동 방향으로 중앙, 좌측, 우측 3갈래 레이캐스트 발사
        bool hitCenter = Physics.Raycast(headPos, moveDir, out RaycastHit centerHit, coverDetectRadius, coverLayer);
        bool hitLeft   = Physics.Raycast(headPos - rightOffset, moveDir, out RaycastHit leftHit, coverDetectRadius, coverLayer);
        bool hitRight  = Physics.Raycast(headPos + rightOffset, moveDir, out RaycastHit rightHit, coverDetectRadius, coverLayer);

        if (hitCenter && centerHit.collider == _currentTargetCover)
        {
            Vector3 finalNormal = centerHit.normal;

            // 4. 좌우 측면 레이를 통한 코너 판별
            if (hitLeft && hitRight)
            {
                // 좌우 레이의 법선 값이 다를 경우 코너로 판별 (내적으로 각도 차이 확인)
                if (Vector3.Dot(leftHit.normal, rightHit.normal) < 0.95f) 
                {
                    // 코너에 자연스럽게 밀착하도록 두 법선의 중간값(평균) 방향 도출
                    finalNormal = (leftHit.normal + rightHit.normal).normalized;
                }
            }

            // 5. 보정 방향 설정 (표면 법선의 반대 방향으로 약간 붙도록)
            Vector3 headOffset = headPos - _xrOrigin.transform.position;
            headOffset.y = 0;

            Vector3 targetHeadPos = centerHit.point + (finalNormal * desiredCoverOffset);
            _targetPosition = targetHeadPos - headOffset;
            _targetPosition.y = _xrOrigin.transform.position.y; // Y축은 플레이어 바닥 유지

            // 캐릭터 회전 목표 (벽에 등이나 몸을 대기 편하게)
            _targetRotation = Quaternion.LookRotation(-finalNormal, Vector3.up);

            // 상태 전이
            _currentState = CoverState.InCoverAssist;
            Debug.Log("<color=cyan>🛡️ [엄폐 성공] 벽에 완벽하게 밀착되었습니다!</color>");
        }
    }

    private void ProcessInCoverAssist()
    {
        // 보정 방식: Vector3.SmoothDamp를 사용하여 플레이어를 목표 지점으로 부드럽게 끌어당김
        _xrOrigin.transform.position = Vector3.SmoothDamp(
            _xrOrigin.transform.position,
            _targetPosition,
            ref _velocity,
            assistSpeed
        );

        _xrOrigin.transform.rotation = Quaternion.Slerp(
            _xrOrigin.transform.rotation,
            _targetRotation,
            Time.deltaTime * (1.0f / assistSpeed)
        );

        CheckExitConditions();
    }

    private void CheckExitConditions()
    {
        // 1. 엄폐물 이탈 (너무 멀어진 경우)
        if (Vector3.Distance(_xrOrigin.transform.position, _targetPosition) > maxAssistDistance)
        {
            ResetCover();
            return;
        }

        Vector2 inputDir = moveInput.action?.ReadValue<Vector2>() ?? Vector2.zero;
        
        // 2. 반대 방향으로 강하게 이동 입력
        if (inputDir.magnitude > exitInputThreshold)
        {
            Vector3 moveDir = _xrOrigin.Camera.transform.TransformDirection(new Vector3(inputDir.x, 0, inputDir.y));
            moveDir.y = 0; moveDir.Normalize();

            // 현재 엄폐면(정면)의 반대 방향으로 빠져나가려는지 내적으로 판별
            if (Vector3.Dot(moveDir, _targetRotation * Vector3.forward) < -0.3f)
            {
                ResetCover();
            }
        }
        
        // ※ 빠른 이동/대시, 점프 등의 특수 행동 해제 조건은 
        // 해당 기능을 처리하는 스크립트에서 이 스크립트의 ResetCover()를 호출하도록 설계하는 것이 결합도를 낮추는 데 유리해!
    }

    public void ResetCover()
    {
        if (_currentState != CoverState.None)
        {
            Debug.Log("<color=red>⚠️ [엄폐 해제] 엄폐물에서 벗어났습니다.</color>");
        }
        _currentState = CoverState.None;
        _currentTargetCover = null;
    }
}