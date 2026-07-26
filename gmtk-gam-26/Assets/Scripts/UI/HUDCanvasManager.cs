using UnityEngine;

public class HUDCanvasManager : MonoBehaviour
{
    [SerializeField] private BountyCanvas _bounty;
    [SerializeField] private SpeedometerCanvas _speedometer;
    [SerializeField] private MoneyCounterCanvas _moneyCounter;
    [SerializeField] private TimerCanvas _timer;

    [Min(1)]
    [SerializeField] private float _speedMultiplier = 6f;

    private void Start()
    {
        GameManager.Instance.ContractChanged += _bounty.SetBountyUI;
        GameManager.Instance.ContractTimerChanged += _bounty.UpdateBountyTimerUI;
        GameManager.Instance.ContractSucceded += _bounty.BountyComplete;
        GameManager.Instance.ContractFailed += _bounty.BountyFailed;
        GameManager.Instance.ContractsCompletedChanged += _bounty.OnBountyCompletCountChanged;

        GameManager.Instance.TimerChanged += _timer.UpdateGlobalTimerUI;
        GameManager.Instance.MoneyChanged += _moneyCounter.OnMoneyChanged;
    }

    private void OnDestroy()
    {
        GameManager.Instance.ContractChanged -= _bounty.SetBountyUI;
        GameManager.Instance.ContractTimerChanged += _bounty.UpdateBountyTimerUI;
        GameManager.Instance.ContractSucceded -= _bounty.BountyComplete;
        GameManager.Instance.ContractFailed -= _bounty.BountyFailed;
        GameManager.Instance.ContractsCompletedChanged -= _bounty.OnBountyCompletCountChanged;

        GameManager.Instance.TimerChanged -= _timer.UpdateGlobalTimerUI;
        GameManager.Instance.MoneyChanged -= _moneyCounter.OnMoneyChanged;
    }

    private void Update()
    {
        Vector3 velocity = PlayerVehicleController.PlayerVelocity;
        velocity.y = 0;
        float speed = velocity.magnitude;
        _speedometer.OnSpeedChanged(Mathf.RoundToInt(_speedMultiplier * speed));
    }
}
