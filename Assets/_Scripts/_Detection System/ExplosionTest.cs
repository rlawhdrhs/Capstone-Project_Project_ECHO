using UnityEngine;

public class ExplosionTestTrigger : MonoBehaviour
{
    [Header("Explosion Test")]
    public KeyCode explosionKey = KeyCode.B;
    public SoundType explosionSoundType = SoundType.Explosion;
    public float explosionLifetime = 1.0f;

    [Header("Visual Debug")]
    public bool drawDebugSphere = true;
    public float debugSphereRadius = 1.0f;

    private void Update()
    {
        if (Input.GetKeyDown(explosionKey))
        {
            TriggerExplosion();
        }
    }

    public void TriggerExplosion()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("SoundManager.Instance가 없습니다.");
            return;
        }

        SoundManager.Instance.EmitSound(
            transform.position,
            explosionLifetime,
            explosionSoundType
        );

        Debug.Log($"[ExplosionTest] Explosion emitted at {transform.position}");
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugSphere)
            return;

        Gizmos.DrawWireSphere(transform.position, debugSphereRadius);
    }
}