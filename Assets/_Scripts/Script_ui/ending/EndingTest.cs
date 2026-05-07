using UnityEngine;

public class EndingTest : MonoBehaviour
{
    public EndingType endingType;

    void Start()
    {
        Debug.Log("현재 엔딩: " + endingType);
    }
}