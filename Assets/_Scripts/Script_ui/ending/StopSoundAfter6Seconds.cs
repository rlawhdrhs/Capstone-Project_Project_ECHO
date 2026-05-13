using System.Collections;
using UnityEngine;

public class StopSoundAfter6Seconds : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        StartCoroutine(StopSound());
    }

    IEnumerator StopSound()
    {
        yield return new WaitForSeconds(6f);

        audioSource.Stop();
    }
}