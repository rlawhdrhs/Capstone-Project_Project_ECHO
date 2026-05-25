using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabSafetyPos : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    // 위치 기억용 변수
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Vector3 lastSafePosition;
    private Quaternion lastSafeRotation;

    private bool isGrabbed = false;

    [Header("안전장치 발동 조건")]
    [Tooltip("손으로부터 이 거리(미터) 이상 벌어지면 튕겨 나간 것으로 판정합니다.")]
    [SerializeField] private float maxDistanceFromHand = 5f;

    [Tooltip("원래 있던 자리에서 이 거리 이상 벌어지면 처음 위치로 리셋합니다.")]
    [SerializeField] private float maxDistanceFromSpawn = 50f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        // XRIT 잡기/놓기 이벤트 연결
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabEnter);
            grabInteractable.selectExited.AddListener(OnGrabExit);
        }

        // 1. 게임 시작 시점의 '태초 위치' 기억
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        lastSafePosition = spawnPosition;
        lastSafeRotation = spawnRotation;
    }

    private void OnGrabEnter(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // 2. 잡히는 순간의 위치를 '마지막 안전 위치'로 갱신
        lastSafePosition = transform.position;
        lastSafeRotation = transform.rotation;
    }

    private void OnGrabExit(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // 정상적으로 놓았을 때만 안전 위치를 현재 위치로 업데이트
        // 만약 놓는 순간 이미 튕겨 나가서 속도가 미쳐 날뛰고 있다면 업데이트 하지 않음
        if (rb != null && rb.linearVelocity.magnitude < 5f)
        {
            lastSafePosition = transform.position;
            lastSafeRotation = transform.rotation;
        }
    }

    void Update()
    {
        // 상황 A: 손에 잡혀있는 상태인데 물리 버그로 손과 물체 거리가 멀어졌을 때 (벌벌 떨다 날아갈 때)
        if (isGrabbed && grabInteractable != null && grabInteractable.interactorsSelecting.Count > 0)
        {
            Transform handTransform = grabInteractable.interactorsSelecting[0].transform;
            float distanceFromHand = Vector3.Distance(transform.position, handTransform.position);

            if (distanceFromHand > maxDistanceFromHand)
            {
                // 강제로 손에서 놓게 만들고 직전 안전 위치로 텔레포트
                grabInteractable.interactionManager.SelectExit(grabInteractable.interactorsSelecting[0], grabInteractable);
                ResetToPosition(lastSafePosition, lastSafeRotation);
                Debug.LogWarning($"[SafetyNet] {name} 물체가 손에서 튕겨나가 직전 안전 위치로 복구했습니다.");
            }
        }

        // 상황 B: 아예 맵 밖으로 추락하거나 태초 마을에서 너무 멀리 날아갔을 때 (우주 탈출 방지)
        float distanceFromSpawn = Vector3.Distance(transform.position, spawnPosition);
        if (transform.position.y < -20f || distanceFromSpawn > maxDistanceFromSpawn)
        {
            ResetToPosition(spawnPosition, spawnRotation);
            Debug.LogWarning($"[SafetyNet] {name} 물체가 맵을 탈출하여 처음 스폰 위치로 강제 소환되었습니다.");
        }
    }

    private void ResetToPosition(Vector3 targetPos, Quaternion targetRot)
    {
        // 팅겨나가는 가속도가 남아있으면 복구되어도 또 날아가므로, 물리 속도를 완전 제로(0)로 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;
    }

    // 의도치 않게 파괴될 때 이벤트 해제
    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabEnter);
            grabInteractable.selectExited.RemoveListener(OnGrabExit);
        }
    }
}