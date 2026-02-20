using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A singleton containing functionality for spawning game tiles in the environment, allowing instantiation of tiles based on
/// specified item definitions and quantities.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject _tilePrefab; // Prefab used to instantiate new content tiles

    [Header("UI references")]
    [SerializeField] private Button _spawnTileButton; // A button used to spawn a debug tile for testing purposes

    [Header("Tile spawning settings")]
    [SerializeField] private ItemDef _itemToSpawn; // The item that will be spawned when the spawn button is pressed
    [Min(0)]
    [SerializeField] private int _quantityToSpawn = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        if (_spawnTileButton)
        {
            _spawnTileButton.onClick.AddListener(() => SpawnTileFromButton());
        }
    }
    public Tile SpawnTileFromSplitting(GameObject tileObjToClone, ItemStack stackToAssign, Transform parentTransform)
    {
        // Instantiate a new content tile representing the given item stack under the specified parent transform
        GameObject newTileObj = Instantiate(tileObjToClone, parentTransform);
        Tile newTile = newTileObj.GetComponent<Tile>();

        newTile.AssignStack(stackToAssign);

        return newTile;
    }

    public Tile SpawnTileFromLoad(ItemStack stackToAssign)
    {
        GameObject newTileObj = Instantiate(_tilePrefab);
        Tile newTile = newTileObj.GetComponent<Tile>();

        newTile.AssignStack(stackToAssign);

        return newTile;
    }

    private void SpawnTileFromButton()
    {
        // Spawns a debug item tile for testing purposes

        if (_itemToSpawn.MaxStackSize < _quantityToSpawn)
        {
            _quantityToSpawn = _itemToSpawn.MaxStackSize;
            Debug.LogWarning($"Spawning a tile of {_itemToSpawn.ItemID} but requested quantity exceeds max stack size, spawning with max stack size instead", this);
        }

        GameObject newTileObj = Instantiate(_tilePrefab);
        Tile newTile = newTileObj.GetComponent<Tile>();
        ItemStack debugStack = new ItemStack(_itemToSpawn, _quantityToSpawn);

        newTile.AssignStack(debugStack);
        InventoryManager.Instance.PlaceTileFromSpawn(newTile);
    }
}