using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntruderStatusUIManager : MonoBehaviour
{
    public static IntruderStatusUIManager Instance;

    private bool _isStunned = false;
    private bool _isDetected = false;
    private bool _isStealth = false;

    private enum UIState { None, Stealth, Detected, Stunned }
    private UIState _currentDisplayState = UIState.None;

    [Header("스턴 (뇌진탕) 설정")]
    public GameObject stunUIObj;        // 화면을 덮을 스턴 캔버스 오버레이
    public Image stunVignetteImage;     // 스턴 UI 내부의 Image 컴포넌트 (울렁거림 연출용)
    public AudioSource tinnitusAudioSource; // "삐--" 소리를 재생할 오디오 소스

    [Header("발견됨 UI 설정")]
    public GameObject warningTextObj;
    public GameObject redVignetteObj;
    public float blinkSpeed = 0.2f;

    [Header("스텔스 UI 설정")]
    public GameObject stealthUIObj;

    [Header("추격자 미션 텍스트 UI")]
    public TMP_Text mission1StatusText; // 미션 1 상태 텍스트
    public TMP_Text mission2StatusText; // 미션 2 상태 텍스트
    public TMP_Text mission3StatusText; // 미션 3 상태 텍스트

    [Header("추격자 미션 2 슬라이더 UI")]
    public Slider mission2Slider;          // 미션 2 진행도 슬라이더 (0 ~ 1)
    public TMP_Text mission2PercentText;  // 미션 2 % 표시 텍스트

    [Header("추격자 사운드 감지 UI")]
    public TMP_Text soundDetectionText;

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

        bool isAnyDroneDetectingSound = false;

        for (int i = 0; i < IntruderSoundDetector_Network.Detectors.Count; i++)
        {
            var detector = IntruderSoundDetector_Network.Detectors[i];
            if (detector != null && detector.IsSoundDetected)
            {
                isAnyDroneDetectingSound = true;
                break;
            }
        }
        UpdateSoundStatusUI(isAnyDroneDetectingSound);
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

    private IEnumerator ConcussionPulseRoutine()
    {
        if (stunVignetteImage != null)
        {
            Color baseColor = stunVignetteImage.color;
            stunVignetteImage.color = baseColor;
        }
        while (true)
        {
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

    public void UpdateMissionUI(bool isM1Cleared, float m2ProgressFraction, bool isM2Cleared, bool isM3Cleared)
    {
        // 1. 미션 1 상태 처리 (전력 복구)
        if (mission1StatusText != null)
        {
            mission1StatusText.text = isM1Cleared ? "POWER                        ONLINE" : "POWER                        OFFLINE";
            mission1StatusText.color = isM1Cleared ? Color.green : Color.red;
        }

        // 2. 미션 2 상태 처리 (데이터 수집)
        if (!isM1Cleared)
        {
            // 미션 1이 깨지기 전엔 무조건 LOCKED
            if (mission2StatusText != null) { mission2StatusText.text = "DATA                             LOCKED"; mission2StatusText.color = Color.gray; }
            if (mission2Slider != null) mission2Slider.value = 0f;
            if (mission2PercentText != null) mission2PercentText.text = "0%";
        }
        else
        {
            // 미션 1이 깨졌으면, 미션 2 자체의 클리어 여부에 따라 CLEAR / UNLOCK 분기
            if (mission2StatusText != null)
            {
                mission2StatusText.text = isM2Cleared ? "DATA                             CLEAR" : "DATA                             UNLOCK";
                mission2StatusText.color = isM2Cleared ? Color.green : Color.red;
            }

            // 미션 2 슬라이더 및 퍼센트 실시간 반영
            if (mission2Slider != null) mission2Slider.value = Mathf.Clamp01(m2ProgressFraction);
            if (mission2PercentText != null)
            {
                int percent = Mathf.RoundToInt(Mathf.Clamp01(m2ProgressFraction) * 100f);
                mission2PercentText.text = $"{percent}%";
            }
        }

        // 3. 미션 3 상태 처리 (탈출구)
        if (!isM1Cleared || !isM2Cleared)
        {
            // ★ [핵심] 미션 1이나 미션 2 중 하나라도 안 깨졌다면 미션 3은 예외 없이 무조건 LOCKED 고정
            if (mission3StatusText != null) { mission3StatusText.text = "EXIT                              LOCKED"; mission3StatusText.color = Color.gray; }
        }
        else
        {
            // 미션 1, 2가 둘 다 완벽히 깨졌을 때만 비로소 미션 3이 활성화됨 (UNLOCK 또는 ESCAPE)
            if (mission3StatusText != null)
            {
                mission3StatusText.text = isM3Cleared ? "EXIT                              ESCAPE" : "EXIT                              UNLOCK";
                mission3StatusText.color = isM3Cleared ? Color.green : Color.red;
            }
        }
    }

    public void UpdateSoundStatusUI(bool isSoundDetected)
    {
        if (soundDetectionText != null)
        {
            soundDetectionText.text = isSoundDetected ? "Detect Sound" : "No Recent Sound";
            soundDetectionText.color = isSoundDetected ? Color.yellow : Color.white; // 가독성을 위한 색상 변화 피드백
        }
    }
}