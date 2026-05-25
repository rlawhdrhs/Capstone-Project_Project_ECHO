using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

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
            masterVolumeSlider.onValueChanged.AddListener(audioSettingsManager.SetMasterVolume);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(audioSettingsManager.SetBGMVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(audioSettingsManager.SetSFXVolume);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnClickReset);

        if (backButton != null)
            backButton.onClick.AddListener(OnClickBack);
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
            masterVolumeSlider.onValueChanged.RemoveListener(audioSettingsManager.SetMasterVolume);

        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.RemoveListener(audioSettingsManager.SetBGMVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(audioSettingsManager.SetSFXVolume);
    }
}