using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
/// <summary>
/// A MonoBehaviour responsible for initializing and registering core game systems at the start of the game.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Game Data")]
    [SerializeField] private ItemDatabase _itemDatabase;

    [Header("Game Managers")]
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private DragManager _dragManager;
    [SerializeField] private InputManager _inputManager;

    private SaveLoadManager _saveLoadManager;
    private InventoryManager _inventoryManager;

    [Header("Inventory Setup")]
    [SerializeField] private List<Slot> _sceneSlots = new List<Slot>();

    private void Awake()
    {
        _itemDatabase.Initialize();
        _inventoryManager = new InventoryManager(_sceneSlots, _spawnManager, _itemDatabase);
        _saveLoadManager = new SaveLoadManager(_inventoryManager);
        _spawnManager.Initialize(_inventoryManager, _dragManager);
        _dragManager.Initialize(_inputManager, _spawnManager, _inventoryManager);
    }

#if UNITY_EDITOR
    [ContextMenu("Find And Register All Slots")]
    private void SetupSlotsFromEditor()
    {
        var foundSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);
        _sceneSlots = foundSlots.OrderBy(s => s.transform.GetSiblingIndex()).Reverse().ToList();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully found and registered {_sceneSlots.Count} Slots!", this);
    }
#endif
}