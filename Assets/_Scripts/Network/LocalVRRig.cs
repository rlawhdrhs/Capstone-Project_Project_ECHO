using UnityEngine;

public class LocalVRRig : MonoBehaviour
{
    public static LocalVRRig Instance;

    public Transform localHead;
    public Transform localLeftHand;
    public Transform localRightHand;

    private void Awake()
    {
        Instance = this;
    }
}