using System;
using TMPro;
using UnityEngine;

public class TimerCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerLabel;
    [SerializeField] private string _timeFormat = "<mspace=0em>{0}</mspace>";

    public void UpdateGlobalTimerUI(float currentTime, float totalTime)
    {
        _timerLabel.text = string.Format(_timeFormat, 
            TimeSpan.FromSeconds(currentTime).ToString("mm"),
            TimeSpan.FromSeconds(currentTime).ToString("ss"));
    }
}
