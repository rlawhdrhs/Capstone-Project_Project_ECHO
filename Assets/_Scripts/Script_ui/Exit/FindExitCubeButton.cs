using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FindExitCubeButton : MonoBehaviour
{
    [Header("눌림 효과")]
    public float pressScale = 0.85f;
    public float pressTime = 0.08f;

    [Header("큐브 클릭 소리")]
    public AudioSource cubeAudioSource;
    public AudioClip cubeClickSound;

    [Header("Exit 소리")]
    public AudioSource ExitAudioSource;
    public AudioClip ExitSound;

    [Header("소리 제어")]
    public ExitDoorSoundOnly soundManager;

    [Header("문 클릭 가능")]
    public ExitDoorController_VR exitDoorController;

    private Vector3 originalScale;
    private bool isPressed = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    //void Update()
    //{
    //    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        Vector2 mousePos = Mouse.current.position.ReadValue();
    //        Ray ray = Camera.main.ScreenPointToRay(mousePos);

    //        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    //        {
    //            if (hit.collider.gameObject == gameObject)
    //            {
    //                Debug.Log("FindExitCube 클릭됨");
    //                PressButton();
    //            }
    //        }
    //    }
    //}

    public void PressButton()
    {
        if (isPressed) return;
        StartCoroutine(PressEffect());
    }

    IEnumerator PressEffect()
    {
        isPressed = true;

        if (cubeAudioSource != null && cubeClickSound != null)
        {
            cubeAudioSource.PlayOneShot(cubeClickSound);
        }

        transform.localScale = originalScale * pressScale;

        yield return new WaitForSeconds(pressTime);

        transform.localScale = originalScale;

        if (soundManager != null)
        {
            soundManager.PlayOnlyExitDoorSounds();
        }

        if (ExitAudioSource != null && ExitSound != null)
        {
            ExitAudioSource.PlayOneShot(ExitSound);
        }

        if (exitDoorController != null)
        {
            exitDoorController.EnableDoorClick();
        }

        isPressed = false;
    }
}





//using UnityEngine;
//using System.Collections;

//public class FindExitCubeButton : MonoBehaviour
//{
//    [Header("눌림 효과")]
//    public float pressScale = 0.85f;
//    public float pressTime = 0.08f;

//    [Header("큐브 클릭 소리")]
//    public AudioSource cubeAudioSource;
//    public AudioClip cubeClickSound;

//    [Header("Exit 소리")]
//    public AudioSource ExitAudioSource;
//    public AudioClip ExitSound;

//    private Vector3 originalScale;
//    private bool isPressed = false;
//    public ExitDoorSoundOnly soundManager;

//    void Start()
//    {
//        originalScale = transform.localScale;
//    }

//    void OnMouseDown()
//    {
//        Debug.Log("FindExitCube 클릭됨");
//        PressButton();
//    }

//    public void PressButton()
//    {
//        if (isPressed) return;
//        StartCoroutine(PressEffect());
//    }

//    IEnumerator PressEffect()
//    {
//        isPressed = true;

//        // 큐브 클릭 소리
//        if (cubeAudioSource != null && cubeClickSound != null)
//        {
//            //cubeAudioSource.PlayOneShot(cubeClickSound);
//            soundManager.PlayOnlyExitDoorSounds();
//        }

//        // 문 쪽에서 나는 소리
//        if (ExitAudioSource != null && ExitSound != null)
//        {
//            ExitAudioSource.PlayOneShot(ExitSound);
//        }

//        // 큐브 작아짐
//        transform.localScale = originalScale * pressScale;

//        yield return new WaitForSeconds(pressTime);

//        // 원래 크기로 돌아옴
//        transform.localScale = originalScale;

//        isPressed = false;
//    }
//}