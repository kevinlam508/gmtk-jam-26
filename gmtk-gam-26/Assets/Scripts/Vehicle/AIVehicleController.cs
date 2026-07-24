using System;
using Barmetler;
using Barmetler.RoadSystem;
using UnityEngine;

public class AIVehicleController : MonoBehaviour
{
    [SerializeField] private Vehicle _vehicle;
    [SerializeField] private RoadSystemNavigator _navigator;

    private GameObject _playerCar = null;
    private Transform _targetWaypoint = null;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _navigator.currentRoadSystem = FindAnyObjectByType<RoadSystem>();
        if (_navigator.currentRoadSystem == null)
        {
            Debug.LogError("[AIVehicleController] Road System is missing from the scene!");
        }
        SetNewGoal();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
#if UNITY_EDITOR
    [ContextMenu("Set Current Point as Spawn Point")]
    public void SetCurrentPositionAsSpawnPoint()
    {
        _targetWaypoint = gameObject.transform;
    }
#endif

    public void OnSpawned(Transform spawnPoint)
    {
        _targetWaypoint = spawnPoint;
    }

    private void SetNewGoal()
    {
        _targetWaypoint = GameManager.Instance.GetRandomWaypointExcept(_targetWaypoint);
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
