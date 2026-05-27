using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Image 컴포넌트 제어용 추가

public class IntruderStatusUIManager : MonoBehaviour
{
    public static IntruderStatusUIManager Instance;

    private bool _isStunned = false;
    private bool _isDetected = false;
    private bool _isStealth = false;

    private enum UIState { None, Stealth, Detected, Stunned }
    private UIState _currentDisplayState = UIState.None;

    [Header("[1순위] 스턴 (뇌진탕) 설정")]
    public GameObject stunUIObj;        // 화면을 덮을 스턴 캔버스 오버레이
    public Image stunVignetteImage;     // 스턴 UI 내부의 Image 컴포넌트 (울렁거림 연출용)
    public AudioSource tinnitusAudioSource; // "삐--" 소리를 재생할 오디오 소스

    [Header("[2순위] 발견됨 UI 설정")]
    public GameObject warningTextObj;
    public GameObject redVignetteObj;
    public float blinkSpeed = 0.2f;

    [Header("[3순위] 스텔스 UI 설정")]
    public GameObject stealthUIObj;

    private Coroutine _blinkCoroutine;
    private Coroutine _concussionPulseCoroutine; // 뇌진탕 울렁거림 코루틴

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        AllUIDisable();
    }

    void Update()
    {
        if (StealthDetector.Instance != null)
        {
            _isStealth = StealthDetector.Instance.isStealthMode;
        }

        DetermineActiveUI();
    }

    public void SetStunStatus(bool stunned) { _isStunned = stunned; }
    public void SetDetectedStatus(bool detected) { _isDetected = detected; }

    private void DetermineActiveUI()
    {
        UIState targetState = UIState.None;

        if (_isStunned) targetState = UIState.Stunned;
        else if (_isDetected) targetState = UIState.Detected;
        else if (_isStealth) targetState = UIState.Stealth;

        if (_currentDisplayState != targetState)
        {
            ApplyUIState(targetState);
        }
    }

    private void ApplyUIState(UIState newState)
    {
        AllUIDisable();
        _currentDisplayState = newState;

        switch (newState)
        {
            case UIState.Stunned:
                if (stunUIObj != null) stunUIObj.SetActive(true);

                // ★ 뇌진탕 연출: 삐-- 이명 소리 재생 및 시야 울렁거림 루틴 시작
                if (tinnitusAudioSource != null) tinnitusAudioSource.Play();
                if (stunVignetteImage != null)
                {
                    _concussionPulseCoroutine = StartCoroutine(ConcussionPulseRoutine());
                }
                break;

            case UIState.Detected:
                if (_blinkCoroutine == null)
                {
                    _blinkCoroutine = StartCoroutine(BlinkRoutine());
                }
                break;

            case UIState.Stealth:
                if (stealthUIObj != null) stealthUIObj.SetActive(true);
                break;
        }
    }

    private void AllUIDisable()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }

        // ★ 스턴 해제 시 코루틴 및 이명 사운드 정지
        if (_concussionPulseCoroutine != null)
        {
            StopCoroutine(_concussionPulseCoroutine);
            _concussionPulseCoroutine = null;
        }
        if (tinnitusAudioSource != null && tinnitusAudioSource.isPlaying)
        {
            tinnitusAudioSource.Stop();
        }

        if (stunUIObj != null) stunUIObj.SetActive(false);
        if (warningTextObj != null) warningTextObj.SetActive(false);
        if (redVignetteObj != null) redVignetteObj.SetActive(false);
        if (stealthUIObj != null) stealthUIObj.SetActive(false);
    }

    // ★ 발로란트 뇌진탕 스타일 시야 울렁거림 코루틴
    private IEnumerator ConcussionPulseRoutine()
    {
        Color baseColor = stunVignetteImage.color;

        while (true)
        {
            // Sin 함수를 이용하여 알파(투명도) 값을 0.4에서 0.8 사이로 부드럽고 불규칙하게 진동시킵니다.
            float pulse = Mathf.PingPong(Time.time * 3f, 0.4f) + 0.4f;
            baseColor.a = pulse;
            stunVignetteImage.color = baseColor;

            yield return null;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        bool toggle = false;
        while (true)
        {
            toggle = !toggle;
            if (warningTextObj != null) warningTextObj.SetActive(toggle);
            if (redVignetteObj != null) redVignetteObj.SetActive(toggle);
            yield return new WaitForSeconds(blinkSpeed);
        }
    }
}