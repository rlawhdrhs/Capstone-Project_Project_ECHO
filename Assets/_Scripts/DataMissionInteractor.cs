using UnityEngine;
using UnityEngine.Events;

public class DataMissionInteractor : MonoBehaviour
{
    [Header("설정")]
    public float targetTime = 3.0f; // 목표 시간
    
    [Header("이벤트")]
    public UnityEvent onMissionClear; // 클리어 시 실행할 이벤트

    private float _timer = 0f;
    private bool _isTouching = false;
    private bool _isCleared = false;

    // XRI 이벤트에서 호출할 함수들
    public void OnTouchStart() { if (!_isCleared) _isTouching = true; }
    public void OnTouchEnd() { _isTouching = false; _timer = 0f; }

    void Update()
    {
        if (_isTouching && !_isCleared)
        {
            _timer += Time.deltaTime;
            Debug.Log($"데이터 전송 중... {_timer:F1}초");

            if (_timer >= targetTime)
            {
                _isCleared = true;
                _isTouching = false;
                Debug.Log("<color=yellow>★ [SYSTEM] 미션 클리어! ★</color>");
                onMissionClear?.Invoke();
                //네트워크에 미션 클리어 동기화
                if (NetworkGameManager.Instance != null)
                {
                    NetworkGameManager.Instance.Rpc_AddDataProgress();
                }
            }
        }
    }
}