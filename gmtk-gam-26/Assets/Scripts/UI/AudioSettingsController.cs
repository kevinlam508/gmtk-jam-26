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
        _masterMixer.audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void OnMusicValueChanged(float value)
    {
        _musicMixer.audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void OnSFXValueChanged(float value)
    {
        _sfxMixer.audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }

    public void OnUIValueChanged(float value)
    {
        _uiMixer.audioMixer.SetFloat("UIVolume", Mathf.Log10(value) * 20);
    }
}
