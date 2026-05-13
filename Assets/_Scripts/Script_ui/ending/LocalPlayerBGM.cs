
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LocalPlayerBGM : MonoBehaviour
{
    public bool isLocalPlayerBGM = true;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (isLocalPlayerBGM)
            audioSource.Play();
        else
            audioSource.Stop();
    }
}

//using Unity.Netcode;
//using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
//public class LocalPlayerBGM : NetworkBehaviour
//{
//    private AudioSource audioSource;

//    void Start()
//    {
//        audioSource = GetComponent<AudioSource>();

//        if (IsOwner)
//        {
//            audioSource.Play();
//        }
//        else
//        {
//            audioSource.Stop();
//        }
//    }
//}