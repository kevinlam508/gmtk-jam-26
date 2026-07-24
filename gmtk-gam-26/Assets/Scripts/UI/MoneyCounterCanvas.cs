using TMPro;
using UnityEngine;
using DG.Tweening;

public class MoneyCounterCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text _moneyText;
    private int _oldMoneyInt;
    private string _newMoneyString;
    [SerializeField] private float _tweenDuration;
    [SerializeField] private GameObject _moneyCounter;
    private Transform _moneyCounterStartTransform;
    [SerializeField] private float _punchScale;
    private Vector3 _scaleVector;
    [SerializeField] private float _shakeDistance;
    private Vector3 _shakeVector;
    [SerializeField] private AnimationCurve _shakeCurve;
    [SerializeField] private int vibrato;
    

    private void Start()
    {
        _moneyText.text = "0".PadLeft(7, '0');
        _moneyCounterStartTransform = _moneyCounter.transform;
        
        _scaleVector = new Vector3(_punchScale, _punchScale, _punchScale);
        _shakeVector = new Vector3(0, _shakeDistance, 0);
    }
    public void OnMoneyChanged(int addedMoney)
    {
        _moneyCounter.transform.position = _moneyCounterStartTransform.position;
        _moneyCounter.transform.localScale = _moneyCounterStartTransform.localScale;
        
        int newTotal = _oldMoneyInt + addedMoney;
        _newMoneyString = newTotal.ToString().PadLeft(7, '0');
        _moneyText.DOText(_newMoneyString, _tweenDuration, true, ScrambleMode.Custom, "0123456789").SetEase(_shakeCurve);
        _moneyCounter.transform.DOShakePosition(_tweenDuration, _shakeVector, vibrato).SetEase(_shakeCurve);

        // punch scale when done
        // maybe increase punch scale depending on how much money you got?
        DOVirtual.DelayedCall(_tweenDuration + 0.1f,()=>
            _moneyCounter.transform.DOPunchScale(_scaleVector, 0.1f)
            );
        
        _oldMoneyInt = newTotal;
    }

    [ContextMenu("Add 100")]
    public void TestAdd100()
    {
        OnMoneyChanged(100);
    }

    [ContextMenu("Add 5000")]
    public void TestAdd5000()
    {
        OnMoneyChanged(5000);
    }
}
