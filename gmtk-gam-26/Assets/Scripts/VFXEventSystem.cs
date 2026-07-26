using UnityEngine;
using UnityEngine.Events;
public class VFXEventSystem : MonoBehaviour
{

    public UnityEvent customEvent; 

    public void InvokeEvent()
    {
        customEvent?.Invoke();
    }
}