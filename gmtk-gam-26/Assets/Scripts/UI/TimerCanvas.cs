using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class TimerCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerLabel;
    [SerializeField] private string _timeFormat = "<mspace=0em>{0}</mspace>";
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _warningColor;
    [SerializeField] private float _punchScale;
    private Vector3 _scaleVector;
    [SerializeField] private float _shakeDistance;
    private Tween shakeTween;
    [SerializeField] private int vibrato;

    private int _lastPunch = 10;

    private void Awake()
    {
        _scaleVector = Vector3.one * _punchScale;
        shakeTween = _timerLabel.transform.DOShakePosition(1, Vector3.one * _shakeDistance, vibrato).SetLoops(-1);
        shakeTween.Pause();
    }

    public void UpdateGlobalTimerUI(float currentTime, float totalTime)
    {
        _timerLabel.text = string.Format(_timeFormat, 
            TimeSpan.FromSeconds(currentTime).ToString("mm"),
            TimeSpan.FromSeconds(currentTime).ToString("ss"));

        _timerLabel.color = currentTime <= 10
            ? _warningColor : _normalColor;
        if (currentTime <= _lastPunch)
        {
            _timerLabel.transform.DOPunchScale(_scaleVector, 0.1f);
            _lastPunch = (int)currentTime;
        }

        if (currentTime <= 3 && !shakeTween.IsPlaying())
        {
            shakeTween.Play();
        }
    }
}
