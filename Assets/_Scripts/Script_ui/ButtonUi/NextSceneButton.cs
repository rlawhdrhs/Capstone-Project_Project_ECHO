using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class NextSceneButton : MonoBehaviour
{
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

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnPressed);
    }

    void OnPressed(SelectEnterEventArgs args)
    {
        if (isPressed) return;
        isPressed = true;

        
        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);

    
        SendHaptic(args.interactorObject);

        StartCoroutine(ScaleAnimation());
    }

    IEnumerator ScaleAnimation()
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

        // �� �̵�
        int index = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(index + 1);
    }

    void SendHaptic(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
    {
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor input)
        {
            input.SendHapticImpulse(0.5f, 0.1f);
        }
    }

    void OnMouseDown()
    {
        if (isPressed) return;
        isPressed = true;

        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);

        StartCoroutine(ScaleAnimation());
    }

}







//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class CubeButton : MonoBehaviour
//{
//    void OnMouseDown()
//    {
//        GoToNextScene();
//    }

//    void GoToNextScene()
//    {
//        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
//        SceneManager.LoadScene(currentSceneIndex + 1);
//    }
//}