using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    public GameObject _tilePrefab; // Prefab used to instantiate new content tiles

    [Header("UI references")]
    [SerializeField] private Button _spawnTileButton;
    //[SerializeField] private Button _clearInventoryButton; move this to slots?

    [Header("Tile spawning settings")]
    [SerializeField] private ItemDef _itemToSpawn;
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
    public Tile SpawnTileFromSplitting(GameObject tileObjToClone, Stack stackToAssign, Transform parentTransform)
    {
        // Instantiate a new content tile representing the given item stack under the specified parent transform
        GameObject newTileObj = Instantiate(tileObjToClone, parentTransform);
        Tile newTile = newTileObj.GetComponent<Tile>();
        newTile.AssignStack(stackToAssign);
        newTile.Initialize();

        return newTileObj.GetComponent<Tile>();
    }

    private void SpawnTileFromButton()
    {
        // Spawns a debug item tile for testing purposes

        if (_itemToSpawn.MaxStackSize < _quantityToSpawn)
        {
            _quantityToSpawn = _itemToSpawn.MaxStackSize;
            Debug.LogWarning($"Spawning a tile of {_itemToSpawn.DisplayName} but requested quantity exceeds max stack size, spawning with max stack size instead");
        }

        GameObject newTileObj = Instantiate(_tilePrefab);
        Tile newTile = newTileObj.GetComponent<Tile>();
        Stack debugStack = new Stack(_itemToSpawn, _quantityToSpawn);

        newTile.AssignStack(debugStack);
        newTile.Initialize();
        InventoryManager.Instance.PlaceTileFromSpawn(newTile);
    }
}
