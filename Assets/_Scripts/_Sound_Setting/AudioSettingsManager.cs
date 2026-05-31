using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MasterVolumeParam = "MasterVolume";
    private const string BGMVolumeParam = "BGMVolume";
    private const string SFXVolumeParam = "SFXVolume";

    private const string MasterVolumeKey = "MasterVolume";
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";

    private const float DefaultVolume = 0.5f;
    private const float MinLinearVolume = 0.0001f;

    public float MasterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float SFXVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
        ApplyAllVolumes();
    }

    private void LoadVolumes()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, DefaultVolume);
        BGMVolume = PlayerPrefs.GetFloat(BGMVolumeKey, DefaultVolume);
        SFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume);
    }

    private void ApplyAllVolumes()
    {
        SetMixerVolume(MasterVolumeParam, MasterVolume);
        SetMixerVolume(BGMVolumeParam, BGMVolume);
        SetMixerVolume(SFXVolumeParam, SFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        SetMixerVolume(MasterVolumeParam, MasterVolume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float value)
    {
        BGMVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BGMVolumeKey, BGMVolume);
        SetMixerVolume(BGMVolumeParam, BGMVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SFXVolumeKey, SFXVolume);
        SetMixerVolume(SFXVolumeParam, SFXVolume);
        PlayerPrefs.Save();
    }

    private void SetMixerVolume(string parameterName, float linearValue)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("[AudioSettingsManager] AudioMixer is not assigned.");
            return;
        }

        float clampedValue = Mathf.Clamp(linearValue, MinLinearVolume, 1f);
        float volumeDb = Mathf.Log10(clampedValue) * 66.4386f + 20f;

        if (linearValue <= MinLinearVolume)
        {
            volumeDb = -80f;
        }

        audioMixer.SetFloat(parameterName, volumeDb);
    }

    public void ResetVolumes()
    {
        SetMasterVolume(DefaultVolume);
        SetBGMVolume(DefaultVolume);
        SetSFXVolume(DefaultVolume);
    }
}