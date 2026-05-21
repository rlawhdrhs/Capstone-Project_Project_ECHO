using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMPDoor_Net : MonoBehaviour
{
    // 🌟 1. 맵에 있는 모든 EMP 문들을 자동으로 관리하는 정적 리스트
    public static List<EMPDoor_Net> AllDoors = new List<EMPDoor_Net>();

    [Header("비상 닫기 설정")]
    [Tooltip("문이 닫히는 속도")]
    public float closeSpeed = 5f;

    private Animator _animator;
    private bool _isOpened = false;

    // 🌟 2. 각 문의 고유한 초기 로컬 위치를 기억할 변수
    private Vector3 _initialLocalPosition;
    private Coroutine _closeCoroutine;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError($"{gameObject.name}에 Animator가 없습니다!");
        }

        // 🌟 3. 부모 오브젝트 대비 나의 '원래 닫혀있던 로컬 위치'를 기억합니다.
        // 이 덕분에 문의 월드 좌표나 크기가 달라도 상관 없어집니다.
        _initialLocalPosition = transform.localPosition;
    }

    void OnEnable() => AllDoors.Add(this);
    void OnDisable() => AllDoors.Remove(this);

    public void OpenDoor()
    {
        if (_isOpened) return;
        _isOpened = true;

        if (_animator != null && _animator.enabled)
        {
            _animator.SetTrigger("Open");
        }
        Debug.Log($"<color=blue>{gameObject.name} 문이 개방되었습니다.</color>");
    }

    public void ForceCloseDoor()
    {
        _isOpened = false;

        // 애니메이터가 켜져 있으면 스크립트의 위치 조정을 방해하므로 과감히 꺼버립니다.
        if (_animator != null) _animator.enabled = false;

        if (_closeCoroutine != null) StopCoroutine(_closeCoroutine);
        _closeCoroutine = StartCoroutine(CloseSliderRoutine());
    }

    private IEnumerator CloseSliderRoutine()
    {
        // 현재 위치에서 처음에 기억해둔 원래 로컬 위치까지 부드럽게 이동
        while (Vector3.Distance(transform.localPosition, _initialLocalPosition) > 0.001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                _initialLocalPosition,
                closeSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 칼같이 위치 고정
        transform.localPosition = _initialLocalPosition;
    }
}