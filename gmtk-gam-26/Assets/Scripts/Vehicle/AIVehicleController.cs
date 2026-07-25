using System;
using System.Collections.Generic;
using Barmetler;
using Barmetler.RoadSystem;
using UnityEngine;

public class AIVehicleController : MonoBehaviour
{
    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private RoadSystemNavigator _navigator;
    [SerializeField] private float _targetDistanceFromGoal = 5f;
    [SerializeField] private float _targetDistanceFromSubgoal = 1f;

    private GameObject _playerCar = null;
    private Transform _targetWaypoint = null;
    private List<Bezier.OrientedPoint> _orientedPoints;
    private int _pathIndex;
    
    private void Start()
    {
        _navigator.currentRoadSystem = FindAnyObjectByType<RoadSystem>();
        if (_navigator.currentRoadSystem == null)
        {
            Debug.Log("[AIVehicleController] No Road System found in the scene!");
        }
        SetNewGoal();
    }

    private void FixedUpdate()
    {
        Vector3 vectorToGoal = _navigator.Goal - _vehicle.transform.position;
        float targetDistanceSqr = _targetDistanceFromGoal * _targetDistanceFromGoal;
        if (vectorToGoal.sqrMagnitude <= targetDistanceSqr)
        {
            SetNewGoal();
        }
        else
        {
            if (_navigator.currentRoadSystem != null)
            {
                Vector3 posXZ = _vehicle.transform.position;
                posXZ.y = 0f;
                Vector3 nextPoint = _orientedPoints[_pathIndex].position;
                Vector3 nextPointXZ = nextPoint;
                nextPointXZ.y = 0f;
                Vector3 vectorToNextPoint = nextPointXZ - posXZ;
                float subtargetDistSqr = _targetDistanceFromSubgoal * _targetDistanceFromSubgoal;
                if (vectorToNextPoint.sqrMagnitude < subtargetDistSqr)
                {
                    _pathIndex++;
                }
                _vehicle.Forward = vectorToNextPoint.normalized;
                _vehicle.DesiredMagnitude = 1f;
                _vehicle.VisualSteer = vectorToNextPoint.x;
            }
            else
            {
                _vehicle.Forward = vectorToGoal.normalized;
                _vehicle.DesiredMagnitude = 1f;
                _vehicle.VisualSteer = vectorToGoal.x;
            }
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Set Current Point as Spawn Point")]
    public void SetCurrentPositionAsSpawnPoint()
    {
        _targetWaypoint = _vehicle.transform;
    }
#endif

    public void OnSpawned(Transform spawnPoint)
    {
        _targetWaypoint = spawnPoint;
    }

    private void SetNewGoal()
    {
        _targetWaypoint = GameManager.Instance.GetRandomWaypointExcept(_targetWaypoint);
        _navigator.Goal = _targetWaypoint.position;
        if (_navigator.currentRoadSystem != null)
        {
            _navigator.CalculateWayPointsSync();
            _orientedPoints = _navigator.CurrentPoints;
            _pathIndex = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _playerCar = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == _playerCar)
        {
            _playerCar = null;
        }
    }
}
