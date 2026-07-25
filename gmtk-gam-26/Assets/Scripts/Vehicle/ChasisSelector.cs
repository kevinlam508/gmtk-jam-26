using System.Linq;
using UnityEngine;

public class ChasisSelector : MonoBehaviour
{
    [SerializeField] private GameObject[] _chasis;
    [SerializeField] private HookTarget _hook;

    private void Awake()
    {
        int showIndex = Random.Range(0, _chasis.Length);
        for (int i = 0; i < _chasis.Length; i++)
        {
            _chasis[i].SetActive(i == showIndex);
        }

        _hook.renderer = _chasis[showIndex].GetComponent<MeshRenderer>();
    }
}
