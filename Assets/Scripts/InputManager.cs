using UnityEngine;

/// <summary>
/// Singleton used to manage player input. It provides a centralized way to check for specific input conditions,
/// such as whether the "split modifier" key is currently pressed.
/// </summary>
public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    [SerializeField] private KeyCode _splitModifierKey = KeyCode.LeftShift;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public bool IsSplitModifierPressed()
    {
        return Input.GetKey(_splitModifierKey);
    }
}