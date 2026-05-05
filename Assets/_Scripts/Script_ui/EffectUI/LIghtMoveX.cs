using UnityEngine;

public class LightMoveX : MonoBehaviour
{
    public float minX = -200f;
    public float maxX = 50f;
    public float speed = 10f;

    private bool movingRight = true;

    void Update()
    {
        Vector3 pos = transform.position;

        if (movingRight)
        {
            pos.x += speed * Time.deltaTime;

            if (pos.x >= maxX)
                movingRight = false;
        }
        else
        {
            pos.x -= speed * Time.deltaTime;

            if (pos.x <= minX)
                movingRight = true;
        }

        transform.position = pos;
    }
}