using UnityEngine;

public class SoundWave : MonoBehaviour
{
    public float expandSpeed = 5f;
    public float lifeTime = 1f;

    private float timer;

    void Update()
    {
        // 점점 커짐
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}