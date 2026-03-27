using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Input Actions")]
    [Tooltip("The key held down to split an item stack.")]
    [SerializeField] private InputAction _splitModifierAction;

    private void OnEnable()
    {
        _splitModifierAction.Enable();
    }

    private void OnDisable()
    {
        _splitModifierAction.Disable();
    }

    public bool IsSplitModifierPressed()
    {
        // Read the state of the action directly
        return _splitModifierAction.IsPressed();
    }
}