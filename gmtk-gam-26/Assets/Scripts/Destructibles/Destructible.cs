using UnityEngine;
using System.Collections.Generic;

public class Destructible : MonoBehaviour
{
    private static float RestoreDistance = 10f;

    [SerializeField] private GameObject intactObject;
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ParticleSystem destructionEffect;
    [SerializeField] private Rigidbody intactRigidbody;

    [SerializeField] private float breakThreshold = 8f;

    [SerializeField] private float impulseForce = 2f;

    [SerializeField] private bool isBroken;

    [SerializeField]
    private Rigidbody[] brokenPieces;
    private List<Vector3> brokenInitialPositions = new();
    private List<Quaternion> brokenInitialRotation = new();
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        Initialize();
        Reset();
    }

    private void Update()
    {
        if (isBroken 
            && Vector3.Distance(initialPosition, PlayerVehicleController.PlayerPosition) > RestoreDistance)
        {
            Reset();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBroken)
        {
            return;
        }

        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce > breakThreshold)
        {
            Break(collision.GetContact(0).point, collision.relativeVelocity);
        }

    }

    private void Initialize()
    {
        brokenInitialPositions.Clear();
        brokenInitialRotation.Clear();

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;

        brokenPieces = brokenObject.GetComponentsInChildren<Rigidbody>(true);
        
        foreach (Rigidbody piece in brokenPieces)
        {
            brokenInitialPositions.Add(piece.transform.localPosition);
            brokenInitialRotation.Add(piece.transform.localRotation);

            Debug.Log(piece.name);
        }
    }

    private void Break(Vector3 impactPoint, Vector3 impactVelocity)
    {
        isBroken = true;

        intactRigidbody.isKinematic = true;

        intactObject.gameObject.SetActive(false);
        brokenObject.gameObject.SetActive(true);
        destructionEffect.Play();

        foreach (Rigidbody piece in brokenPieces)
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
        destructionEffect.Stop();

        brokenObject.gameObject.SetActive(false);

        intactRigidbody.linearVelocity = Vector3.zero;
        intactRigidbody.angularVelocity = Vector3.zero;
        transform.SetLocalPositionAndRotation(initialPosition, initialRotation);

        for (int i=0; i<brokenPieces.Length; i++)
        {
            Rigidbody piece = brokenPieces[i];
            piece.linearVelocity = Vector3.zero;
            piece.angularVelocity = Vector3.zero;
            brokenPieces[i].transform.SetLocalPositionAndRotation(brokenInitialPositions[i], brokenInitialRotation[i]);
            piece.Sleep();
        }
        Debug.Log("Reset");
        
        intactRigidbody.isKinematic = false;
        intactObject.gameObject.SetActive(true);
        isBroken = false;
    }
}
