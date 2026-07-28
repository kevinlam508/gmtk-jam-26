using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicleController : MonoBehaviour
{
    public static Vector3 PlayerPosition => Instance._vehicle.transform.position;
    public static Vector3 PlayerVelocity => Instance._vehicle.Velocity;
    public static float PlayerTopSpeed => Instance._vehicle.TopSpeed;

    private static PlayerVehicleController Instance;

    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private Transform _camera;

    [SerializeField] private CinemachineOrbitalFollow _orbit;
    [SerializeField] private float _cameraTurnSpeedDegrees = 30f;

    [Header("Juice")]
    [SerializeField, Range(0, 1)] private float _speedRatioMidLimit;
    [SerializeField, Range(0, 1)] private float _speedRatioMaxLimit;
    [SerializeField, Range(0, 1)] private float _speedRatioMegaMaxLimit;
    [SerializeField] private CinemachineCamera _cinemachineCam;
    [SerializeField] private float _cameraMinFOV = 60f;
    [SerializeField] private float _cameraMidFOV = 70f;
    [SerializeField] private float _cameraMaxFOV = 80f;
    [SerializeField] private float _cameraMegaMaxFOV = 90f;
    [SerializeField, Range(0, 1)] private float _cameraStepFOV = 0.5f;
    [SerializeField] private ParticleSystem _speedLines;
    [SerializeField] private TrailRenderer _brakeLightTrail_L;
    [SerializeField] private TrailRenderer _brakeLightTrail_R;

    private float _steer;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void FixedUpdate()
    {
        _orbit.HorizontalAxis.Value += Mathf.Sign(_vehicle.DesiredMagnitude)
            * _steer * _cameraTurnSpeedDegrees * Time.fixedDeltaTime;

        Vector3 forward = _camera.transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        _vehicle.Forward = forward;
        UpdateFOV();
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        Vector2 acceleration = context.ReadValue<Vector2>();
        _vehicle.DesiredMagnitude = acceleration.y;
        _steer = acceleration.x;

        _vehicle.VisualSteer = acceleration.x;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _vehicle.Jump();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Vector3 forward = _cinemachineCam.transform.forward;
            forward.y = 0;
            _vehicle.AirDash(forward.normalized);
        }
    }

    private void UpdateFOV()
    {
        Vector3 velocity = PlayerVelocity;
        velocity.y = 0f;

        float _currentSpeedRatio = velocity.magnitude/PlayerTopSpeed;

        float _currentFOV = _cinemachineCam.Lens.FieldOfView;
        float _desiredFOV;

        if (_currentSpeedRatio > _speedRatioMegaMaxLimit)
        {
            _desiredFOV = _cameraMegaMaxFOV;
            JuiceEffects(true, true);
        }
        else if (_currentSpeedRatio > _speedRatioMaxLimit)
        {
            _desiredFOV = _cameraMaxFOV;
            JuiceEffects(true, true);
        }
        else if (_currentSpeedRatio > _speedRatioMidLimit)
        {
            _desiredFOV = _cameraMidFOV;
            JuiceEffects(true, false);
        }
        else
        {
            _desiredFOV = _cameraMinFOV;
            JuiceEffects(false, false);
        }

        _cinemachineCam.Lens.FieldOfView = Mathf.Lerp(_currentFOV, _desiredFOV, Mathf.Min(_currentSpeedRatio, _cameraStepFOV));
    }

    private void JuiceEffects(bool activateTrails, bool activateSpeedLines)
    {
        if (activateTrails)
        {
            _brakeLightTrail_L.emitting = true;
            _brakeLightTrail_R.emitting = true;
        }
        else
        {
            _brakeLightTrail_L.emitting = false;
            _brakeLightTrail_R.emitting = false;
        }

        if (activateSpeedLines)
        {
            _speedLines.Play();
        }
        else
        {
            _speedLines.Stop();
        }
    }
}
