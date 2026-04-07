using UnityEngine;

public class LaserDetector : MonoBehaviour
{
    public float laserDistance = 10f;
    public LineRenderer lineRenderer;
    public PlayerDetectable player;
    public LayerMask hitMask;

    public KeyCode toggleKey  = KeyCode.Space;
    private bool isLaserOn = false;
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
        if(Input.GetKeyDown(toggleKey))
        {
            isLaserOn = !isLaserOn;

            if(lineRenderer != null)
            {
                lineRenderer.enabled = isLaserOn;
            }
        }
        if (!isLaserOn)
        {
            if(player != null)
            {
                player.SetDetected(false);
                player.UpdateGauge(false);
            }
            return;
        }
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = -transform.right;
        Vector3 endPoint = origin + direction * laserDistance;

        bool isDetected = false;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, laserDistance, hitMask))
        {
            endPoint = hit.point;
            
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