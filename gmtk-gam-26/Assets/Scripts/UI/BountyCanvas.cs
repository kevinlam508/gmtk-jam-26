using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class BountyCanvas : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private GameObject _currentBounty;
    [SerializeField] private GameObject _coffin;
    [SerializeField] private Image _coffinFill;
    [SerializeField] private Image _coffinFillGraphic;
    private Color _coffinDefaultColor;
    [SerializeField] private Color _coffinFillWarningColor;
    [SerializeField] private GameObject _gravestone;
    [SerializeField] private Image _bountyPortrait;
    [SerializeField] private TMP_Text _bountyValueText;
    [SerializeField] private Image _bountyCarImage;
    [SerializeField] private GameObject _completeText;
    [SerializeField] private TMP_Text _failedtext;
    [SerializeField] private GameObject _gravestoneCrack;
    [SerializeField] private GameObject _coffinOverlay;
    private bool startedShaking;

    [Header("Tween Settings")]
    [SerializeField] private Transform _newBountyStartPos;
    [SerializeField] private Transform _newBountyEndPos;
    [SerializeField] private float _newBountyDropDuration;
    [SerializeField] private AnimationCurve _newBountyDropCurve;
    [SerializeField] private float _punchScale;
    [SerializeField] private float _shakeDistance;
    [SerializeField] private int vibrato;
    [SerializeField] private float _rotationAmount;


    private Vector3 _coffinStartPos;
    private Vector3 _coffinStartRot;
    private Vector3 _graveStartPos;
    private Vector3 _graveStartRot;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _coffinStartPos = _coffin.transform.position;
        _coffinStartRot = _coffin.transform.eulerAngles;
        _graveStartPos = _gravestone.transform.position;
        _graveStartRot = _gravestone.transform.eulerAngles;
        _currentBounty.transform.position = _newBountyStartPos.position;
        _currentBounty.transform.localScale = _newBountyStartPos.localScale;
        _canvasGroup.alpha = 0;
        _completeText.SetActive(false);
        _failedtext.gameObject.SetActive(false);
        _gravestoneCrack.SetActive(false);
        _coffinOverlay.SetActive(false);
        _coffinDefaultColor = _coffinFillGraphic.color;
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
            _coffinFillGraphic.color = _coffinDefaultColor;
            
            // tween down with scale
            _currentBounty.transform.DOMove(_newBountyEndPos.position, _newBountyDropDuration).SetEase(_newBountyDropCurve);
            _currentBounty.transform.DOScale(_newBountyEndPos.localScale.x, _newBountyDropDuration)
                .SetEase(_newBountyDropCurve);
            
            _canvasGroup.DOFade(1, _newBountyDropDuration);

            yield return new WaitForSeconds(_newBountyDropDuration - 0.1f);
            ShakeObj(_coffin, 0.2f);
            ShakeObj(_gravestone, 0.2f);
            _currentBounty.transform.DOPunchScale(Vector3.one*_punchScale, 0.1f);
            
            yield return new WaitForSeconds(0.2f);
            
            _coffin.transform.position = _coffinStartPos;
            _coffin.transform.eulerAngles = _coffinStartRot;
            _gravestone.transform.position = _graveStartPos;
            _gravestone.transform.eulerAngles = _graveStartRot;
        }
    }

    private void ShakeObj(GameObject obj, float duration, float shakeMult = 1f, int vibratoMult = 1)
    {
        obj.transform.DOShakePosition(duration, Vector3.one*_shakeDistance*shakeMult, vibrato*vibratoMult);
        obj.transform.DOShakeRotation(duration, new Vector3(0, 0, _rotationAmount)*shakeMult, vibrato*vibratoMult);
    }
    
    // set current bounty UI
    public void SetBountyUI(ContractProfileData profile)
    {
        _bountyPortrait.sprite = profile.Portrait;
        _bountyValueText.text = $"${profile.Bounty:n0}"; // formats value with comma
        NewBounty();
    }
    
    // update timer value
    public void UpdateBountyTimerUI(float currentTime, float totalTime)
    {
        // if timer is running out, coffin should shake, maybe change the fill color too

        _coffinFill.fillAmount = currentTime / totalTime;

        if (currentTime / totalTime <= .1)
        {
            _coffinFillGraphic.color = _coffinFillWarningColor;
            
            if (!startedShaking)
            {
                ShakeObj(_coffin, 5, 0.75f);
                startedShaking = true;
            }
        }
    }
    
    // timer runs out, bounty failed
    public void BountyFailed()
    {
        // shake/rotate,
        
        
        StartCoroutine(BountyFailedCoroutine());

        IEnumerator BountyFailedCoroutine()
        {
            _failedtext.gameObject.SetActive(true);
            _gravestoneCrack.SetActive(true);
            _coffinOverlay.SetActive(true);
            _failedtext.alpha = 0f;
            _failedtext.transform.eulerAngles = Vector3.zero;
            _currentBounty.transform.DOPunchScale(Vector3.one*_punchScale, 0.1f);

            _failedtext.DOFade(1, 0.2f);
            yield return new WaitForSeconds(.2f);
            _failedtext.transform.DORotate(new Vector3(0, 0, -10), 0.2f).SetEase(Ease.OutBack);
            ShakeObj(_coffin, 0.8f, 0.75f);
            ShakeObj(_gravestone, 0.8f, 0.75f);
            ShakeObj(_coffinOverlay, 0.8f, 0.75f);
            ShakeObj(_gravestoneCrack, 0.8f, 0.75f);
            
            yield return new WaitForSeconds(.8f);
            
            _canvasGroup.DOFade(0, .5f);
        
            _failedtext.gameObject.SetActive(false);
            _gravestoneCrack.SetActive(false);
            _coffinOverlay.SetActive(false);
            _coffin.transform.position = _coffinStartPos;
            _coffin.transform.eulerAngles = _coffinStartRot;
            _gravestone.transform.position = _graveStartPos;
            _gravestone.transform.eulerAngles = _graveStartRot;
        }
    }
    
    // bounty complete
    public void BountyComplete()
    {
        // some effect idk
        
        StartCoroutine(BountyCompleteCoroutine());

        IEnumerator BountyCompleteCoroutine()
        {
            _completeText.SetActive(true);
            _completeText.transform.localScale = Vector3.one*0.15f;
        
            _completeText.transform.DOScale(1, .2f).SetEase(Ease.OutBack);
            
            yield return new WaitForSeconds(1);
                
            _canvasGroup.DOFade(0, .5f);
        
            _completeText.SetActive(false);
            _coffin.transform.position = _coffinStartPos;
            _coffin.transform.eulerAngles = _coffinStartRot;
            _gravestone.transform.position = _graveStartPos;
            _gravestone.transform.eulerAngles = _graveStartRot;
        }
    }
}
