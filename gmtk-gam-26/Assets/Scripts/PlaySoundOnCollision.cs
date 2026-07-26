using UnityEngine;

public class PlaySoundOnCollision : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<Vehicle>())
        {
            _audioSource.Play();
        }
    }
}
