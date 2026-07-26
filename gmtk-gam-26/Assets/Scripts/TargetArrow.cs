using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    private enum HoverState
    {
        OnFollow,
        ToFollow,
        OnTarget,
        ToTarget
    }

    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _followOffset;
    [SerializeField] private GameObject _visualRoot;

    [Space]
    [SerializeField] private float _hoverOverTargetDistance = 10f;
    [SerializeField] private float _moveToTargetSpeed = 40f;

    private HoverState _state = HoverState.OnFollow;

    private void Update()
    {
        Vector3? targetPosition = GameManager.Instance?.TargetLocation;
        _visualRoot.SetActive(targetPosition.HasValue);
        if (!targetPosition.HasValue)
        {
            return;
        }

        bool shouldBeOverTarget = (_follow.position - targetPosition.Value).magnitude < _hoverOverTargetDistance;
        switch (_state)
        {
            case HoverState.OnFollow:
            case HoverState.ToFollow:
                if (shouldBeOverTarget)
                {
                    _state = HoverState.ToTarget;
                }
                break;
            case HoverState.OnTarget:
            case HoverState.ToTarget:
                if (!shouldBeOverTarget)
                {
                    _state = HoverState.ToFollow;
                }
                break;
        }

        Vector3 position = transform.position - _followOffset;
        switch (_state)
        {
            case HoverState.OnFollow:
                position = _follow.position;
                break;
            case HoverState.OnTarget:
                position = targetPosition.Value;
                break;
            case HoverState.ToFollow:
                Vector3 toFollow = _follow.position - position;
                Vector3 delta = toFollow.normalized * _moveToTargetSpeed * Time.deltaTime;
                if (delta.magnitude > toFollow.magnitude)
                {
                    delta = toFollow;
                    _state = HoverState.OnFollow;
                }
                position += delta;
                break;
            case HoverState.ToTarget:
                Vector3 toTarget = targetPosition.Value - position;
                Vector3 targetDelta = toTarget.normalized * _moveToTargetSpeed * Time.deltaTime;
                if (targetDelta.magnitude > toTarget.magnitude)
                {
                    targetDelta = toTarget;
                    _state = HoverState.OnTarget;
                }
                position += targetDelta;
                break;
        }

        transform.position = position + _followOffset;
        Vector3 lookAtUp = _state switch
        {
            HoverState.OnTarget => transform.position - _follow.position,
            HoverState.ToTarget => transform.position - _follow.position,
            _ => Vector3.up
        };
        transform.LookAt(targetPosition.Value, lookAtUp);
    }
}
