using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class BountyCanvas : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _currentBounty;
    [SerializeField] private GameObject _coffin;
    [SerializeField] private Image _coffinFill;
    [SerializeField] private Color _coffinFillWarningColor;
    [SerializeField] private GameObject _gravestone;
    [SerializeField] private Image _bountyPortrait;
    [SerializeField] private TMP_Text _bountyValueText;
    [SerializeField] private Image _bountyCarImage;

    [Header("Tween Settings")]
    [SerializeField] private Transform _newBountyStartPos;
    [SerializeField] private Transform _newBountyEndPos;
    [SerializeField] private float _newBountyDropDuration;
    [SerializeField] private AnimationCurve _newBountyDropCurve;
    [SerializeField] private float _punchScale;
    [SerializeField] private float _shakeDistance;
    [SerializeField] private int vibrato;
    [SerializeField] private float _rotationAmount;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentBounty.transform.position = _newBountyStartPos.position;
        _currentBounty.transform.localScale = _newBountyStartPos.localScale;
        _canvasGroup.alpha = 0;
    }

    [ContextMenu("new bounty tween")]
    public void NewBounty()
    {
        StartCoroutine(BountyTweenCoroutine());
        
        IEnumerator BountyTweenCoroutine()
        {
            _currentBounty.transform.position = _newBountyStartPos.position;
            _currentBounty.transform.localScale = _newBountyStartPos.localScale;
            _canvasGroup.alpha = 0;
            
            // tween down with scale
            _currentBounty.transform.DOMove(_newBountyEndPos.position, _newBountyDropDuration).SetEase(_newBountyDropCurve);
            _currentBounty.transform.DOScale(_newBountyEndPos.localScale.x, _newBountyDropDuration)
                .SetEase(_newBountyDropCurve);
            
            _canvasGroup.DOFade(1, _newBountyDropDuration);

            yield return new WaitForSeconds(_newBountyDropDuration - 0.1f);
            ShakeObj(_coffin, 0.2f);
            ShakeObj(_gravestone, 0.2f);
            _currentBounty.transform.DOPunchScale(Vector3.one*_punchScale, 0.1f);
        }
    }

    private void ShakeObj(GameObject obj, float duration)
    {
        obj.transform.DOShakePosition(duration, Vector3.one*_shakeDistance, vibrato);
        obj.transform.DOShakeRotation(duration, new Vector3(0, 0, _rotationAmount), vibrato);
    }
    
    // set current bounty UI
    public void SetBountyUI(Sprite portrait, int value /*, Image car*/)
    {
        _bountyPortrait.sprite = portrait;
        _bountyValueText.text = $"${value:n0}"; // formats value with comma
    }
    
    // update timer value
    public void UpdateBountyTimerUI(float time)
    {
        // if timer is running out, coffin should shake, maybe change the fill color too

        _coffinFill.fillAmount = time;
    }
    
    // timer runs out, bounty failed
    public void BountyFailed()
    {
        // shake/rotate,
        
    }
    
    // bounty complete
    public void BountyComplete()
    {
        // some effect idk
    }
}
