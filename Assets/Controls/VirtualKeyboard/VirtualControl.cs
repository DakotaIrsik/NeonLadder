using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualControlManager : MonoBehaviour
{
    public InputActionReference actionReference; // Reference to your InputAction

    public void OnPointerDown()
    {
        // Start the action or perform the action
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Enable();
            actionReference.action.ReadValue<float>(); // Or ReadValue<Vector2>() if it's a stick input, for example
        }
    }

    public void OnPointerUp()
    {
        // Stop the action
        if (actionReference != null && actionReference.action != null)
        {
            actionReference.action.Disable();
        }
    }
}
