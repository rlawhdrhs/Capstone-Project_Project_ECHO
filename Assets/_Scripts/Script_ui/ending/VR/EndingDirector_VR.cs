using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingDirector_VR : MonoBehaviour
{
    [Header("카메라 교체 설정")]
    [Tooltip("연출이 진행되는 동안 켜져 있을 기존 일반 카메라")]
    public Camera cinematicCamera;

    [Tooltip("연출이 끝난 후 켜질 플레이어의 XR Origin 최상위 객체")]
    public GameObject xrOriginRoot;

    [Tooltip("XR Origin 하위에 있는 Main Camera (버튼 위치 계산용)")]
    public Camera xrMainCamera;

    [Header("Ending UI")]
    public TMP_Text endingText;
    public Image fadeImage;
    public GameObject restartButtonCube;

    [Header("Scene Specific Settings")]
    [Tooltip("이 씬에 출력될 텍스트 (예: MISSION COMPLETE)")]
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
        // 1. 초기 상태: 연출 카메라는 켜고, XR Origin은 끈다.
        cinematicCamera.gameObject.SetActive(true);
        xrOriginRoot.SetActive(false);

        // 2. UI 초기화
        restartButtonCube.SetActive(false);
        endingText.gameObject.SetActive(false);
        endingText.text = displayMessage;
        endingText.color = messageColor;

        fadeImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator EndingSequence()
    {
        // 연출 시간만큼 대기 (이 동안 cinematicCamera가 움직임)
        yield return new WaitForSeconds(endingDuration);

        // 페이드 아웃 (화면 까매짐)
        float timer = 0f;
        Color currentColor = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            yield return null;
        }

        // --- 🌟 핵심: 카메라 스왑 및 순간이동 ---
        SwapCameraToVR();

        endingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        // 버튼 띄우기 (이제 XR 카메라를 기준으로 띄움)
        ShowButtonInFrontOfPlayer();
    }

    void SwapCameraToVR()
    {
        // 1. XR Origin의 위치를 연출 카메라의 마지막 위치로 순간이동
        xrOriginRoot.transform.position = cinematicCamera.transform.position;

        // 2. XR Origin의 회전값을 연출 카메라의 Y축 회전값에 맞춤 (X, Z는 멀미 방지를 위해 무시)
        Vector3 cinematicEuler = cinematicCamera.transform.eulerAngles;
        xrOriginRoot.transform.rotation = Quaternion.Euler(0f, cinematicEuler.y, 0f);

        // 3. 연출 카메라 끄고, XR Origin 켜기
        cinematicCamera.gameObject.SetActive(false);
        xrOriginRoot.SetActive(true);
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