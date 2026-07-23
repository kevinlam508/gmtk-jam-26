using DG.Tweening;
using UnityEngine;

public class CanvasGroupToggler : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool toggleOnStart = false;
    [SerializeField] float durationOnStart = 0;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (toggleOnStart)
        {
            ShowCanvasGroup(durationOnStart);
        }
    }
    public void ShowCanvasGroup(float fadeDuration = 0)
    {
        if (canvasGroup == null)
        {
            return;
        }

        // canvasGroup.alpha = 1f;
        canvasGroup.DOFade(1f, fadeDuration);
        if (interactable)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;  
        }
        // canvasGroup.interactable = true;
        // canvasGroup.blocksRaycasts = true;
    }

    public void HideCanvasGroup(float fadeDuration = 0)
    {
        if (canvasGroup == null)
        {
            return;
        }

        // canvasGroup.alpha = 0f;
        canvasGroup.DOFade(0f, fadeDuration);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}