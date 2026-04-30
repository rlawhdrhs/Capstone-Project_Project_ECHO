using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MapButton : MonoBehaviour
{
    [Header("Map UI")]
    public GameObject mapPanel;

    [Header("Camera Target")]
    public Transform cameraTarget;

    [Header("Cube Position")]
    public float cubeForward = 1.2f;
    public float cubeRight = 0.7f;
    public float cubeUp = -0.1f;

    [Header("Map Position")]
    public float mapForward = 1.5f;
    public float mapUp = 0f;

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
            Debug.Log("XR 이벤트 연결됨: " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("XR Simple Interactable 없음: " + gameObject.name);
        }

        if (mapPanel == null)
            Debug.LogError("Map Panel이 Inspector에 연결되지 않았음!");

        if (cameraTarget == null)
            Debug.LogError("Camera Target이 Inspector에 연결되지 않았음!");
    }

    void OnMouseDown()
    {
        Debug.Log("마우스로 Map 버튼 클릭됨");
        PressButton();
    }

    void OnXRPressed(SelectEnterEventArgs args)
    {
        Debug.Log("XR로 Map 버튼 클릭됨");
        PressButton();
    }

    void PressButton()
    {
        if (isAnimating)
        {
            Debug.Log("이미 애니메이션 중이라 무시됨");
            return;
        }

        ToggleMap();

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

        isAnimating = false;
    }

    void ToggleMap()
    {
        if (mapPanel == null)
        {
            Debug.LogError("Map Panel이 null이라 지도 토글 불가");
            return;
        }

        bool nextState = !mapPanel.activeSelf;
        mapPanel.SetActive(nextState);

        Debug.Log("지도 상태 변경됨: " + nextState);

        if (nextState && cameraTarget != null)
        {
            mapPanel.transform.position =
                cameraTarget.position
                + cameraTarget.forward * mapForward
                + cameraTarget.up * mapUp;

            mapPanel.transform.LookAt(cameraTarget);
            mapPanel.transform.Rotate(0f, 180f, 0f);

            // 지도가 뒤집혀 보이면 이 줄 주석 해제
            // mapPanel.transform.Rotate(0f, 180f, 0f);
        }
    }

    void LateUpdate()
    {
        if (cameraTarget == null) return;

        transform.position =
            cameraTarget.position
            + cameraTarget.forward * cubeForward
            + cameraTarget.right * cubeRight
            + cameraTarget.up * cubeUp;

        transform.LookAt(cameraTarget);

        // 큐브 앞면이 반대로 보이면 이 줄 주석 해제
        // transform.Rotate(0f, 180f, 0f);
    }
}