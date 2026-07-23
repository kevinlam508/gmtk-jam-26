using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenSwitcher : MonoBehaviour
{
    

    [SerializeField] private string _destinationScene;
    [SerializeField] private float waitTime = 0f;
    
    public void Switch()
    {
        StartCoroutine(WaitToSwitch());
    }

    private IEnumerator WaitToSwitch()
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene(_destinationScene);
    }
}
