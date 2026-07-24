using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ParticleSystem destructionEffect;
    [SerializeField] private Collider intactObjectCollider;

    [SerializeField] private float breakThreshold = 8f;

    [SerializeField] private float impulseForce = 2f;

    [SerializeField] private bool isBroken;

    void Start()
    {
        Reset();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Entered"); 
        if (isBroken)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;

        Debug.Log(impactForce);

        if (impactForce > breakThreshold)
        {
            Break(collision.GetContact(0).point, collision.relativeVelocity);
        }

    }

    private void Break(Vector3 impactPoint, Vector3 impactVelocity)
    {
        intactObject.gameObject.SetActive(false);
        brokenObject.gameObject.SetActive(true);
        // destructionEffect.transform.position = impactPoint;
        destructionEffect.Play();
        intactObjectCollider.enabled = false;

        foreach (Rigidbody piece in brokenObject.GetComponentsInChildren<Rigidbody>())
        {
            Vector3 direction = (piece.worldCenterOfMass - impactPoint).normalized;

            Vector3 force =
                direction * impulseForce + impactVelocity.normalized * impulseForce;

            piece.AddForce(force, ForceMode.Impulse);
            piece.AddTorque(Random.insideUnitSphere * impulseForce,
                ForceMode.Impulse);
        }
    }

    private void Reset()
    {
        intactObject.gameObject.SetActive(true);
        brokenObject.gameObject.SetActive(false);
        isBroken = false;
        intactObjectCollider.enabled = true;
    }
}
