using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class EndScreen : MonoBehaviour
{
    public static bool Won;
    public static int MoneyCollected;
    public static int ContractsCaught;
    public static float TimeLeft;

    [SerializeField] private TMP_Text _wonLabel;
    [SerializeField] private TMP_Text _moneyCollectedLabel;
    [SerializeField] private TMP_Text _contractsCaughtLabel;
    [SerializeField] private TMP_Text _timeLeftLabel;

    [Header("Tweening")]
    private List<GameObject> _itemsToTweenList;
    [SerializeField] private float _tweenDuration;
    [SerializeField] private float _timeBetweenTweens;
    [SerializeField] private float _startDelay;
    [SerializeField] private GameObject _toMainButton;
    

    void Start()
    {
        _wonLabel.text = Won ? "Victory" : "Failure";
        _moneyCollectedLabel.text = $"Earned ${MoneyCollected:n0}";
        _contractsCaughtLabel.text = $"Caught {ContractsCaught} suckers";

        TimeSpan time = TimeSpan.FromSeconds(TimeLeft);
        _timeLeftLabel.text = $"Time Left {time.ToString("mm\\:ss")}";

        _itemsToTweenList = new List<GameObject>()
        {
            _wonLabel.gameObject,
            _moneyCollectedLabel.gameObject,
            _contractsCaughtLabel.gameObject,
            _timeLeftLabel.gameObject,
            _toMainButton
        };
        
        foreach (GameObject item in _itemsToTweenList)
        {
            item.SetActive(false);
        }

        AddItemToList();
    }

    private void AddItemToList()
    {
        StartCoroutine(AddItemToListCoroutine());

        IEnumerator AddItemToListCoroutine()
        {
            yield return new WaitForSeconds(_startDelay);
            
            foreach (GameObject item in _itemsToTweenList)
            {
                item.SetActive(true);
                TweenItem(item);
                yield return new WaitForSeconds(_timeBetweenTweens);
                
            }
        }
    }

    private void TweenItem(GameObject item)
    {
        // scale punch tween the object
        item.transform.DOPunchScale(Vector3.one*0.15f, _tweenDuration);
    }
}
