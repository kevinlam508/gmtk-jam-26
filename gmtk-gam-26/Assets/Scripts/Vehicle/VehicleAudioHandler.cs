using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class VehicleAudioHandler : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _revDriveSource;
    [SerializeField] private AudioSource _driftSource;
    [SerializeField] private AudioSource _impactSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] _revClips;
    [SerializeField] private AudioClip _driveClip;
    [SerializeField] private AudioClip _driftClip;

    private Vehicle _vehicle;
    private bool _isAccelerating;
    private bool _isSteering;
    private bool _isPlayingRev;
    private float _revClipTimer;

    private void Awake()
    {
        _vehicle = GetComponent<Vehicle>();
    }

    private void FixedUpdate()
    {
        _isAccelerating = _vehicle.DesiredMagnitude != 0f && _vehicle.IsGrounded;
        _isSteering = _vehicle.VisualSteer != 0f && _vehicle.IsGrounded;
    }

    private void Update()
    {
        if (_isAccelerating)
        {
            if (!_revDriveSource.isPlaying)
            {
                _revDriveSource.clip = _revClips[Random.Range(0, _revClips.Length)];
                _revClipTimer = _revDriveSource.clip.length;
                _revDriveSource.loop = false;
                _revDriveSource.pitch = Random.Range(0.5f, 1f);
                _revDriveSource.Play();
                _isPlayingRev = true;
            }
            else // already playing
            {
                if (_isPlayingRev)
                {
                    _revClipTimer += Time.deltaTime;
                    if (_revClipTimer >= _revDriveSource.clip.length)
                    {
                        _revDriveSource.clip = _driveClip;
                        _revDriveSource.loop = true;
                        _revDriveSource.Play();
                        _isPlayingRev = false;
                    }
                }
            }
        }
        else // no longer accelerating or grounded
        {
            if (_revDriveSource.isPlaying)
            {
                _revDriveSource.Stop();
            }
        }

        if (_isSteering)
        {
            if (!_driftSource.isPlaying)
            {
                _driftSource.clip = _driftClip;
                _driftSource.loop = true;
                _driftSource.pitch = Random.Range(0.5f, 1f);
                _driftSource.Play();
            }
        }
        else // no longer steering or grounded
        {
            if (_driftSource.isPlaying)
            {
                _driftSource.Stop();
            }
        }
    }
}
