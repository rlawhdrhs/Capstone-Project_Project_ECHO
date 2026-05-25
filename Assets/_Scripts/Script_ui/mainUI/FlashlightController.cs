using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Light")]
    public Light flashlight;

    [Header("Mouse Button")]
    public KeyCode toggleKey = KeyCode.Mouse0; // 왼쪽 클릭

    void Start()
    {
        // 자동으로 Light 가져오기
        if (flashlight == null)
        {
            flashlight = GetComponent<Light>();
        }
    }

    void Update()
    {
        // 마우스 클릭 시 ON/OFF
        if (Input.GetKeyDown(toggleKey))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}