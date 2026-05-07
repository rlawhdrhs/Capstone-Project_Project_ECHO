using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ChooseSceneAndCharacter : MonoBehaviour
{
    [Header("Scene")]
    public string targetSceneName;

    [Header("Camera Mode")]
    public string cameraMode;

    [Header("Scale Animation")]
    public float pressedScale = 0.8f;
    public float speed = 0.08f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private bool isPressed = false;

    void Start()
    {
        originalScale = transform.localScale;

        XRBaseInteractable interactable = GetComponent<XRBaseInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnXRPressed);
        }
    }

    void OnMouseDown()
    {
        PressButton();
    }

    void OnXRPressed(SelectEnterEventArgs args)
    {
        PressButton();
    }

    void PressButton()
    {
        if (isPressed) return;
        isPressed = true;

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        StartCoroutine(ButtonRoutine());
    }

    IEnumerator ButtonRoutine()
    {
        Vector3 targetScale = originalScale * pressedScale;

        float t = 0f;
        while (t < speed)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t / speed);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        while (t < speed)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t / speed);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;

        yield return new WaitForSeconds(0.2f);

        GameEntryData.cameraMode = cameraMode;

        SceneManager.LoadScene(targetSceneName);
    }
}