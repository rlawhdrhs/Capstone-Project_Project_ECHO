using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LocalIntruderBGM_Normal : MonoBehaviour
{
    [Header("Layer")]
    public string intruderLayerName = "intruder";

    [Header("Delay")]
    public float playDelay = 6f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();

        if (gameObject.layer != LayerMask.NameToLayer(intruderLayerName))
        {
            audioSource.enabled = false;
            return;
        }

        StartCoroutine(PlayAfterDelay());
    }

    IEnumerator PlayAfterDelay()
    {
        yield return new WaitForSeconds(playDelay);

        audioSource.enabled = true;
        audioSource.Play();
    }
}

//ngo버전...

//using System.Collections;
//using UnityEngine;
//using Unity.Netcode;

//[RequireComponent(typeof(AudioSource))]
//public class LocalIntruderBGM : NetworkBehaviour
//{
//    [Header("Role Layer")]
//    public string intruderLayerName = "intruder";

//    [Header("Delay")]
//    public float playDelay = 6f;

//    private AudioSource audioSource;

//    public override void OnNetworkSpawn()
//    {
//        audioSource = GetComponent<AudioSource>();
//        audioSource.Stop();

//        // 내 로컬 플레이어가 아니면 소리 절대 안 남
//        if (!IsOwner)
//        {
//            audioSource.enabled = false;
//            return;
//        }

//        // 내 오브젝트가 intruder 레이어가 아니면 소리 안 남
//        if (gameObject.layer != LayerMask.NameToLayer(intruderLayerName))
//        {
//            audioSource.enabled = false;
//            return;
//        }

//        StartCoroutine(PlayAfterDelay());
//    }

//    private IEnumerator PlayAfterDelay()
//    {
//        yield return new WaitForSeconds(playDelay);

//        audioSource.enabled = true;
//        audioSource.Play();
//    }
//}