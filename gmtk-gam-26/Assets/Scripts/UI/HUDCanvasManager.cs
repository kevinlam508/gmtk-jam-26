using UnityEngine;

public class HUDCanvasManager : MonoBehaviour
{
    [SerializeField] private BountyCanvas _bounty;
    [SerializeField] private SpeedometerCanvas _speedometer;
    [SerializeField] private MoneyCounterCanvas _moneyCounter;
    [SerializeField] private TimerCanvas _timer;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    [ContextMenu("MoneyCounter: Add 100")]
    public void TestMoneyAdd100()
    {
        _moneyCounter.OnMoneyChanged(100);
    }

    [ContextMenu("MoneyCounter: Add 5000")]
    public void TestMoneyAdd5000()
    {
        _moneyCounter.OnMoneyChanged(5000);
    }
}
