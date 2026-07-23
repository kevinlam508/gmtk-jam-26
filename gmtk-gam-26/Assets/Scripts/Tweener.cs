using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Tweener : MonoBehaviour
{
    [SerializeField] private float tweenDuration;
    [SerializeField] private bool playOnAwake;
    [SerializeField] private bool loop;
    
    [SerializeField] private UnityEvent onComplete;
    
    [Header("Scale")]
        [SerializeField] private Vector3 startScale = Vector3.one;
        [SerializeField] private Vector3 endScale;
        [SerializeField] private AnimationCurve scaleTween;
        [SerializeField] private LoopType scaleLooptype;

    [Header("UI Size Delta Tween")]
        [SerializeField] private RectTransform startRect;
        [SerializeField] private RectTransform endRect;
        [SerializeField] private AnimationCurve rectTween;
        [SerializeField] private LoopType rectLooptype;
        private RectTransform rect;

    [Header("Rotation")]
        [SerializeField] private Vector3 startRot;
        [SerializeField] private Vector3 endRot;
        [SerializeField] private AnimationCurve rotTween;
        [SerializeField] private LoopType rotateLooptype;

    void OnEnable()
    {
        if (playOnAwake)
        {
            Tween();
        }
        
    }
    
    public void Tween()
    {
        Scale();
        // RectTransform();
        Rotate();
    }

    public void Scale()
    {
        if (scaleTween == null) return;
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

    public void RectTransform()
    {
        if (rectTween == null) return;
        if (gameObject.GetComponent<RectTransform>() == null) return;
        else
        {
            rect = gameObject.GetComponent<RectTransform>();
        }
        if (loop)
        {
            // Debug.Log("running scale tween");
            rect = startRect;
            rect.DOAnchorMax(endRect.sizeDelta, tweenDuration).SetEase(rectTween).SetLoops(-1, rectLooptype);
            rect.DOAnchorMin(endRect.sizeDelta, tweenDuration).SetEase(rectTween).SetLoops(-1, rectLooptype);
        }
        // Debug.Log("running scale tween");
        rect = startRect;
        rect.DOAnchorMax(endRect.sizeDelta, tweenDuration).SetEase(rectTween).OnComplete(OnComplete);
        rect.DOAnchorMin(endRect.sizeDelta, tweenDuration).SetEase(rectTween).OnComplete(OnComplete);
    }

    private void Rotate()
    {
        if (rotTween == null) return;
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

    void OnComplete()
    {
        onComplete.Invoke();
    }
}
