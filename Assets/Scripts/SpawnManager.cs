using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TMP_Dropdown _itemSelectionDropdown; // A dropdown used to select which item to spawn when the spawn button is pressed

    [Header("Tile spawning settings")]
    [Min(0)]
    [SerializeField] private int _quantityToSpawn = 1; // How many items to spawn, default 1
    private ItemDef _itemToSpawn; // The item that will be spawned when the spawn button is pressed (controlled via dropdown)
    private Dictionary<string, string> _itemNameToID = new Dictionary<string, string>(); // A hidden lookup to convert the human readable item names back to their IDs

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        if (_spawnTileButton) _spawnTileButton.onClick.AddListener(() => SpawnTileFromButton());
        else Debug.LogError("SpawnManager is missing a reference to the Spawn Tile Button, spawning via button will not work", this);

        if (_itemSelectionDropdown)
        {
            _itemSelectionDropdown.onValueChanged.AddListener((index) => OnItemSelected(index));
            PopulateItemSelectionDropdown();
        }
        else Debug.LogError("SpawnManager is missing a reference to the Item Selection Dropdown, spawning via button will not work", this);
        
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

        if (_itemToSpawn == null)
        {
            Debug.LogWarning("Spawning a tile but no item selected, spawning will not work", this);
            return;
        }

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

    private void OnItemSelected(int index)
    {
        string selectedItemName = _itemSelectionDropdown.options[index].text;
        SetItemToSpawn(selectedItemName);
    }

    private void SetItemToSpawn(string itemName)
    {
        if (_itemNameToID.TryGetValue(itemName, out string itemID))
        {
            _itemToSpawn = GameManager.Instance.ItemDatabase.GetItemByID(itemID);
        }
        else
        {
            Debug.LogWarning($"Item name '{itemName}' not found in the ID lookup dictionary, spawning this item won't work");
        }
    }

    private void PopulateItemSelectionDropdown()
    {
        _itemSelectionDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> uiOptions = new List<TMP_Dropdown.OptionData>();

        foreach (var entry in GameManager.Instance.ItemDatabase.AllItems)
        {
            // Add the readable name to the UI list
            string name = entry.ItemDisplayName;
            string id = entry.ItemID;

            uiOptions.Add(new TMP_Dropdown.OptionData(name));

            // Add the name/id pair to our hidden lookup list
            _itemNameToID.Add(name, id);
        }

        _itemSelectionDropdown.AddOptions(uiOptions);

        if (uiOptions.Count > 0)
        {
            // When options assigned, it defaults to 0th item in the UI, make sure our class knows about this
            string defaultItemName = _itemSelectionDropdown.options[0].text;
            SetItemToSpawn(defaultItemName);
        }
        else
        {
            Debug.LogWarning("No options were found to add to spawn dropdown, is the database empty?", this);
        }
    }
}