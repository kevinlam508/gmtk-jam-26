using System;
using System.Collections.Generic;
using Barmetler.RoadSystem;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private Action<float, float> _timerChanged;
    private Action<float, float> _contractTimerChanged;
    private Action<int> _moneyChanged;
    private Action<ContractProfileData> _contractChanged;
    private Action<int, int> _contractsCompletedChanged;
    public GameObject contractCompleteVFX;

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
    public event Action<int, int> ContractsCompletedChanged
    {

        add
        {
            _contractsCompletedChanged += value;
            if (_currentContract >= 0)
            {
                value.Invoke(ContractsCompleted, _numRequiredContracts);
            }
        }
        remove => _contractsCompletedChanged -= value;
    }

    public event Action ContractSucceded;
    public event Action ContractFailed;

    [SerializeField] private ScreenSwitcher _switcher;

    [Header("Starting Values")] 
    [SerializeField] private float _totalTime;
    [SerializeField] private float _contractTime;
    [SerializeField] private int _numStartingCars;
    [SerializeField] private int _numRequiredContracts;

    [SerializeField] private ContractProfileData[] _contractProfiles;

    [SerializeField] private GameObject _aiCarPrefab;
    [SerializeField] private List<Transform> _waypoints;

    [Space]
    [SerializeField] private float _roadWaypointOffset = 1f;

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

    private int _contractsCompleted;
    public int ContractsCompleted 
    {
        get => _contractsCompleted;
        private set
        {
            _contractsCompleted = value;
            _contractsCompletedChanged?.Invoke(value, _numRequiredContracts);
        }
    }


    public Vector3? TargetLocation => _contractTarget?.Location;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        _contractPool.AddRange(_contractProfiles);

        HashSet<RoadAnchor> roadAnchors = new HashSet<RoadAnchor>();
        RoadSystem road = FindAnyObjectByType<RoadSystem>();
        foreach (Road r in road.Roads)
        {
            roadAnchors.Add(r.start);
            roadAnchors.Add(r.end);
        }
        foreach (RoadAnchor anchor in roadAnchors)
        {
            GameObject waypoint = new GameObject();
            waypoint.name = "Waypoint";
            waypoint.transform.SetParent(transform);

            waypoint.transform.position = anchor.transform.position
                + _roadWaypointOffset * Vector3.up;
            _waypoints.Add(waypoint.transform);
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
            SpawnNewVehicle(true);
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
            SpawnNewVehicle(true);
        }
        Vector3 loc = aiCar.Location;
        Instantiate(contractCompleteVFX, loc, Quaternion.identity);
        Destroy(aiCar.gameObject);
    }

    public Transform GetRandomWaypointExcept(Transform pointToExclude)
    {
        List<Transform> waypointsCopy = new List<Transform>(_waypoints);
        waypointsCopy.Remove(pointToExclude);
        return waypointsCopy[Random.Range(0, waypointsCopy.Count)];
    }

    private AIVehicleController SpawnNewVehicle(bool removeHook)
    {
        Transform spawnPoint = _waypoints[Random.Range(0, _waypoints.Count)];
        GameObject newCar = Instantiate(_aiCarPrefab, spawnPoint.position, spawnPoint.rotation);
        AIVehicleController aiCar = newCar.GetComponent<AIVehicleController>();
        if (aiCar == null)
        {
            Debug.LogError("[GameManager] AI Car Prefab is missing AIVehicleController component!");
            return null;
        }
        
        if (removeHook)
        {
            aiCar.RemoveHookTarget();
        }

        aiCar.OnSpawned(spawnPoint);
        _activeVehicles.Add(aiCar);
        return aiCar;
    }

    private void SpawnNewContractTarget()
    {
        _contractTarget = SpawnNewVehicle(false);
        ContractTimer = _contractTime;

        _currentContract = Random.Range(0, _contractPool.Count);
        
        // sorry kevin i just want it to delay a little bit between contracts
        DOVirtual.DelayedCall(1.5f,()=>
                _contractChanged?.Invoke(_contractPool[_currentContract])
        );
    }

    private void EndGame(bool won)
    {
        EndScreen.Won = won;
        EndScreen.MoneyCollected = _money;
        EndScreen.ContractsCaught = ContractsCompleted;
        EndScreen.TimeLeft = _timer;

        _switcher.Switch();
    }
}
