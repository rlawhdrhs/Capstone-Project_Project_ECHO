using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class RobotSelectButton : MonoBehaviour
{
    [Header("Camera Body")]
    public Transform playerBody;        // Main Camera가 들어있는 부모 오브젝트

    [Header("Possession Target")]
    public Transform targetViewPoint;   // 선택할 Chaser 안의 ViewPoint
    public GreenMove[] allGreenMoves;   // 모든 Chaser의 GreenMove
    public GreenMove targetGreenMove;   // 이 버튼이 조종할 Chaser의 GreenMove

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
        PressButton();
    }

    void OnXRPressed(SelectEnterEventArgs args)
    {
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

        //PossessChaser();

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

        if (playerBody == null)
        {
            Debug.LogError("Player Body가 비어있음");
            return;
        }

        if (targetViewPoint == null)
        {
            Debug.LogError("Target ViewPoint가 비어있음");
            return;
        }

        if (targetGreenMove == null)
        {
            Debug.LogError("Target GreenMove가 비어있음");
            return;
        }

        // 1. 모든 Chaser 이동 끄기
        foreach (GreenMove move in allGreenMoves)
        {
            if (move != null)
            {
                move.enabled = false;
            }
        }

        // 2. 선택한 Chaser만 이동 켜기
        targetGreenMove.enabled = true;

        // 3. 카메라 몸통을 선택한 Chaser의 ViewPoint에 붙이기
        //playerBody.SetParent(targetViewPoint);
        //playerBody.SetParent(targetViewPoint, false);

        // 4. ViewPoint 위치와 회전에 딱 맞추기
        //playerBody.localPosition = Vector3.zero;
        //playerBody.localRotation = Quaternion.identity;


        //veiwpoint자리차지코드
        //Transform chaserBody = targetGreenMove.transform;

        //playerBody.SetParent(chaserBody, false);
        //playerBody.localPosition = targetViewPoint.localPosition;
        //playerBody.localRotation = targetViewPoint.localRotation;

        //Vector3 bodyEuler = chaserBody.eulerAngles;
        //bodyEuler.y = targetViewPoint.eulerAngles.y;
        //chaserBody.eulerAngles = bodyEuler;

        playerBody.SetParent(targetViewPoint, false);

        playerBody.localPosition = Vector3.zero;
        playerBody.localRotation = Quaternion.identity;


        Debug.Log("현재 움직이는 Chaser: " + targetGreenMove.gameObject.name);
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