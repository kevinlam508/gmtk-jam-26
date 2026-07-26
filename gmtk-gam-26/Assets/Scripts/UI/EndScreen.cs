using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class EndScreen : MonoBehaviour
{
    public static bool Won;
    public static int MoneyCollected;
    public static int ContractsCaught;
    public static float TimeLeft;

    [SerializeField] private TMP_Text _wonLabel;
    [SerializeField] private TMP_Text _moneyCollectedLabel;
    [SerializeField] private TMP_Text _contractsCaughtLabel;
    [SerializeField] private TMP_Text _timeLeftLabel;

    void Start()
    {
        _wonLabel.text = Won ? "Victory" : "Failure";
        _moneyCollectedLabel.text = $"Earned ${MoneyCollected:n0}";
        _contractsCaughtLabel.text = $"Caught {ContractsCaught} suckers";

        TimeSpan time = TimeSpan.FromSeconds(TimeLeft);
        _timeLeftLabel.text = $"Time Left {time.ToString("mm\\:ss")}";
    }
}
