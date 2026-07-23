using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVehicleController : MonoBehaviour
{
    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private Camera _camera;

    private void FixedUpdate()
    {
        Vector3 forward = _camera.transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        _vehicle.Forward = forward;
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        Vector2 acceleration = context.ReadValue<Vector2>();
        _vehicle.DesiredMagnitude = acceleration.y;
        _vehicle.DesiredSteer = acceleration.x;
    }
}
