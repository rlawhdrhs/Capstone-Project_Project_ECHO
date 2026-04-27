using UnityEngine;

public class SensorControlInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SensorManager.Instance.SwitchToSensorById(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SensorManager.Instance.SwitchToSensorById(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SensorManager.Instance.SwitchToSensorById(3);
        }
    }
}