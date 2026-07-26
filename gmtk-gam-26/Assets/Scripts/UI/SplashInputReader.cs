using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class SplashInputReader : MonoBehaviour
{
    public UnityEvent TransitionToMainPanel;
    [SerializeField] private GameObject splashPanel; 

    // Update is called once per frame
    void Update()
    {
        InputSystem.onAnyButtonPress.Call((action) => TransitionToMainPanel?.Invoke());
    }

    void Start()
    {
        splashPanel.SetActive(true);
    }
}
