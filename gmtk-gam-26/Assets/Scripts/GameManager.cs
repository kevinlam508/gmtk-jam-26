using System;
using System.Collections.Generic;
using Barmetler.RoadSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public event Action<float, float> TimerChanged;
    public event Action<float, float> ContractTimerChanged;
    public event Action<int> MoneyChanged;

    [Header("Starting Values")] 
    [SerializeField] private float _totalTime;
    [SerializeField] private float _contractTime;
    [SerializeField] private int _numStartingCars;
    [SerializeField] private int _numRequiredContracts;
    [SerializeField] private int _contractReward;

    [SerializeField] private GameObject _aiCarPrefab;
    [SerializeField] private List<Transform> _waypoints;
    
    public static GameManager Instance => _instance;
    private static GameManager _instance = null;

    private List<AIVehicleController> _activeVehicles;
    private AIVehicleController _contractTarget;

    private float _timer;
    public float Timer 
    {
        get => _timer;
        private set
        {
            _timer = value;
            TimerChanged?.Invoke(_timer, _totalTime);
        }
    }

    private float _contractTimer;
    public float ContractTimer
    {
        get => _contractTimer;
        private set
        {
            _contractTimer = value;
            TimerChanged?.Invoke(_contractTimer, _contractTime);
        }
    }

    private int _money;
    public int Money 
    {
        get => _money;
        private set
        {
            _money = value;
            MoneyChanged?.Invoke(_money);
        }
    }
    public int ContractsCompleted { get; private set; }


    public Vector3? TargetLocation => _contractTarget?.Location;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }
    
    private void Start()
    {
        _activeVehicles = new();
        Timer = _totalTime;
        ContractTimer = _contractTime;
        Money = 0;

        for (int i = 0; i < _numStartingCars - 1; i++)
        {
            SpawnNewVehicle();
        }

        SpawnNewContractTarget();
    }

    private void Update()
    {
        Timer -= Time.deltaTime;
        ContractTimer -= Time.deltaTime;
        if (Timer <= 0)
        {
            EndGame(false);
        }
        else if (ContractTimer <= 0)
        {
            _activeVehicles.Remove(_contractTarget);
            Destroy(_contractTarget.gameObject);
            SpawnNewContractTarget();
        }
    }

    public void OnAIVehicleCaptured(AIVehicleController aiCar)
    {
        if (aiCar == _contractTarget)
        {
            Money += _contractReward;
            ContractsCompleted++;
            Destroy(aiCar.gameObject);

            if (ContractsCompleted == _numRequiredContracts)
            {
                EndGame(true);
            }
            else
            {
                SpawnNewContractTarget();
            }
        }
        else
        {
            SpawnNewVehicle();
        }
    }

    public Transform GetRandomWaypointExcept(Transform pointToExclude)
    {
        List<Transform> waypointsCopy = new List<Transform>(_waypoints);
        waypointsCopy.Remove(pointToExclude);
        return waypointsCopy[Random.Range(0, waypointsCopy.Count)];
    }

    private AIVehicleController SpawnNewVehicle()
    {
        Transform spawnPoint = _waypoints[Random.Range(0, _waypoints.Count)];
        GameObject newCar = Instantiate(_aiCarPrefab, spawnPoint.position, spawnPoint.rotation);
        AIVehicleController aiCar = newCar.GetComponent<AIVehicleController>();
        if (aiCar == null)
        {
            Debug.LogError("[GameManager] AI Car Prefab is missing AIVehicleController component!");
            return null;
        }
            
        aiCar.OnSpawned(spawnPoint);
        _activeVehicles.Add(aiCar);
        return aiCar;
    }

    private void SpawnNewContractTarget()
    {
        _contractTarget = SpawnNewVehicle();
        ContractTimer = _contractTime;
    }

    private void EndGame(bool won)
    {
        
    }
}
