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
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private DragManager _dragManager;

    private void Awake()
    {
        // We have to initialize the database before anyone tries to use it
        _itemDatabase.Init();

        ServiceLocator.Register<ItemDatabase>(_itemDatabase);
        ServiceLocator.Register<InventoryManager>(_inventoryManager);
        ServiceLocator.Register<SpawnManager>(_spawnManager);
        ServiceLocator.Register<DragManager>(_dragManager);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ItemDatabase>();
        ServiceLocator.Unregister<InventoryManager>();
        ServiceLocator.Unregister<SpawnManager>();
        ServiceLocator.Unregister<DragManager>();
    }
}