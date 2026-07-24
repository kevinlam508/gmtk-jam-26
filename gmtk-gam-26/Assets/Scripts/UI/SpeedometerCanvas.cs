using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class SpeedometerCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text _speedText;
    [SerializeField] private float _tweenDuration;
    [SerializeField] private GameObject _speedometer;
    private Transform _speedometerStartTransform;
    [SerializeField] private float _punchScale;
    private Vector3 _scaleVector;
    [SerializeField] private float _shakeDistance;
    private Vector3 _shakeVector;
    [SerializeField] private int vibrato;
    private int _oldSpeed;
    private Tween shakeTween;
    private Tween scaleTween;

    [SerializeField] private bool _debug;
    [SerializeField] private int _debugSpeed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _speedText.text = 0.ToString();
        
        _speedometerStartTransform = _speedometer.transform;
        
        shakeTween = _speedometer.transform.DOShakePosition(_tweenDuration, _shakeVector, vibrato).SetLoops(-1);
        shakeTween.Pause();
    }

    public void OnSpeedChanged(int currentSpeed)
    {
        if (currentSpeed == _oldSpeed)
        {
            return;
        }

        _shakeVector = new Vector3(_shakeDistance, _shakeDistance, _shakeDistance);
        _scaleVector = new Vector3(_punchScale, _punchScale, _punchScale);
            
        // _speedometer.transform.position = _speedometerStartTransform.position;
        
        // reset
        if (scaleTween != null)
        {
            scaleTween.Kill();
        }

        if (currentSpeed >= 80)
        {
            _speedometer.transform.localScale = Vector3.one*1.2f;
        }
        else
        {
            _speedometer.transform.localScale = Vector3.one;
        }
        
        
        _speedText.text = currentSpeed.ToString();
        Debug.Log(currentSpeed);

        if (_oldSpeed < currentSpeed)
        {
            // accelerated
            
            if (currentSpeed >= 80)
            {
                scaleTween = _speedText.transform.DOPunchScale(_scaleVector + Vector3.one*0.2f, _tweenDuration);
            
                // // make sure it returns to og scale 
                // DOVirtual.DelayedCall(_tweenDuration, ()=>
                //     _speedometer.transform.localScale = Vector3.one
                // );
            }
            else
            {
                scaleTween = _speedText.transform.DOPunchScale(_scaleVector, _tweenDuration);
            
                // make sure it returns to og scale 
                // DOVirtual.DelayedCall(_tweenDuration, ()=>
                //     _speedometer.transform.localScale = Vector3.one
                // );
            }

            
        }
        else
        {
            // for now at least, don't do anything when decelerating
        }
        
        if (currentSpeed >= 80)
        {
            if (!shakeTween.IsPlaying())
            {
                shakeTween = _speedometer.transform.DOShakePosition(_tweenDuration, _shakeVector, vibrato).SetLoops(-1);
                shakeTween.Play();
                Debug.Log($"is shaking, dotween says shake tween is {shakeTween.IsPlaying()}");
            }
            else
            {
                Debug.Log($"is ALREADY shaking, dotween says shake tween is {shakeTween.IsPlaying()}");
            }
        }
        
        else
        {
            if (shakeTween != null)
            {
                shakeTween.Kill();
            }
        }

        _oldSpeed = currentSpeed;
    }

    private void Update()
    {
        if (_debug)
        {
            if (_debugSpeed == _oldSpeed)
            {
                return;
            }
            OnSpeedChanged(_debugSpeed);
        }
    }
}
