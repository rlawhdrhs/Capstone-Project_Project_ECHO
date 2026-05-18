using UnityEngine;

public class RandomLightSwing : MonoBehaviour
{
    [Header("Rotation Range")]
    float minX = -90f;
    float maxX = 270f;

    [Header("Movement")]
    public float smoothSpeed = 8f;

    [Header("Target Change")]
    public float changeInterval = 0.15f;

    private float targetX;
    private float timer;

    void Start()
    {
        float startX = Random.Range(minX, maxX);

        Vector3 rot = transform.eulerAngles;
        rot.x = startX;
        transform.eulerAngles = rot;

        SetNewTarget();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 짧은 간격마다 목표 각도 계속 변경
        if (timer >= changeInterval)
        {
            SetNewTarget();
            timer = 0f;
        }

        Vector3 rot = transform.eulerAngles;

        float currentX = NormalizeAngle(rot.x);

        // 빠르고 부드럽게 이동
        float smoothX = Mathf.Lerp(
            currentX,
            targetX,
            Time.deltaTime * smoothSpeed
        );

        rot.x = smoothX;
        transform.eulerAngles = rot;
    }

    void SetNewTarget()
    {
        targetX = Random.Range(minX, maxX);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}