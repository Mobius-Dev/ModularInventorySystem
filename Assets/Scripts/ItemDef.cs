using UnityEngine;

/// <summary>
/// Scriptable object which represents an item type in the inventory system. It contains all the static data about an item, such as its name, sprite, and maximum stack size.
/// Each unique item type should have a corresponding ItemDef asset created in the Unity editor.
/// </summary>
[CreateAssetMenu(fileName = "New Item Definition", menuName = "Inventory/Item Definition")]
public class ItemDef : ScriptableObject
{
    public string ItemID;
    public Sprite Sprite;
    public int MaxStackSize = 1;
}