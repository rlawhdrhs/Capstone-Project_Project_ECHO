using UnityEngine;

public class PressButton : MonoBehaviour
{
    public WakeUpTransition wakeUpTransition;

    void Start()
    {
        if (wakeUpTransition != null)
        {
            wakeUpTransition.PlayWakeUp(null);
        }
    }
}