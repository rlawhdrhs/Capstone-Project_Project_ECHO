using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingDirector_VR : MonoBehaviour
{
    [Header("카메라 교체 설정")]
    public Camera cinematicCamera;
    public GameObject xrOriginRoot;
    public Camera xrMainCamera;

    [Header("Ending UI & Canvas")]
    [Tooltip("fadeImage와 endingText를 담고 있는 최상위 Canvas")]
    public Canvas uiCanvas;
    public TMP_Text endingText;
    public Image fadeImage;
    public GameObject restartButtonCube;

    [Header("Scene Specific Settings")]
    [Multiline] public string displayMessage = "MISSION\nCOMPLETE";
    public Color messageColor = Color.blue;

    [Header("설정")]
    public float endingDuration = 10f;
    public float fadeDuration = 3f;
    public float buttonDistance = 2f;
    public float buttonHeightOffset = -0.2f;

    void Start()
    {
        InitSettings();
        StartCoroutine(EndingSequence());
    }

    void InitSettings()
    {
        cinematicCamera.gameObject.SetActive(true);
        xrOriginRoot.SetActive(false);

        restartButtonCube.SetActive(false);
        endingText.gameObject.SetActive(false);
        endingText.text = displayMessage;
        endingText.color = messageColor;

        fadeImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(endingDuration);

        float timer = 0f;
        Color currentColor = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        SwapCameraToVR();

        endingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        ShowButtonInFrontOfPlayer();
    }

    void SwapCameraToVR()
    {
        xrOriginRoot.transform.position = new Vector3(0f, -999f, 0f);

        Vector3 cinematicEuler = cinematicCamera.transform.eulerAngles;
        xrOriginRoot.transform.rotation = Quaternion.Euler(0f, cinematicEuler.y, 0f);

        cinematicCamera.gameObject.SetActive(false);
        xrOriginRoot.SetActive(true);

        if (uiCanvas != null)
        {
            uiCanvas.renderMode = RenderMode.WorldSpace;
            uiCanvas.transform.SetParent(xrMainCamera.transform);
            uiCanvas.transform.localPosition = new Vector3(0f, 0f, 3.5f);
            uiCanvas.transform.localRotation = Quaternion.identity;
            uiCanvas.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
        }
    }

    void ShowButtonInFrontOfPlayer()
    {
        Transform cam = xrMainCamera.transform;
        Vector3 camForward = cam.forward;
        camForward.y = 0;

        if (camForward.sqrMagnitude < 0.01f)
        {
            camForward = cam.up;
            camForward.y = 0;
        }
        camForward.Normalize();

        restartButtonCube.transform.position = cam.position + (camForward * buttonDistance) + (Vector3.up * buttonHeightOffset);
        restartButtonCube.transform.rotation = Quaternion.LookRotation(cam.position - restartButtonCube.transform.position);

        restartButtonCube.SetActive(true);
    }
}