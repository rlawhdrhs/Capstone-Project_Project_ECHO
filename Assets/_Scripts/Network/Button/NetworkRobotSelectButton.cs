using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkRobotSelectButton : MonoBehaviour
{
    [Header("Network Targeting")]
    [Tooltip("이 버튼을 누르면 조종하게 될 센서 로봇의 고유 ID")]
    public int targetSensorId;

    [Header("Button Effect")]
    public float pressedScale = 0.8f;
    public float speed = 0.08f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private bool isAnimating = false;

    public BlinkTransition blinkTransition;
    public WakeUpTransition wakeUpTransition;

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
        // 추격자(Client)가 PC 마우스로 클릭했을 때 작동
        PressButton();
    }

    void OnXRPressed(SelectEnterEventArgs args)
    {
        // 잠입자(VR)가 컨트롤러로 클릭했을 때 작동 (필요시 권한 체크로 막을 수 있음)
        PressButton();
    }

    void PressButton()
    {
        if (!isAnimating)
        {
            StartCoroutine(ButtonRoutine());
        }
    }

    IEnumerator ButtonRoutine()
    {
        isAnimating = true;

        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }

        Vector3 smallScale = originalScale * pressedScale;
        float t = 0f;

        while (t < speed)
        {
            transform.localScale = Vector3.Lerp(originalScale, smallScale, t / speed);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = smallScale;
        yield return new WaitForSeconds(0.05f);
        t = 0f;

        while (t < speed)
        {
            transform.localScale = Vector3.Lerp(smallScale, originalScale, t / speed);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;

        // 트랜지션 연출 후 빙의 함수 실행
        if (wakeUpTransition != null)
        {
            wakeUpTransition.PlayWakeUp(PossessChaser);
        }
        else
        {
            PossessChaser();
        }
    }

    void PossessChaser()
    {
        if (NetworkSensorManager.Instance != null)
        {
            NetworkSensorManager.Instance.SwitchToSensorById(targetSensorId);
        }
        else
        {
            Debug.LogError("씬에 NetworkSensorManager가 없습니다!");
        }
    }

    void OnDisable()
    {
        isAnimating = false;
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }
    }
}