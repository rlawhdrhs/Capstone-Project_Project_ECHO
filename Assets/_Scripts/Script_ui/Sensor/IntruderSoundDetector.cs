using UnityEngine;

public class IntruderSoundDetector : MonoBehaviour
{
    [Header("감지 범위")]
    public float detectRange = 15f;

    [Header("Intruder Layer")]
    public LayerMask intruderLayer;

    [Header("큐브 색 변경")]
    public CubeColorChanger targetCube;

    private bool intruderDetected = false;

    void Update()
    {
        DetectIntruderSound();
    }

    void DetectIntruderSound()
    {
        intruderDetected = false;

        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectRange,
            intruderLayer
        );

        foreach (Collider col in colliders)
        {
            AudioSource audioSource = col.GetComponent<AudioSource>();

            if (audioSource != null && audioSource.isPlaying)
            {
                intruderDetected = true;

                Debug.Log("Intruder 소리 감지 : " + col.name);

                break;
            }
        }

        if (targetCube != null)
        {
            if (intruderDetected)
            {
                targetCube.ChangeToDetectedColor();
            }
            else
            {
                targetCube.ChangeToNormalColor();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}