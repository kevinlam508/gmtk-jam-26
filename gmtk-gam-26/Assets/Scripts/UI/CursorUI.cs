using UnityEngine;
using UnityEngine.InputSystem;


public class CursorUI : MonoBehaviour
{
    [SerializeField] private RectTransform _cursorTransform;
    void Start()
    {
        Cursor.visible = false;

    }

    // Update is called once per frame
    void Update()
    {
        _cursorTransform.position = Mouse.current.position.ReadValue();

    }
}
