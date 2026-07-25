using System;
using System.Collections.Generic;
using Barmetler.RoadSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private Action<float, float> _timerChanged;
    private Action<float, float> _contractTimerChanged;
    private Action<int> _moneyChanged;
    private Action<ContractProfileData> _contractChanged;

    public event Action<float, float> TimerChanged
    {
        add
        {
            _timerChanged += value;
            value.Invoke(_timer, _totalTime);
        }
        remove => _timerChanged -= value;
    }
    public event Action<float, float> ContractTimerChanged
    {
        add
        {
            _contractTimerChanged += value;
            value.Invoke(_contractTimer, _contractTime);
        }
        remove => _contractTimerChanged -= value;
    }
    public event Action<int> MoneyChanged
    {
        add
        {
            _moneyChanged += value;
            value.Invoke(_money);
        }
        remove => _moneyChanged -= value;
    }
    public event Action<ContractProfileData> ContractChanged
    {
        add
        {
            _contractChanged += value;
            if (_currentContract >= 0)
            {
                value.Invoke(_contractPool[_currentContract]);
            }
        }
        remove => _contractChanged -= value;
    }

    public event Action ContractSucceded;
    public event Action ContractFailed;

    [Header("Starting Values")] 
    [SerializeField] private float _totalTime;
    [SerializeField] private float _contractTime;
    [SerializeField] private int _numStartingCars;
    [SerializeField] private int _numRequiredContracts;

    [SerializeField] private ContractProfileData[] _contractProfiles;

    [SerializeField] private GameObject _aiCarPrefab;
    [SerializeField] private List<Transform> _waypoints;
    
    public static GameManager Instance => _instance;
    private static GameManager _instance = null;

    private List<AIVehicleController> _activeVehicles;
    private AIVehicleController _contractTarget;
    private List<ContractProfileData> _contractPool = new List<ContractProfileData>();
    private int _currentContract = -1;

    private float _timer;
    public float Timer 
    {
        get => _timer;
        private set
        {
            _timer = value;
            _timerChanged?.Invoke(_timer, _totalTime);
        }
    }

    private float _contractTimer;
    public float ContractTimer
    {
        get => _contractTimer;
        private set
        {
            _contractTimer = value;
            _contractTimerChanged?.Invoke(_contractTimer, _contractTime);
        }
    }

    private int _money;
    public int Money 
    {
        get => _money;
        private set
        {
            _money = value;
            _moneyChanged?.Invoke(_money);
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

        _contractPool.AddRange(_contractProfiles);
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
            ContractFailed?.Invoke();
            SpawnNewContractTarget();
        }
    }

    public void OnAIVehicleCaptured(AIVehicleController aiCar)
    {
        if (aiCar == _contractTarget)
        {
            Money += _contractPool[_currentContract].Bounty;
            ContractsCompleted++;

            _contractPool.RemoveAt(_currentContract);
            if (_contractPool.Count == 0)
            {
                // Just in case there are less than the required amount
                _contractPool.AddRange(_contractProfiles);
            }
            ContractSucceded?.Invoke();

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

        _currentContract = Random.Range(0, _contractPool.Count - 1);
        _contractChanged?.Invoke(_contractPool[_currentContract]);
    }

    private void EndGame(bool won)
    {
        
    }
}
