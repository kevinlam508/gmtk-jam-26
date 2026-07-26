using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tweener : MonoBehaviour
{
    [SerializeField] private float startDelay;
    [SerializeField] private float tweenDuration;
    [SerializeField] private bool playOnAwake;
    [SerializeField] private bool loop;
    [SerializeField] private bool destroyOnEnd;
    
    [SerializeField] private UnityEvent onComplete;
    
    [Header("Scale")]
        [SerializeField] private Vector3 startScale = Vector3.one;
        [SerializeField] private Vector3 endScale;
        [SerializeField] private AnimationCurve scaleTween;
        [SerializeField] private LoopType scaleLooptype;

    // [Header("UI Size Delta Tween")]
    //     [SerializeField] private RectTransform startRect;
    //     [SerializeField] private RectTransform endRect;
    //     [SerializeField] private AnimationCurve rectTween;
    //     [SerializeField] private LoopType rectLooptype;
    //     private RectTransform rect;
        
    [Header("Position")]
        [SerializeField] private GameObject startPos;
        [SerializeField] private GameObject endPos;
        [SerializeField] private AnimationCurve posTween;
        [SerializeField] private LoopType posLooptype;
        // private RectTransform rect;

    [Header("Rotation")]
        [SerializeField] private Vector3 startRot;
        [SerializeField] private Vector3 endRot;
        [SerializeField] private AnimationCurve rotTween;
        [SerializeField] private LoopType rotateLooptype;

    [Header("Alpha")]
        [SerializeField] private Image image;
        [SerializeField] private AnimationCurve alphaCurve;
        [SerializeField] private bool alpha;
        [SerializeField] private float startAlpha;
        [SerializeField] private float endAlpha;

    void OnEnable()
    {
        if (playOnAwake)
        {
            Reset();
            Tween();
        }
    }

    [ContextMenu("reset")]
    public void Reset()
    {
        if (scaleTween != null) gameObject.transform.localScale = startScale;
        if (posTween != null || startPos != null) gameObject.transform.position = startPos.transform.position;
        if (rotTween != null) gameObject.transform.localEulerAngles = startRot;
        if (alpha) image.DOFade(startAlpha, 0);
    }
    
    [ContextMenu("tween")]
    public void Tween()
    {
        StartCoroutine(DelayTween());
        
        IEnumerator DelayTween()
        {
            yield return new WaitForSeconds(startDelay);
            Scale();
            // RectTransform();
            Position();
            Rotate();
        }
    }

    // public void TweenReverse()
    // {
    //     StartCoroutine(DelayTween());
    //     
    //     IEnumerator DelayTween()
    //     {
    //         yield return new WaitForSeconds(startDelay);
    //         Scale(true);
    //         Position(true);
    //         Rotate(true);
    //     }
    // }

    public void Scale(bool isReverse = false)
    {
        if (scaleTween == null) return;
        
        // if (isReverse)
        // {
        //     gameObject.transform.localScale = endScale;
        //     gameObject.transform.DOScale(startScale, tweenDuration).SetEase(Ease.InBack).OnComplete(OnComplete);
        //     return;
        // }
        if (loop)
        {
            // Debug.Log("running scale tween");
            gameObject.transform.localScale = startScale;
            gameObject.transform.DOScale(endScale, tweenDuration).SetEase(scaleTween).SetLoops(-1, scaleLooptype);
        }
        // Debug.Log("running scale tween");
        gameObject.transform.localScale = startScale;
        gameObject.transform.DOScale(endScale, tweenDuration).SetEase(scaleTween).OnComplete(OnComplete);
    }
    
    public void Position(bool isReverse = false)
    {
        if (posTween == null) return;
        
        // if (isReverse)
        // {
        //     gameObject.transform.position = endPos.transform.position;
        //     gameObject.transform.DOMove(startPos.transform.position, tweenDuration).SetEase(Ease.InBack).OnComplete(OnComplete);
        //     return;
        // }
        if (loop)
        {
            // Debug.Log("running scale tween");
            gameObject.transform.position = startPos.transform.position;
            gameObject.transform.DOMove(endPos.transform.position, tweenDuration).SetEase(posTween).SetLoops(-1, posLooptype);
        }
        // Debug.Log("running scale tween");
        gameObject.transform.position = startPos.transform.position;
        gameObject.transform.DOMove(endPos.transform.position, tweenDuration).SetEase(posTween).OnComplete(OnComplete);
    }

    // public void RectTransform()
    // {
    //     if (rectTween == null) return;
    //     if (gameObject.GetComponent<RectTransform>() == null) return;
    //     else
    //     {
    //         rect = gameObject.GetComponent<RectTransform>();
    //     }
    //     if (loop)
    //     {
    //         // Debug.Log("running scale tween");
    //         rect = startRect;
    //         rect.DOAnchorMax(endRect.sizeDelta, tweenDuration).SetEase(rectTween).SetLoops(-1, rectLooptype);
    //         rect.DOAnchorMin(endRect.sizeDelta, tweenDuration).SetEase(rectTween).SetLoops(-1, rectLooptype);
    //     }
    //     // Debug.Log("running scale tween");
    //     rect = startRect;
    //     rect.DOAnchorMax(endRect.sizeDelta, tweenDuration).SetEase(rectTween).OnComplete(OnComplete);
    //     rect.DOAnchorMin(endRect.sizeDelta, tweenDuration).SetEase(rectTween).OnComplete(OnComplete);
    // }

    private void Rotate(bool isReverse = false)
    {
        if (rotTween == null) return;
        
        // if (isReverse)
        // {
        //     gameObject.transform.localEulerAngles = endRot;
        //     gameObject.transform.DORotate(startRot, tweenDuration, RotateMode.LocalAxisAdd).SetEase(Ease.InBack).OnComplete(OnComplete);
        //     return;
        // }
        if (loop)
        {
            gameObject.transform.localEulerAngles = startRot;
            gameObject.transform.DORotate(endRot, tweenDuration, RotateMode.LocalAxisAdd).SetEase(rotTween).SetLoops(-1, rotateLooptype);
        }
        else
        {
            // Debug.Log("running rotate tween");
            gameObject.transform.localEulerAngles = startRot;
            gameObject.transform.DORotate(endRot, tweenDuration, RotateMode.LocalAxisAdd).SetEase(rotTween).OnComplete(OnComplete);
        }
    }

    private void Alpha()
    {
        if (!alpha) return;
        
        image.DOFade(startAlpha, 0);
        image.DOFade(endAlpha, tweenDuration).SetEase(alphaCurve);
    }

    void OnComplete()
    {
        onComplete.Invoke();
        if(destroyOnEnd) Destroy(this);
    }
}
