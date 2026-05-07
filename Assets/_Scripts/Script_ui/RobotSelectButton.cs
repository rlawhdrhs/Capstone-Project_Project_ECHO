using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RobotSelectButton : MonoBehaviour
{
    [Header("Teleport")]
    public Transform playerRoot;
    public Transform viewPoint;

    [Header("Global Volume")]
    public GameObject globalVolume;
    public bool turnOnGlobalVolumeOnClick = true;

    [Header("Button Effect")]
    public float pressedScale = 0.8f;
    public float speed = 0.08f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private bool isAnimating = false;

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
        if (isAnimating) return;

        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        StartCoroutine(ButtonRoutine());
    }

    IEnumerator ButtonRoutine()
    {
        isAnimating = true;

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

        // 여기서 실행
        if (playerRoot != null && viewPoint != null)
        {
            playerRoot.position = viewPoint.position;
            Debug.Log($"이동 목표: {viewPoint.position} / 실제 이동후 위치: {playerRoot.position}");

            playerRoot.rotation = viewPoint.rotation;
        }

        if (turnOnGlobalVolumeOnClick && globalVolume != null)
        {
            globalVolume.SetActive(true);
        }

        isAnimating = false;
    }

}