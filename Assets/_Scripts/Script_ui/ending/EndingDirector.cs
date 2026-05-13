using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingDirector : MonoBehaviour
{
    public enum EndingSceneType
    {
        EscapeEnding,   // Intruder 승리 / Chaser 패배
        CaptureEnding   // Chaser 승리 / Intruder 패배
    }

    [Header("현재 엔딩 씬 타입")]
    public EndingSceneType endingSceneType;

    [Header("카메라")]
    public Camera intruderCamera;
    public Camera chaserCamera;

    [Header("Intruder UI")]
    public TMP_Text intruderEndingText;
    public Image intruderFadeImage;
    public GameObject intruderRestartButtonCube;

    [Header("Chaser UI")]
    public TMP_Text chaserEndingText;
    public Image chaserFadeImage;
    public GameObject chaserRestartButtonCube;

    [Header("설정")]
    public float endingDuration = 10f;
    public float fadeDuration = 3f;

    [Header("버튼 위치")]
    public float buttonDistance = 10f;
    public float buttonHeightOffset = -0.2f;

    void Start()
    {
        InitUI();

        SetEndingBySceneType();

        StartCoroutine(EndingSequence());
    }

    void InitUI()
    {
        intruderRestartButtonCube.SetActive(false);
        chaserRestartButtonCube.SetActive(false);

        intruderEndingText.gameObject.SetActive(false);
        chaserEndingText.gameObject.SetActive(false);

        intruderFadeImage.color = new Color(0, 0, 0, 0);
        chaserFadeImage.color = new Color(0, 0, 0, 0);
    }

    void SetEndingBySceneType()
    {
        intruderCamera.gameObject.SetActive(true);
        chaserCamera.gameObject.SetActive(true);

        switch (endingSceneType)
        {
            case EndingSceneType.EscapeEnding:

                // Intruder 승리
                intruderEndingText.text = "MISSION\nCOMPLETE";
                intruderEndingText.color = Color.blue;

                // Chaser 패배
                chaserEndingText.text = "SYSTEM\nSHUTDOWN";
                chaserEndingText.color = Color.red;

                break;

            case EndingSceneType.CaptureEnding:

                // Intruder 패배
                intruderEndingText.text = "ESCAPE\nFAILED";
                intruderEndingText.color = Color.red;

                // Chaser 승리
                chaserEndingText.text = "SECURITY\nMAINTAINED";
                chaserEndingText.color = Color.blue;

                break;
        }
    }

    IEnumerator EndingSequence()
    {
        yield return new WaitForSeconds(endingDuration);

        float timer = 0f;

        Color intruderColor = intruderFadeImage.color;
        Color chaserColor = chaserFadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(0f, 0.8f, timer / fadeDuration);

            intruderFadeImage.color =
                new Color(intruderColor.r, intruderColor.g, intruderColor.b, alpha);

            chaserFadeImage.color =
                new Color(chaserColor.r, chaserColor.g, chaserColor.b, alpha);

            yield return null;
        }

        intruderEndingText.gameObject.SetActive(true);
        chaserEndingText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        ShowButtonInFrontOfPlayer(
            intruderRestartButtonCube,
            intruderCamera
        );

        ShowButtonInFrontOfPlayer(
            chaserRestartButtonCube,
            chaserCamera
        );
    }

    void ShowButtonInFrontOfPlayer(GameObject button, Camera targetCamera)
    {
        Transform cam = targetCamera.transform;

        button.transform.position =
            cam.position +
            cam.forward * buttonDistance +
            cam.up * buttonHeightOffset;

        button.transform.rotation =
            Quaternion.LookRotation(button.transform.position - cam.position);

        button.transform.Rotate(0, 180f, 0);

        button.SetActive(true);
    }
}