using System;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;

    [Header("Tires")]
    [SerializeField] private float _tireTurnAngle = 30f;
    [SerializeField] private Transform[] _frontTireTransforms;
    [SerializeField] private Transform[] _backTireTransforms;

    [Header("Movement")]
    [SerializeField] private float _defaultSpeed = 2f;
    [SerializeField] private float _maxAcceleration = .5f;
    [Range(0, 1)]
    [SerializeField] private float _tractionStrength = .5f;

    [Header("Steer")]
    [Min(0)]
    [SerializeField] private float _steerTorque = .5f;
    [Min(0)]
    [SerializeField] private float _maxSteerTorque = .5f;
    [Min(0)]
    [SerializeField] private float _tarqueDamping = 5f;

    [Header("Suspension")]
    [SerializeField] private LayerMask _suspensionIgnoreLayers;
    [SerializeField] private Transform[] _tirePivots;
    [Min(0)]
    [Tooltip("Larger = Retern to spring length away from the ground faster")]
    [SerializeField] private float _springForce = 1f;
    [Min(0)]
    [Tooltip("Ideal height off the ground")]
    [SerializeField] private float _springLength = .4f;
    [Min(0)]
    [Tooltip("Higher = Stablize faster")]
    [SerializeField] private float _springDamping = .3f;

    [Header("Tilt")]
    [Range(0, 90)]
    [SerializeField] private float _maxTiltDegrees = 60f;

    public float VisualSteer { get; set; }

    public float DesiredSteer { get; set; }
    public float DesiredMagnitude { get; set; }
    public Vector3 Forward { get; set; }

    private Vector3 DesiredForward => Forward + (Vector3.Cross(Vector3.up, Forward) * DesiredSteer);

    private void FixedUpdate()
    {
        float groundedRatio = ProcessSuspension();
        ProcessMovement(Time.fixedDeltaTime, groundedRatio);
        ProcessSteer(Time.fixedDeltaTime, groundedRatio);
        ProcessTilt(Time.fixedDeltaTime);
    }

    private float ProcessSuspension()
    {
        int groundedTireCount = 0;
        Vector3 down = -_body.transform.up;
        foreach (Transform tirePoint in _tirePivots)
        {
            bool hit = Physics.Raycast(tirePoint.position, down, out RaycastHit info, _springLength, ~_suspensionIgnoreLayers);
            if (!hit)
            {
                continue;
            }

            float distanceThroughGround = _springLength - info.distance;
            float desiredForce = distanceThroughGround * _springForce;

            Vector3 existingVelocity = _body.GetPointVelocity(tirePoint.position);
            float damping = Vector3.Dot(existingVelocity, -down) * _springDamping;

            Vector3 force = (desiredForce - damping) * -down;
            _body.AddForceAtPosition(force, tirePoint.position);

            groundedTireCount++;
        }

        return 1.0f * groundedTireCount / _tirePivots.Length;
    }

    private void ProcessMovement(float timeStep, float groundedRatio)
    {
        Vector3 desiredDirection = DesiredForward;
        Vector3 currentVelocity = _body.linearVelocity;

        // Accelerate towards desired velocity
        float desiredSpeed = _defaultSpeed * DesiredMagnitude;
        float forwardSpeed = Vector3.Dot(currentVelocity, desiredDirection);
        float instantAcceleration = (desiredSpeed - forwardSpeed) / timeStep;
        float appliedForce = Mathf.Clamp(instantAcceleration, -_maxAcceleration, _maxAcceleration)
            * _body.mass;
        _body.AddForce(desiredDirection * appliedForce * groundedRatio);

        // Apply traction to side velocity
        Vector3 sideDirection = Vector3.Cross(desiredDirection, Vector3.up);
        float sideSpeed = Vector3.Dot(currentVelocity, sideDirection);
        float instantSideAcceleration = -sideSpeed / timeStep;
        float appliedSideForce = instantSideAcceleration * _tractionStrength
            * _body.mass;
        _body.AddForce(sideDirection * appliedSideForce * groundedRatio);
    }

    private void ProcessSteer(float timeStep, float groundedRatio)
    {
        Quaternion frontTireRotation =
            Mathf.Approximately(VisualSteer, 0)
            ? Quaternion.identity
            : Quaternion.Euler(0, VisualSteer * _tireTurnAngle, 0);
        foreach (Transform tire in _frontTireTransforms)
        {
            tire.localRotation = frontTireRotation;
        }
        Quaternion backTireRotation =
            Mathf.Approximately(VisualSteer, 0)
            ? Quaternion.identity
            : Quaternion.Euler(0, -VisualSteer * _tireTurnAngle, 0);
        foreach (Transform tire in _backTireTransforms)
        {
            tire.localRotation = backTireRotation;
;
        }

        if (groundedRatio < 1)
        {
            return;
        }

        Vector3 bodyForward = _body.transform.forward;
        bodyForward.y = 0;

        Vector3 steerForward = DesiredForward;
        steerForward = steerForward.normalized;
        float angle = Vector3.SignedAngle(bodyForward, steerForward, Vector3.up) * Mathf.Deg2Rad;

        float angularVelocity = Vector3.Dot(_body.angularVelocity, Vector3.up);
        float turnForce = (angle * _steerTorque) - (angularVelocity * _tarqueDamping);
        turnForce = Mathf.Clamp(turnForce, -_maxSteerTorque, _maxSteerTorque);
        _body.AddTorque(_body.transform.up * turnForce);
    }

    private void ProcessTilt(float timeStep)
    {
        Vector3 bodyUp = _body.transform.up;
        float upDot = Vector3.Dot(Vector3.up, bodyUp);
        float deviation = Vector3.Angle(Vector3.up, bodyUp);
        if (upDot > 0 && deviation < 90 - _maxTiltDegrees)
        {
            return;
        }

        float torque = Mathf.Deg2Rad 
            * (upDot > 0 ? 90 - deviation : deviation);
        Vector3 axis = Vector3.Cross(bodyUp, Vector3.up);
        _body.AddTorque(axis * torque, ForceMode.VelocityChange);
    }
}
