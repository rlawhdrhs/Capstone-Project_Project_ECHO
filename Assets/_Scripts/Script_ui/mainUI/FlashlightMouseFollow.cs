using UnityEngine;

public class FlashlightMouseFollow : MonoBehaviour
{
    public Light flashlight;
    public Camera targetCamera;

    public KeyCode toggleKey = KeyCode.Mouse0;

    void Start()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            flashlight.enabled = !flashlight.enabled;

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        transform.rotation = Quaternion.LookRotation(ray.direction);
    }
}