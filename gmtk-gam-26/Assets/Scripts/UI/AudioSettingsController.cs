using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Slider _uiSlider;
    [SerializeField] private AudioMixerGroup _masterMixer;
    [SerializeField] private AudioMixerGroup _musicMixer;
    [SerializeField] private AudioMixerGroup _sfxMixer;
    [SerializeField] private AudioMixerGroup _uiMixer;

    private void Awake()
    {
        _masterSlider.onValueChanged.AddListener(OnMasterValueChanged);
        _musicSlider.onValueChanged.AddListener(OnMusicValueChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXValueChanged);
        _uiSlider.onValueChanged.AddListener(OnUIValueChanged);
    }

    public void OnMasterValueChanged(float value)
    {
        SetMixerVolume(_masterMixer.audioMixer, value);
    }

    public void OnMusicValueChanged(float value)
    {
        SetMixerVolume(_musicMixer.audioMixer, value);
    }

    public void OnSFXValueChanged(float value)
    {
        SetMixerVolume(_sfxMixer.audioMixer, value);
    }

    public void OnUIValueChanged(float value)
    {
        SetMixerVolume(_uiMixer.audioMixer, value);
    }

    private void SetMixerVolume(AudioMixer mixer, float sliderValue)
    {
        mixer.SetFloat("Volume", Mathf.Log10(sliderValue) * 20);
        
    }
}
