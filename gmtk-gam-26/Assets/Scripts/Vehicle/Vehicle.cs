using Barmetler.RoadSystem.Util;
using System;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [SerializeField] private Rigidbody _body;
    [SerializeField] private Transform _visualRoot;

    [Header("Tires")]
    [SerializeField] private float _tireTurnAngle = 30f;
    [SerializeField] private Transform[] _frontTireTransforms;
    [SerializeField] private Transform[] _backTireTransforms;
    [Min(.001f)]
    [Tooltip("For determining spin speed from velocity. Bigger = slower spin")]
    [SerializeField] private float _tireRadius;
    [SerializeField] private Transform[] _tireVisuals;

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

    [Header("Steer Visuals")]
    [Min(0)]
    [SerializeField] private float _visualTurnBonusAngle = 0f;
    [Min(1)]
    [SerializeField] private float _visualTurnEnterMultiplier = 1f;
    [Min(1)]
    [SerializeField] private float _visualTurnRestorationMultiplier = 1f;

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
    [Range(0, 1)]
    [SerializeField] private float _tiltCorrectionTolerance = .1f;
    [Min(0)]
    [SerializeField] private float _tiltCorrectionTorque = 5f;
    [Min(0)]
    [SerializeField] private float _tiltCheckDistance = .5f;
    [SerializeField] private BoxCollider _mainBoxCollider;

    [Header("Aerial")]
    [Min(0)]
    [SerializeField] private float _jumpSpeed = 10f;
    [Min(0)]
    [SerializeField] private int _maxJumpCount = 2;
    [Min(0)]
    [SerializeField] private float _airDashSpeed = 10f;
    [Min(0)]
    [SerializeField] private int _maxAirDashCount = 1;

    public float VisualSteer { get; set; }
    public float DesiredMagnitude { get; set; }
    public Vector3 Forward { get; set; }

    public bool IsGrounded { get; private set; }
    private int _jumpCount = 0;
    private int _airDashCount = 0;

    private void FixedUpdate()
    {
        Debug.DrawRay(_body.position, _body.transform.forward * 5, Color.snow, -1);
        Debug.DrawRay(_body.position, Forward * 5, Color.black, -1);
        Debug.DrawRay(_body.position, _body.linearVelocity.normalized * 5, Color.green, -1);

        float groundedRatio = ProcessSuspension();
        ProcessMovement(Time.fixedDeltaTime, groundedRatio);
        ProcessSteer(Time.fixedDeltaTime, groundedRatio);
        ProcessTilt(Time.fixedDeltaTime);
    }

    private float ProcessSuspension()
    {
        int groundedTireCount = 0;
        Vector3 down = -_body.transform.up;
        for (int i = 0; i < _tirePivots.Length; i++)
        {
            Transform tirePoint = _tirePivots[i];

            // Place s.t. visual root is touching the ground. Actual tire should be above the root
            Transform tireVisual = i < _frontTireTransforms.Length
                ? _frontTireTransforms[i] : _backTireTransforms[i - _frontTireTransforms.Length];

            bool hit = Physics.Raycast(tirePoint.position, down, out RaycastHit info, _springLength, ~_suspensionIgnoreLayers);
            if (!hit)
            {
                tireVisual.localPosition = tirePoint.localPosition - Vector3.up * _springLength;
                continue;
            }

            tireVisual.localPosition = tirePoint.localPosition - Vector3.up * info.distance;

            float distanceThroughGround = _springLength - info.distance;
            float desiredForce = distanceThroughGround * _springForce;

            Vector3 existingVelocity = _body.GetPointVelocity(tirePoint.position);
            float damping = Vector3.Dot(existingVelocity, -down) * _springDamping;

            Vector3 force = (desiredForce - damping) * -down;
            _body.AddForceAtPosition(force, tirePoint.position);

            groundedTireCount++;
        }

        IsGrounded = groundedTireCount == _tirePivots.Length;
        if (IsGrounded)
        {
            _jumpCount = 0;
            _airDashCount = 0;
        }

        return 1.0f * groundedTireCount / _tirePivots.Length;
    }

    private void ProcessMovement(float timeStep, float groundedRatio)
    {
        Vector3 bodyForward = _body.transform.forward;
        Vector3 driveDirection = bodyForward;
        driveDirection = driveDirection.normalized;
        Vector3 currentVelocity = _body.linearVelocity;

        // Accelerate towards desired velocity
        float desiredSpeed = _defaultSpeed * DesiredMagnitude;
        float forwardSpeed = Vector3.Dot(currentVelocity, driveDirection);
        float instantAcceleration = (desiredSpeed - forwardSpeed) / timeStep;
        float appliedForce = Mathf.Clamp(instantAcceleration, -_maxAcceleration, _maxAcceleration)
            * _body.mass;
        _body.AddForce(driveDirection * appliedForce * groundedRatio);
        Debug.DrawRay(_body.position, driveDirection * 5 * Mathf.Sign(appliedForce), Color.red, -1);

        // Apply traction to side velocity
        Vector3 sideDirection = Vector3.Cross(Vector3.up, driveDirection);
        float sideSpeed = Vector3.Dot(currentVelocity, sideDirection);
        float instantSideAcceleration = -sideSpeed / timeStep;
        float appliedSideForce = instantSideAcceleration * _tractionStrength
            * _body.mass;
        _body.AddForce(sideDirection * appliedSideForce * groundedRatio);
        Debug.DrawRay(_body.position, sideDirection * 5 * Mathf.Sign(appliedSideForce), Color.blue, -1);

        if (IsGrounded)
        {
            float tireAngularChange = forwardSpeed / _tireRadius * timeStep;
            Quaternion tireRotation = Quaternion.Euler(tireAngularChange * Mathf.Rad2Deg, 0, 0);
            foreach (Transform tireVisual in _tireVisuals)
            {
                tireVisual.localRotation *= tireRotation;
            }
        }
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

        Quaternion rootRotation = Quaternion.Euler(0, VisualSteer * _visualTurnBonusAngle, 0);
        float rootRotationMultiplier = Mathf.Approximately(VisualSteer, 0)
            ? _visualTurnRestorationMultiplier : _visualTurnEnterMultiplier;
        _visualRoot.localRotation = Quaternion.Lerp(_visualRoot.localRotation, rootRotation, timeStep * rootRotationMultiplier);

        if (groundedRatio < 1)
        {
            return;
        }

        Vector3 bodyForward = _body.transform.forward;
        bodyForward.y = 0;

        Vector3 steerForward = Forward;
        steerForward = steerForward.normalized;
        float angle = Vector3.SignedAngle(bodyForward, steerForward, Vector3.up) * Mathf.Deg2Rad;

        float angularVelocity = Vector3.Dot(_body.angularVelocity, Vector3.up);
        float turnForce = (angle * _steerTorque) - (angularVelocity * _tarqueDamping);
        turnForce = Mathf.Clamp(turnForce, -_maxSteerTorque, _maxSteerTorque);
        _body.AddTorque(_body.transform.up * turnForce);
    }

    private void ProcessTilt(float timeStep)
    {
        float upDot = Vector3.Dot(Vector3.up, _body.transform.up);
        if (Mathf.Abs(upDot) > _tiltCorrectionTolerance)
        {
            return;
        }

        bool onSide = HasSideContact(_mainBoxCollider.center, _body.transform.right, _mainBoxCollider.size.x)
            || HasSideContact(_mainBoxCollider.center, -_body.transform.right, _mainBoxCollider.size.x)
            || HasSideContact(_mainBoxCollider.center, _body.transform.up, _mainBoxCollider.size.z)
            || HasSideContact(_mainBoxCollider.center, -_body.transform.up, _mainBoxCollider.size.z);
        if (!onSide)
        {
            return;
        }

        _body.AddTorque(_tiltCorrectionTorque * Vector3.Cross(_body.transform.up, Vector3.up));

        bool HasSideContact(Vector3 position, Vector3 direction, float bonusDistance)
        {
            return Physics.Raycast(_mainBoxCollider.transform.position + position, direction, _tiltCheckDistance + bonusDistance, ~_suspensionIgnoreLayers);
        }
    }

    public void AirDash()
    {
        if (IsGrounded)
        {
            return;
        }

        // Don't know what direction to dash
        if (Mathf.Approximately(DesiredMagnitude, 0) && Mathf.Approximately(VisualSteer, 0))
        {
            return;
        }

        if (_airDashCount < _maxAirDashCount)
        {
            Vector3 direction = new Vector3(VisualSteer, 0, DesiredMagnitude);
            direction = _body.transform.rotation * direction;
            direction = direction.normalized;
            float existingSpeed = Vector3.Dot(_body.linearVelocity, direction);
            _body.AddForce(direction * (_airDashSpeed - existingSpeed), ForceMode.VelocityChange);
            _airDashCount++;
        }
    }

    public void Jump()
    {
        if (IsGrounded || _jumpCount < _maxJumpCount)
        {
            float existingVertical = Vector3.Dot(_body.linearVelocity, Vector3.up);

            _body.AddForce((_jumpSpeed - existingVertical) * Vector3.up, ForceMode.VelocityChange);
            _jumpCount++;
        }
    }
}
