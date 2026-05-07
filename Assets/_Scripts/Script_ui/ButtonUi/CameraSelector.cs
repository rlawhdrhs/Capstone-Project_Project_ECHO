using UnityEngine;

public class CameraSelector : MonoBehaviour
{
    public GameObject intruderCamera;
    public GameObject chaserCamera;

    void Start()
    {
        Debug.Log("CameraMode = " + GameEntryData.cameraMode);

        intruderCamera.SetActive(false);
        chaserCamera.SetActive(false);

        if (GameEntryData.cameraMode == "Intruder")
        {
            intruderCamera.SetActive(true);
            Debug.Log("Intruder Camera ON");
        }
        else if (GameEntryData.cameraMode == "Chaser")
        {
            chaserCamera.SetActive(true);
            Debug.Log("Chaser Camera ON");
        }
        else
        {
            Debug.LogWarning("CameraMode 값이 비어있거나 잘못됨: " + GameEntryData.cameraMode);
        }
    }
}