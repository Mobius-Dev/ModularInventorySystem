using UnityEngine;

/// <summary>
/// Provides a centralized way to check for specific input conditions,
/// such as whether the "split modifier" key is currently pressed.
/// </summary>
public static class InputUtility
{
    public static bool IsSplitModifierPressed()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }
}