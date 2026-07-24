using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicleController : MonoBehaviour
{
    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private Transform _camera;

    [SerializeField] private CinemachineOrbitalFollow _orbit;
    [SerializeField] private float _cameraTurnSpeedDegrees = 30f;

    private float _steer;

    private void FixedUpdate()
    {
        _orbit.HorizontalAxis.Value += _steer * _cameraTurnSpeedDegrees * Time.fixedDeltaTime;

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
}
