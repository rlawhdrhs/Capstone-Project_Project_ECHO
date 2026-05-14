using System.Collections;
using UnityEngine;

public class AutoExplosionChildren : MonoBehaviour
{
    public float explosionForce = 15f;
    public float upwardForce = 5f;
    public float randomTorque = 10f;

    void Start()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();

            if (rb == null)
                rb = child.gameObject.AddComponent<Rigidbody>();

            Collider col = child.GetComponent<Collider>();

            if (col == null)
                child.gameObject.AddComponent<BoxCollider>();

            Vector3 dir =
                (child.position - transform.position).normalized;

            dir += Random.insideUnitSphere * 0.3f;

            rb.linearVelocity =
                dir * explosionForce +
                Vector3.up * upwardForce;

            rb.angularVelocity =
                Random.insideUnitSphere * randomTorque;
        }
    }
}