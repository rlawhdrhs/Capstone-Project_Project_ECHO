using Fusion;
using UnityEngine;

public class NetworkSplitSlidingDoor : NetworkBehaviour
{
    [Header("문짝 오브젝트 연결")]
    [Tooltip("위로 올라갈 윗문짝(Upper)의 Transform을 연결하세요.")]
    public Transform upperPanel;
    [Tooltip("아래로 내려갈 아랫문짝(Lower)의 Transform을 연결하세요.")]
    public Transform lowerPanel;

    [Header("닫힌 상태 기본 Y 좌표 (디폴트 설정)")]
    // 👈 인스펙터에서 기본값을 지정하고 문마다 자유롭게 수정할 수 있도록 노출합니다.
    [Tooltip("윗문짝이 닫혀있을 때의 로컬 Y 좌표 기본값")]
    public float upperClosedY = 2.2f;
    [Tooltip("아랫문짝이 닫혀있을 때의 로컬 Y 좌표 기본값")]
    public float lowerClosedY = 0.5f;

    [Header("문 움직임 설정")]
    [Tooltip("문이 열릴 때 '윗문짝 기준' 이동할 로컬 방향과 거리입니다. 아랫문짝은 이 값의 반대 방향으로 자동 계산됩니다.")]
    public Vector3 openOffset = new Vector3(0f, 1.2f, 0f); // Y축으로 1.2m씩 찢어짐
    public float openSpeed = 3f;

    // 포톤 퓨전 동기화 변수 (서버가 바꾸면 모두에게 동기화)
    [Networked, OnChangedRender(nameof(OnDoorStateChanged))]
    public NetworkBool IsOpen { get; set; }

    // 윗문짝 좌표들
    private Vector3 _upperClosedPos;
    private Vector3 _upperOpenPos;
    private Vector3 _upperTargetPos;

    // 아랫문짝 좌표들
    private Vector3 _lowerClosedPos;
    private Vector3 _lowerOpenPos;
    private Vector3 _lowerTargetPos;

    public override void Spawned()
    {
        if (upperPanel == null || lowerPanel == null)
        {
            Debug.LogError($"{gameObject.name}에 문짝(Panel)들이 제대로 연결되지 않았습니다.");
            return;
        }

        _upperClosedPos = new Vector3(upperPanel.localPosition.x, upperClosedY, upperPanel.localPosition.z);
        _lowerClosedPos = new Vector3(lowerPanel.localPosition.x, lowerClosedY, lowerPanel.localPosition.z);

        // 2. 열린 상태의 좌표 계산
        _upperOpenPos = _upperClosedPos + openOffset;
        _lowerOpenPos = _lowerClosedPos - openOffset;

        // 3. 네트워크 초기 상태에 맞춰 타겟 설정
        UpdateTargetPositions();

        // 텔레포트하듯 초기 위치 강제 고정 (시작하자마자 0.5와 2.2 위치로 딱 달라붙습니다)
        upperPanel.localPosition = _upperTargetPos;
        lowerPanel.localPosition = _lowerTargetPos;
    }

    // [State Authority 전용] 문 열고 닫는 토글 함수
    public void ToggleDoor()
    {
        if (Object.HasStateAuthority)
        {
            IsOpen = !IsOpen;
        }
        else
        {
            Rpc_RequestToggleDoor();
        }
    }

    // 네트워크 변수(IsOpen)가 바뀌면 모든 PC에서 타겟 좌표를 동기화
    private void OnDoorStateChanged()
    {
        UpdateTargetPositions();
    }

    // 현재 상태에 따라 윗문/아랫문의 목적지를 정해주는 함수
    private void UpdateTargetPositions()
    {
        _upperTargetPos = IsOpen ? _upperOpenPos : _upperClosedPos;
        _lowerTargetPos = IsOpen ? _lowerOpenPos : _lowerClosedPos;
    }

    // VR 프레임에 맞춰 부드럽게 두 문짝을 동시에 찢거나 닫아줌
    public override void Render()
    {
        if (upperPanel == null || lowerPanel == null) return;

        // 윗문짝 이동
        upperPanel.localPosition = Vector3.MoveTowards(
            upperPanel.localPosition,
            _upperTargetPos,
            Time.deltaTime * openSpeed
        );

        // 아랫문짝 이동
        lowerPanel.localPosition = Vector3.MoveTowards(
            lowerPanel.localPosition,
            _lowerTargetPos,
            Time.deltaTime * openSpeed
        );
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_RequestToggleDoor()
    {
        IsOpen = !IsOpen;
    }
}