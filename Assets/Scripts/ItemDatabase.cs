using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Represents a database of item definitions for an inventory system, enabling efficient retrieval of items by their
/// unique identifiers.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemDef> AllItems;

    // Optimization: A Dictionary is faster than a List for lookups
    private Dictionary<string, ItemDef> _lookup;

    public void Init()
    {
        // Convert the List to a Dictionary for instant access
        _lookup = new Dictionary<string, ItemDef>();
        foreach (var item in AllItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.ItemName))
            {
                if (!_lookup.ContainsKey(item.ItemName))
                {
                    _lookup.Add(item.ItemName, item);
                }
                else
                {
                    Debug.LogWarning($"Duplicate Item ID found: {item.ItemName} on {item.name}");
                }
            }
        }
    }

    public ItemDef GetItemByID(string id)
    {
        // Ensure dictionary is ready
        if (_lookup == null) Init();

        if (_lookup.TryGetValue(id, out ItemDef item))
        {
            return item;
        }

        Debug.LogError($"Item ID not found in Database: {id}");
        return null;
    }

#if UNITY_EDITOR
    // A button in the inspector to automatically load all ItemDef assets from the project into the database
    [ContextMenu("Load All Items From Project")]
    public void LoadAllItems()
    {
        AllItems = new List<ItemDef>();

        // Find all ScriptableObjects of type ItemDef in the project
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDef");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemDef item = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDef>(path);
            AllItems.Add(item);
        }

        // Mark the database as dirty to ensure it gets saved
        UnityEditor.EditorUtility.SetDirty(this); 

        Debug.Log($"Loaded {AllItems.Count} items into the database.");
    }
#endif
}