using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRButton : MonoBehaviour
{
    [Header("Animation Settings")]
    public float pressedScale = 0.8f;
    public float speed = 0.08f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    [Header("Button Event")]
    public UnityEvent onClick; // 인스펙터에서 원하는 함수를 연결할 수 있게 함

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

    public void PressButton()
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

        // 애니메이션이 끝난 후 이벤트 실행
        onClick.Invoke();

        yield return new WaitForSeconds(0.2f);
        isPressed = false; // 버튼 재사용을 위해 초기화
    }
}