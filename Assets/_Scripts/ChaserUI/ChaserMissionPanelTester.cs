using UnityEngine;

public class ChaserMissionPanelTester : MonoBehaviour
{
    [SerializeField] private ChaserMissionPanelUI panelUI;

    private float progress;

    private void Update()
    {
        if (panelUI == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            panelUI.SetPowerRestored();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            panelUI.SetDataInProgress();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            progress += 0.15f;
            panelUI.UpdateDataProgress(progress);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            panelUI.ShowSoundDetected(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            panelUI.ShowSoundDetected(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            panelUI.SetDataCompleted();
            panelUI.SetExitUnlocked();
            panelUI.UpdateDataProgress(1f);
        }
    }
}