using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DataMissionInteractor : MonoBehaviour
{
    [Header("설정")]
    public float targetTime = 3.0f; // 목표 시간
    public float toleranceTime = 0.5f;
    private float _outOfFocusTimer = 0f;

    [Header("이벤트")]
    public UnityEvent onMissionClear; // 클리어 시 실행할 이벤트

    private float _timer = 0f;
    private bool _isTouching = false;
    private bool _isCleared = false;

    // XRI 이벤트에서 호출할 함수들
    public void OnTouchStart() { if (!_isCleared) _isTouching = true; }
    public void OnTouchEnd()   {_isTouching = false;}

    void Update()
    {
        if (_isTouching)
        {
            // 다시 조준했으므로 빗나간 타이머는 리셋
            _outOfFocusTimer = 0f;

            _timer += Time.deltaTime;
            Debug.Log($"데이터 전송 중... {_timer:F1}초");

            if (_timer >= targetTime)
            {
                _isCleared = true;
                _isTouching = false;
                Debug.Log("<color=yellow>★ [SYSTEM] 미션 클리어! ★</color>");
                onMissionClear?.Invoke();

                if (NetworkGameManager.Instance != null)
                {
                    NetworkGameManager.Instance.Rpc_AddDataProgress();
                }
            }
        }
        else if (_timer > 0f) // 조준을 안 하고 있는데 게이지가 남아있다면?
        {
            // 빗나간 시간을 잽니다.
            _outOfFocusTimer += Time.deltaTime;

            // 만약 빗나간 시간이 유예 시간(0.5초)을 넘어가면 그제서야 초기화!
            if (_outOfFocusTimer >= toleranceTime)
            {
                _timer = 0f;
                _outOfFocusTimer = 0f;
                Debug.Log("<color=red>조준이 완전히 풀려 데이터 전송이 취소되었습니다.</color>");
            }
        }
    }
}