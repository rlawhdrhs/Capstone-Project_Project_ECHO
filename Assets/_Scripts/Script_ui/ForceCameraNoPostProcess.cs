using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ForceCameraNoPostProcess : MonoBehaviour
{
    void Start()
    {
        var cam = GetComponent<Camera>();
        var urp = GetComponent<UniversalAdditionalCameraData>();

        Debug.Log("붙은 카메라 이름: " + gameObject.name);

        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            Debug.Log("Camera 확인됨");
        }

        if (urp != null)
        {
            urp.renderPostProcessing = false;
            urp.requiresDepthOption = CameraOverrideOption.Off;
            urp.requiresColorOption = CameraOverrideOption.Off;
            Debug.Log("URP Post Processing 강제 OFF");
        }
        else
        {
            Debug.LogWarning("UniversalAdditionalCameraData 없음");
        }
    }

    void OnPreCull()
    {
        RenderSettings.fog = false;
    }

    void OnPreRender()
    {
        RenderSettings.fog = false;
    }
}