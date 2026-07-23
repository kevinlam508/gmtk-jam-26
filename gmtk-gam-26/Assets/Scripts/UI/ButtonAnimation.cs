using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private float hoverScale = 1.07f;
    [SerializeField] private float returnScale = 1f;
    [SerializeField] private float clickScale = 0.98f;

    [Header("Audio")] 
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _hoverSfx;
    [SerializeField] private AudioClip _clickSfx;
    
    private float hoverTweenDuration = 0.15f;
    private float clickTweenDuration = 0.05f;
    private float returnToDuration = 0.05f;
    private bool ran = false;
    private Ease hoverEase = Ease.OutExpo;
    private Ease clickEase = Ease.OutBack;
    //

    private void Start()
    {
    }
    
    // hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!ran && gameObject.GetComponent<Button>().interactable == true)
        {
            gameObject.transform.DOScale(hoverScale, hoverTweenDuration).SetEase(hoverEase).SetLink(gameObject);
            _audioSource.PlayOneShot(_hoverSfx);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.transform.DOKill();
        gameObject.transform.DOScale(returnScale, returnToDuration).SetLink(gameObject);
    }


    // click
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ran && gameObject.GetComponent<Button>().interactable == true)
        {
            gameObject.transform.DOScale(clickScale, clickTweenDuration).SetEase(clickEase).SetLink(gameObject);
            _audioSource.PlayOneShot(_clickSfx);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        this.transform.DOKill();
        this.transform.DOScale(returnScale, returnToDuration).SetLink(gameObject);
        ran = false;
    }
}