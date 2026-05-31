using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneTransition : MonoBehaviour
{
    [Header("Fade (VR Optimized)")]
    // VR 환경에서는 UI Image 대신 카메라 바로 앞을 막아서 가려줄 MeshRenderer를 사용합니다.
    [SerializeField] private MeshRenderer fadeQuadRenderer;
    [SerializeField] private float transitionDuration = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSource[] fadeOutSounds;
    [SerializeField] private AudioSource doorSound;
    [SerializeField] private AudioClip doorClip;

    [Header("Move Forward")]
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private Transform moveTarget;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "1.Tutorial";

    private bool isTransitioning = false;
    private Material fadeMaterial;

    private void Start()
    {
        if (fadeQuadRenderer != null)
        {
            // 런타임에 독립된 머티리얼 인스턴스를 가져옵니다.
            fadeMaterial = fadeQuadRenderer.material;

            // 시작할 때는 투명하게 설정합니다.
            Color c = fadeMaterial.color;
            c.a = 0f;
            fadeMaterial.color = c;
        }
    }

    public void StartTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        CharacterController characterController = xrOrigin.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        Vector3 startPos = xrOrigin.position;
        Vector3 targetPos = moveTarget.position;

        float[] originalVolumes = new float[fadeOutSounds.Length];
        for (int i = 0; i < fadeOutSounds.Length; i++)
            originalVolumes[i] = fadeOutSounds[i].volume;

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // 목적지로 부드럽게 이동
            xrOrigin.position = Vector3.Lerp(startPos, targetPos, smoothT);

            // [변경] VR 페이드 적용: Quad 머티리얼의 알파값 증가 (검은색으로 바꾸기)
            if (fadeMaterial != null)
            {
                Color color = fadeMaterial.color;
                color.a = smoothT;
                fadeMaterial.color = color;
            }

            // 주변 사운드 점점 작아짐
            for (int i = 0; i < fadeOutSounds.Length; i++)
            {
                fadeOutSounds[i].volume = Mathf.Lerp(originalVolumes[i], 0f, smoothT);
            }

            yield return null;
        }

        if (doorSound != null && doorClip != null)
        {
            doorSound.PlayOneShot(doorClip);
        }

        yield return new WaitForSeconds(1.5f);

        // 실제 씬 전환 실행
        //SceneManager.LoadScene(nextSceneName);
    }
}