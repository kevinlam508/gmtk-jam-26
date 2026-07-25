using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _followOffset;
    [SerializeField] private GameObject _visualRoot;

    private void Update()
    {
        transform.position = _follow.position + _followOffset;

        Vector3? targetPosition = GameManager.Instance?.TargetLocation;
        _visualRoot.SetActive(targetPosition.HasValue);
        if (targetPosition.HasValue)
        {
            transform.LookAt(targetPosition.Value);
        }
    }
}
