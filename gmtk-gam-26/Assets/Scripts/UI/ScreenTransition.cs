using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenTransition : MonoBehaviour
{ 
    public static ScreenTransition instance;
    [SerializeField]
    private float duration = 1f;
    private Material mat;


    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start() 
    {
        mat = GetComponent<Image>().material;
        FadeOut();
    }

    public void FadeIn() 
    {
        mat.SetFloat("_FadeTransition", 0);
        mat.DOFloat( 1, "_FadeTransition", duration);
    }

    public void FadeOut() 
    {
        mat.SetFloat("_FadeTransition", 1);
        mat.DOFloat( 0, "_FadeTransition", duration);
    }

    public void FadeInOut()
    {
        mat.SetFloat("_FadeTransition", 0);
        mat.DOFloat( 1, "_FadeTransition", duration/2).SetLoops(2, LoopType.Yoyo);
    }
       
        
}
