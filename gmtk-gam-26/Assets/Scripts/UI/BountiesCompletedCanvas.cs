using UnityEngine;
using TMPro;
using DG.Tweening;

public class BountiesCompletedCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text _bountyCompletedText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateBountiesCompleted(int current, int total)
    {
        _bountyCompletedText.transform.DOPunchScale(Vector3.one*0.3f, .2f);
        _bountyCompletedText.text = $"Bounties Completed <color=#C270CF>{current}/{total}</color>";
    }
}
