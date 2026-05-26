using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    public static AudioSettingsManager Instance;
    public float MasterVolume { get; private set; }
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Value Texts")]
    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Buttons")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    private AudioSettingsManager audioSettingsManager;

    private void Start()
    {
        audioSettingsManager = AudioSettingsManager.Instance;

        if (audioSettingsManager == null)
        {
            Debug.LogError("[SoundSettingsUI] AudioSettingsManager is missing in the scene.");
            return;
        }

        InitializeSliders();
        RegisterEvents();
    }

    private void InitializeSliders()
    {
        SetupSlider(masterVolumeSlider, audioSettingsManager.MasterVolume);
        SetupSlider(bgmVolumeSlider, audioSettingsManager.BGMVolume);
        SetupSlider(sfxVolumeSlider, audioSettingsManager.SFXVolume);

        UpdateMasterValueText(audioSettingsManager.MasterVolume);
        UpdateBGMValueText(audioSettingsManager.BGMVolume);
        UpdateSFXValueText(audioSettingsManager.SFXVolume);
    }

    private void SetupSlider(Slider slider, float value)
    {
        if (slider == null) return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.SetValueWithoutNotify(value);
    }

    private void RegisterEvents()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnClickReset);

        if (backButton != null)
            backButton.onClick.AddListener(OnClickBack);
    }

    private void OnMasterVolumeChanged(float value)
    {
        audioSettingsManager.SetMasterVolume(value);
        UpdateMasterValueText(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        audioSettingsManager.SetBGMVolume(value);
        UpdateBGMValueText(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        audioSettingsManager.SetSFXVolume(value);
        UpdateSFXValueText(value);
    }

    private void UpdateMasterValueText(float value)
    {
        UpdateValueText(masterValueText, value);
    }

    private void UpdateBGMValueText(float value)
    {
        UpdateValueText(bgmValueText, value);
    }

    private void UpdateSFXValueText(float value)
    {
        UpdateValueText(sfxValueText, value);
    }

    private void UpdateValueText(TMP_Text targetText, float value)
    {
        if (targetText == null) return;

        int percent = Mathf.RoundToInt(value * 100f);
        targetText.text = percent + "%";
    }

    private void OnClickReset()
    {
        audioSettingsManager.ResetVolumes();
        InitializeSliders();
    }

    private void OnClickBack()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (audioSettingsManager == null) return;

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(OnClickReset);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnClickBack);
    }
}