using System.Collections.Generic;
using UnityEngine;

public class SensorManager : MonoBehaviour
{
    public static SensorManager Instance;

    public List<SoundSensor> sensors = new List<SoundSensor>();

    public SoundSensor lastDetectedSensor;
    public SoundSensor currentControlledSensor;

    public Vector3 lastDetectedPosition;
    public float lastDetectedIntensity;

    public Camera mainCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void RegisterSensor(SoundSensor sensor)
    {
        if (!sensors.Contains(sensor))
            sensors.Add(sensor);
    }

    public void ReportDetection(SoundSensor sensor, SoundData sound, float intensity)
    {
        lastDetectedSensor = sensor;
        lastDetectedPosition = sound.position;
        lastDetectedIntensity = intensity;

        Debug.Log($"[SensorManager] {sensor.name} 감지 / 위치: {sound.position} / 강도: {intensity}");
    }

    public void SwitchControlToSensor(SoundSensor targetSensor)
    {
        if (targetSensor == null) return;

        if (currentControlledSensor != null)
            currentControlledSensor.SetControlled(false);

        currentControlledSensor = targetSensor;
        currentControlledSensor.SetControlled(true);

        if (mainCamera != null && targetSensor.cameraPoint != null)
        {
            mainCamera.transform.SetParent(targetSensor.cameraPoint);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;
        }

        Debug.Log($"{targetSensor.name} 조종 시작");
    }

    public void SwitchToSensorByIndex(int index)
    {
        if (index < 0 || index >= sensors.Count)
        {
            Debug.Log("해당 번호의 센서 없음");
            return;
        }

        SwitchControlToSensor(sensors[index]);
    }
}