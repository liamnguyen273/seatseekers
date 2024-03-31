using System.Collections;
using System.Collections.Generic;
using com.brg.UnityComponents;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private ContextMenuGeneric _menu;
    
    public void Show()
    {
        var mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        _menu.Show(mousePosition, null);
    }
}