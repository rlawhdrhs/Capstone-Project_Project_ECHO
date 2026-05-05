using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MapButton : MonoBehaviour
{
    [Header("Player / Camera Target")]
    public Transform cameraTarget;

    [Header("Teleport Target")]
    public Transform CCTVRoomViewPoint;
    public Transform playerRoot;
    // 보통 XR Origin 또는 Player 루트 넣기

    [Header("Button Follow Position")]
    public float cubeForward = 1.2f;
    public float cubeRight = 0.7f;
    public float cubeUp = -0.1f;

    [Header("Button Effect")]
    public float pressedScale = 0.8f;
    public float speed = 0.08f;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip clickSound;


    [Header("Global Volume")]
    public GameObject globalVolume;
    public bool turnOffGlobalVolumeOnClick = true;


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

        if (cameraTarget == null)
            Debug.LogError("Camera Target이 연결되지 않았습니다.");

        if (CCTVRoomViewPoint == null)
            Debug.LogError("CCTVRoomViewPoint가 연결되지 않았습니다.");

        if (playerRoot == null)
            Debug.LogError("Player Root가 연결되지 않았습니다.");
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

        if(turnOffGlobalVolumeOnClick && globalVolume != null)
        {
            globalVolume.SetActive(false);
        }


        TeleportToCCTVRoom();

        isAnimating = false;
    }

    void TeleportToCCTVRoom()
    {
        if (playerRoot == null || CCTVRoomViewPoint == null)
        {
            Debug.LogError("텔레포트 실패: Player Root 또는 CCTVRoomViewPoint가 없습니다.");
            return;
        }

        playerRoot.position = CCTVRoomViewPoint.position;
        playerRoot.rotation = CCTVRoomViewPoint.rotation;

        Debug.Log("CCTVRoomViewPoint로 이동 완료");
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
        transform.Rotate(0f, 180f, 0f);
    }
}