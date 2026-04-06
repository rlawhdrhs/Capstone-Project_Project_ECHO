using UnityEngine;

public class LaserDetector : MonoBehaviour
{
    public float laserDistance = 10f;
    public LineRenderer lineRenderer;
    public PlayerDetectable player;
    public LayerMask playerLayer;

    void Start()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
        }
    }

    void Update()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = -transform.right;
        Vector3 endPoint = origin + direction * laserDistance;

        bool isDetected = false;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, laserDistance, playerLayer))
        {
            PlayerDetectable detectedPlayer = hit.collider.GetComponent<PlayerDetectable>();
            if (detectedPlayer != null)
            {
                isDetected = true;
            }
        }

        if (player != null)
        {
            player.SetDetected(isDetected);
            player.UpdateGauge(isDetected);
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, endPoint);
        }
    }
}