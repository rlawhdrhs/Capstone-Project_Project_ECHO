using UnityEngine;

public class SoundListener : MonoBehaviour
{
    public float hearingRange = 10f;

    void Update()
    {
        if (SoundManager.Instance == null) return;

        foreach (SoundEventData sound in SoundManager.Instance.soundEvents)
        {
            float distance = Vector3.Distance(transform.position, sound.position);

            if (distance <= hearingRange)
            {
                Debug.Log(gameObject.name + " detected sound at: " + sound.position);
            }
        }
    }
}