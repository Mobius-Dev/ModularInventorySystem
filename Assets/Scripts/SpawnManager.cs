using UnityEngine;
using UnityEngine.Pool;

public class SpawnManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private int _defaultCapacity = 20;
    [SerializeField] private int _maxSize = 100;

    private ObjectPool<Tile> _tilePool;

    // Dependencies
    private InventoryManager _inventoryManager;
    private DragManager _dragManager;

    private void Awake()
    {
        // Initialize the pool
        _tilePool = new ObjectPool<Tile>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
    }

    public void Initialize(InventoryManager inventoryManager, DragManager dragManager)
    {
        _inventoryManager = inventoryManager;
        _dragManager = dragManager;
    }

    // --- POOL CALLBACKS ---
    private Tile CreatePooledItem()
    {
        // Instantiate and inject dependencies ONLY ONCE when created
        Tile newTile = Instantiate(_tilePrefab);
        newTile.Initialize(
            ServiceLocator.Get<InventoryManager>(),
            ServiceLocator.Get<DragManager>()
        );
        return newTile;
    }

    private void OnTakeFromPool(Tile tile)
    {
        tile.gameObject.SetActive(true);
        tile.SetRaycastBlocking(true); // Ensure it can be interacted with
    }

    private void OnReturnedToPool(Tile tile)
    {
        tile.Clear(); // Wipe data
        tile.gameObject.SetActive(false);
        tile.SetRaycastBlocking(false); // Prevent interaction while in pool
    }

    private void OnDestroyPoolObject(Tile tile)
    {
        Destroy(tile.gameObject); // Fallback if pool exceeds max size
    }

    // --- PUBLIC API ---
    public void ReturnTileToPool(Tile tile)
    {
        _tilePool.Release(tile);
    }

    public void SpawnItem(ItemDef itemToSpawn, int quantityToSpawn)
    {
        if (itemToSpawn == null) return;

        if (!ServiceLocator.Get<InventoryManager>().HasEmptySlot())
        {
            NotificationBus.PostMessage($"Inventory full! Cannot spawn {itemToSpawn.ItemDisplayName}.");
            return;
        }

        if (itemToSpawn.MaxStackSize < quantityToSpawn) quantityToSpawn = itemToSpawn.MaxStackSize;

        Tile newTile = _tilePool.Get();

        ItemStack debugStack = new ItemStack(itemToSpawn, quantityToSpawn);
        newTile.AssignStack(debugStack);

        ServiceLocator.Get<InventoryManager>().PlaceTileFromSpawn(newTile);
    }

    public Tile SpawnTileFromSplitting(Tile sourceTile, ItemStack stackToAssign, Transform parentTransform)
    {
        Tile newTile = _tilePool.Get(); // Get from pool

        newTile.transform.SetParent(parentTransform);
        newTile.transform.position = sourceTile.transform.position; // Snap visually to the source tile
        newTile.AssignStack(stackToAssign);

        return newTile;
    }

    public Tile SpawnTileFromLoad(ItemStack stackToAssign)
    {
        Tile newTile = _tilePool.Get(); // Get from pool
        newTile.AssignStack(stackToAssign);
        return newTile;
    }
}