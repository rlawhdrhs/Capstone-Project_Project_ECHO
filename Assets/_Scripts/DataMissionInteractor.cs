using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // [추가] UI 컴포넌트 사용을 위해 필수

public class DataMissionInteractor : MonoBehaviour
{
    [Header("설정")]
    public float targetTime = 3.0f; // 목표 시간
    public float toleranceTime = 0.5f;
    private float _outOfFocusTimer = 0f;

    [Header("UI 설정")]
    public Slider progressSlider; // [추가] 데이터 진행도를 보여줄 UI 슬라이더

    [Header("이벤트")]
    public UnityEvent onMissionClear; // 클리어 시 실행할 이벤트

    [Header("사운드 매니저 연동 설정")]
    public SoundType transmissionSoundType;
    public SoundType clearSoundType;
    private SpatialSoundPlayer _activeLoopPlayer;

    private float _timer = 0f;
    private bool _isTouching = false;
    private bool _isCleared = false;

    // [추가] 치트 매니저가 이 미션이 이미 깨졌는지 확인하기 위한 프로퍼티
    public bool IsCleared => _isCleared;

    void Start()
    {
        // 시작할 때 슬라이더 초기화 및 비활성화 (선택 사항)
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false);
        }
    }

    public void OnTouchStart() { if (!_isCleared) _isTouching = true; }
    public void OnTouchEnd() { _isTouching = false; }

    void Update()
    {
        if (_isTouching)
        {
            _outOfFocusTimer = 0f;

            if (_activeLoopPlayer == null && SoundManager.Instance != null)
            {
                _activeLoopPlayer = SoundManager.Instance.EmitLoopingSound(transform.position, transmissionSoundType);
            }

            _timer += Time.deltaTime;
            Debug.Log($"데이터 전송 중... {_timer:F1}초");

            // [추가] 실시간 UI 게이지 갱신
            UpdateProgressUI();

            if (_timer >= targetTime)
            {
                ClearMission(); // 로직을 public 함수로 이동시켰습니다.
            }
        }
        else if (_timer > 0f)
        {
            StopLoopingSound();
            _outOfFocusTimer += Time.deltaTime;

            if (_outOfFocusTimer >= toleranceTime)
            {
                _timer = 0f;
                _outOfFocusTimer = 0f;
                Debug.Log("<color=red>조준이 완전히 풀려 데이터 전송이 취소되었습니다.</color>");

                // [추가] 취소되었으므로 UI 초기화
                UpdateProgressUI();
            }
        }
    }

    // [추가] 슬라이더 UI 제어 함수
    private void UpdateProgressUI()
    {
        if (progressSlider == null) return;

        if (_timer > 0f && !_isCleared)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = _timer / targetTime; // 0.0 ~ 1.0 비율 반영
        }
        else
        {
            progressSlider.value = 0f;
            if (_isCleared) progressSlider.gameObject.SetActive(false); // 클리어 시 UI 숨김
        }
    }

    // [리팩토링] 치트키나 정상 터치 모두 이 함수를 거쳐 클리어되도록 통합
    public void ClearMission()
    {
        if (_isCleared) return;

        _isCleared = true;
        _isTouching = false;
        _timer = targetTime;
        UpdateProgressUI();

        Debug.Log("<color=yellow>★ [SYSTEM] 미션 클리어! ★</color>");

        StopLoopingSound();

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.RPC_PlayGlobalSound(transform.position, 2.0f, clearSoundType);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(transform.position, 2.0f, clearSoundType);
        }

        onMissionClear?.Invoke();

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.Rpc_AddDataProgress();
        }
    }

    private void StopLoopingSound()
    {
        if (_activeLoopPlayer != null)
        {
            Destroy(_activeLoopPlayer.gameObject);
            _activeLoopPlayer = null;
        }
    }

    private void OnDestroy()
    {
        StopLoopingSound();
    }
}