using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChaserMissionPanelUI : MonoBehaviour
{
    [Header("Mission Status Texts")]
    [SerializeField] private TMP_Text powerStatusText;
    [SerializeField] private TMP_Text dataStatusText;
    [SerializeField] private TMP_Text exitStatusText;

    [Header("Data Breach UI")]
    [SerializeField] private Slider dataProgressSlider;
    [SerializeField] private TMP_Text dataPercentText;

    [Header("Sound Alert UI")]
    [SerializeField] private TMP_Text soundAlertText;

    [Header("Sound Alert Settings")]
    [SerializeField] private string idleSoundMessage = "NO RECENT SOUND";
    [SerializeField] private string soundAlertFormat = "SOUND DETECTED : ZONE {0}";

    [SerializeField] private Color idleSoundColor = new Color(0.65f, 0.9f, 1f, 0.7f);
    [SerializeField] private Color alertSoundColor = Color.red;

    [SerializeField] private float blinkInterval = 0.25f;
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private float remainTimeAfterBlink = 0.5f;

    private Coroutine soundAlertCoroutine;

    private void Start()
    {
        SetMissionStatus("POWER                        OFFLINE", "DATA                             LOCKED", "EXIT                              LOCKED");
        UpdateDataProgress(0f);
        SetSoundAlertIdle();
    }

    public void SetMissionStatus(string powerStatus, string dataStatus, string exitStatus)
    {
        if (powerStatusText != null)
            powerStatusText.text = powerStatus;

        if (dataStatusText != null)
            dataStatusText.text = dataStatus;

        if (exitStatusText != null)
            exitStatusText.text = exitStatus;
    }

    public void SetPowerRestored()
    {
        if (powerStatusText != null)
            powerStatusText.text = "POWER                     RESTORED";
    }

    public void SetDataInProgress()
    {
        if (dataStatusText != null)
            dataStatusText.text = "DATA                    IN PROGRESS";
    }

    public void SetDataCompleted()
    {
        if (dataStatusText != null)
            dataStatusText.text = "DATA                       COMPLETED";
    }

    public void SetExitUnlocked()
    {
        if (exitStatusText != null)
            exitStatusText.text = "EXIT                         UNLOCKED";
    }

    public void UpdateDataProgress(float progress)
    {
        float clampedProgress = Mathf.Clamp01(progress);

        if (dataProgressSlider != null)
        {
            dataProgressSlider.minValue = 0f;
            dataProgressSlider.maxValue = 1f;
            dataProgressSlider.SetValueWithoutNotify(clampedProgress);
        }

        if (dataPercentText != null)
        {
            int percent = Mathf.RoundToInt(clampedProgress * 100f);
            dataPercentText.text = percent + "%";
        }
    }

    public void ShowSoundDetected(int zoneIndex)
    {
        if (soundAlertText == null)
        {
            Debug.LogWarning("[ChaserMissionPanelUI] SoundAlertText is not assigned.");
            return;
        }

        if (soundAlertCoroutine != null)
        {
            StopCoroutine(soundAlertCoroutine);
        }

        soundAlertCoroutine = StartCoroutine(BlinkSoundAlert(zoneIndex));
    }

    private IEnumerator BlinkSoundAlert(int zoneIndex)
    {
        string message = string.Format(soundAlertFormat, zoneIndex);

        for (int i = 0; i < blinkCount; i++)
        {
            soundAlertText.text = message;
            soundAlertText.color = alertSoundColor;
            soundAlertText.enabled = true;
            yield return new WaitForSeconds(blinkInterval);

            soundAlertText.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
        }

        soundAlertText.enabled = true;
        soundAlertText.text = message;
        soundAlertText.color = alertSoundColor;

        yield return new WaitForSeconds(remainTimeAfterBlink);

        SetSoundAlertIdle();
        soundAlertCoroutine = null;
    }

    private void SetSoundAlertIdle()
    {
        if (soundAlertText == null) return;

        soundAlertText.enabled = true;
        soundAlertText.text = idleSoundMessage;
        soundAlertText.color = idleSoundColor;
    }
}