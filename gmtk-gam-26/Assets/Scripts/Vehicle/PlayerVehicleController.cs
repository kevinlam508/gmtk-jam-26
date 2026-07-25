using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicleController : MonoBehaviour
{
    public static Vector3 PlayerPosition => Instance._vehicle.transform.position;

    private static PlayerVehicleController Instance;

    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private Transform _camera;

    [SerializeField] private CinemachineOrbitalFollow _orbit;
    [SerializeField] private float _cameraTurnSpeedDegrees = 30f;

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
            _vehicle.AirDash();
        }
    }
}
