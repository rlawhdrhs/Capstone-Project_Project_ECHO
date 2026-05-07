using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float rotateSpeed = 60f; // 숫자가 클수록 빠르게 회전

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }
}