using UnityEngine;

public class SensorControlInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SensorManager.Instance.SwitchToSensorByIndex(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SensorManager.Instance.SwitchToSensorByIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SensorManager.Instance.SwitchToSensorByIndex(2);
        }
    }
}