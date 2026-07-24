using UnityEngine;
using UnityEngine.Rendering;

public class LightingDayNightController : MonoBehaviour
{
    [SerializeField][Range(0f,1f)] float _floatDayNightLerp;
    [SerializeField] private Gradient _skyBoxTintGradient;
    [SerializeField][Range(0f,1f)] float _timeSpeed;
    [SerializeField] Renderer _renderer;
    [SerializeField] AnimationCurve _volumeLerpCurve1;
    [SerializeField] AnimationCurve _volumeLerpCurve2;
    [SerializeField] AnimationCurve _skyBoxLerpCurve;
    [SerializeField] AnimationCurve _directionalLightLerpCurve;
    [SerializeField] Volume _nightVolume;
    [SerializeField] Volume _morningVolume;
    [SerializeField] Volume _transitionVolume;
    [SerializeField] private Gradient _directionalLightColorGradient;
    [SerializeField] private Light _directionalLight;
    [SerializeField] private Material _actualSkyBoxMaterial;
    private Material _skyBoxSphereMaterial;
    private float _time = 0f;
   [SerializeField] private float _lerpTimeLength = 20f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake() 
    {
        _skyBoxSphereMaterial = _renderer.material;
    }
    void Start()
    {
        _nightVolume.weight = 1f;
        _transitionVolume.weight = 0f;
        _morningVolume.weight = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        _time+=(Time.deltaTime * _timeSpeed);
        float lerpAmt = _time/_lerpTimeLength;
        _skyBoxSphereMaterial.SetFloat("_SkyboxTextureLerp", _skyBoxLerpCurve.Evaluate(lerpAmt));
        
        
        if(lerpAmt <= 0.5f)
        {
            _nightVolume.weight = 1f;
            _morningVolume.weight = 0f;
            _transitionVolume.weight = _volumeLerpCurve1.Evaluate(lerpAmt * 2f);
        }
        else
        {
            _nightVolume.weight = 1f;
            _transitionVolume.weight = 1f;
            _morningVolume.weight = _volumeLerpCurve2.Evaluate((lerpAmt *2f) - 1f);
        }
        if(_time >= _lerpTimeLength)
        {
            _time = 0f;
        }
        _directionalLight.color = _directionalLightColorGradient.Evaluate(_directionalLightLerpCurve.Evaluate(lerpAmt));
    }
}
