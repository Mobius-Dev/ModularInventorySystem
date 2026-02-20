using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton used to manage the inventory system as a whole. It keeps track of all the slots in the inventory, handles placing and removing tiles from slots
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Button _clearInventoryButton;
    [SerializeField] private Button _loadDataButton; // Button for loading inventory from JSON
    [SerializeField] private Button _saveDataButton; // Button for saving inventory to JSON

    [Header("Database References")]
    [SerializeField] private ItemDatabase _itemDatabase; // Reference to the item database ScriptableObject

    private List<Slot> _allSlots = new();
    private InventoryRepository _repository;

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        if (_clearInventoryButton)
        {
            _clearInventoryButton.onClick.AddListener(() => EmptyAllSlots());
        }
        if (_loadDataButton) {
            _loadDataButton.onClick.AddListener(() => LoadInventoryData());
        }
        if (_saveDataButton) {
            _saveDataButton.onClick.AddListener(() => SaveInventoryData());
        }

        // Dependency Injection (Manual)
        // We create the tools we need.
        IJsonFileReader reader = new LocalJsonFileReader();
        IJsonFileWriter writer = new LocalJsonFileWriter();
        _repository = new InventoryRepository(reader, writer);

        // Check if we have data to load
        // TODO UI integration: For now, it's here so you can see how loading works without needing to create UI for it.
        CheckInventoryDataExists();
    }

    public void RegisterSlot(Slot slot)
    {
        //Called by an instance by Slot when it is created to register itself with the manager
        if (!_allSlots.Contains(slot)) _allSlots.Add(slot);
        else Debug.LogWarning($"{slot.gameObject.name} tried to register multiple times!", this);
    }

    public void ReleaseSlotFromTile(Tile tile)
    {
        //Release a slot holding a given tile
        Slot slotToBeReleased = GetSlotWithTile(tile);

        // Safety Check: Only try to empty the slot if we actually found one
        if (slotToBeReleased != null)
        {
            slotToBeReleased.TileStored = null;
        }
        else
        {
            Debug.LogError($"Could not find a slot containing {tile.name}", this);
        }
    }

    public Slot GetSlotWithTile(Tile tile)
    {
        // Returns a slot holding a given tile
        Slot foundSlot = _allSlots.FirstOrDefault(slot => slot.TileStored == tile);
        return foundSlot;
    }

    public void PlaceTileFromSpawn(Tile tileToPlace)
    {
        // Last instead of first so items start appearing in the scene starting from top-left corner, not bottom-right
        Slot emptySlot = _allSlots.LastOrDefault(slot => slot.TileStored == null);

        if (!emptySlot)
        {
            Debug.LogWarning($"Tried to place spawned tile {tileToPlace.name} into an empty slot but found none!", this);
            Destroy(tileToPlace.gameObject);
            return;
        }

        emptySlot.TileStored = tileToPlace;
    }

    public void PlaceTileFromDrag(Tile tileToPlace, Slot fallbackSlot)
    {
        Slot selectedSlot = GetClosestSlot(tileToPlace);

        PlacementResult placementResult = TryPlaceTileAt(selectedSlot, tileToPlace);

        switch(placementResult)
        {
            case PlacementResult.MergedFully:
                // Tile was fully merged, no need to place anything
                return;
            case PlacementResult.MovedToEmpty:
                selectedSlot.TileStored = tileToPlace;
                break;
            case PlacementResult.MergedPartially:
                // Partial Success: We merged some, but have leftovers.
                // The leftovers must go back to the fallback slot.
                SnapTileBack(tileToPlace, fallbackSlot);
                return;
            case PlacementResult.Failed:
            default:
                // Failure: We couldn't do anything at the target. 
                // Send the whole thing back.
                SnapTileBack(tileToPlace, fallbackSlot);
                return;
        }
    }

    public void DestroyTile(Tile tileToDestroy)
    {
        if (tileToDestroy)
        {
            Destroy(tileToDestroy.gameObject);
        }
    }

    private void SnapTileBack(Tile tileToPlace, Slot fallbackSlot)
    {
        PlacementResult placementResult = TryPlaceTileAt(fallbackSlot, tileToPlace);
        switch (placementResult)
        {
            case PlacementResult.MergedFully:
                // Tile was fully merged, no need to place anything
                return;
            case PlacementResult.MovedToEmpty:
                fallbackSlot.TileStored = tileToPlace;
                break;
            default:
                Debug.LogError($"Could not fully return tile {tileToPlace.name} to its fallback slot {fallbackSlot.name}. This should never happen!", this);
                break;
        }
    }

    private PlacementResult TryPlaceTileAt(Slot targetSlot, Tile targetTile)
    {
        // Handle Empty Slot (Guard Clause)
        if (!targetSlot.TileStored)
        {
            return PlacementResult.MovedToEmpty;
        }

        // Handle Occupied Slot (Attempt Merge)
        if (!StackManager.Instance.AttemptMerge(targetSlot.TileStored.StackStored, targetTile.StackStored))
        {
            return PlacementResult.Failed;
        }

        // Handle Cleanup
        if (targetTile.StackStored.QuantityStored == 0)
        {
            Destroy(targetTile.gameObject);
            return PlacementResult.MergedFully;
        }

        return PlacementResult.MergedPartially;
    }
    private Slot GetClosestSlot(Tile tile)
    {
        Slot closest = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 tilePos = tile.transform.position;

        foreach (Slot slot in _allSlots)
        {
            // Optimization: Use sqrMagnitude to avoid square roots
            float distSqr = (slot.transform.position - tilePos).sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closest = slot;
            }
        }

        return closest;
    }

    private void EmptyAllSlots()
    {
        foreach (Slot slot in _allSlots)
        {
            if (slot.TileStored != null)
            {
                Destroy(slot.TileStored.gameObject);
                slot.TileStored = null;
            }
        }
    }

    private void ReconstructInventory(InventorySaveData data)
    {
        // Clear existing inventory before reconstruction
        EmptyAllSlots();

        // We iterate through the saved item stacks, reconstruct the corresponding ItemStack and Tile for each, and place them in the inventory.
        foreach (var itemData in data.ItemStacks)
        {
            ItemDef realItemDef = _itemDatabase.GetItemByID(itemData.ItemID);

            ItemStack newStack = new ItemStack(realItemDef, itemData.QuantityStored);

            Tile reconstructedTile = SpawnManager.Instance.SpawnTileFromLoad(newStack);

            PlaceTileFromSpawn(reconstructedTile);
        }
    }

    private async void LoadInventoryData()
    {
        Debug.Log("Loading Inventory...");
        InventorySaveData data = await _repository.LoadInventoryAsync();

        if (data != null)
        {
            ReconstructInventory(data);
        }
    }

    public async void SaveInventoryData()
    {
        InventorySaveData saveData = new InventorySaveData();

        // Iterate through slots, find the ones with tiles, and create ItemStackData for each to be saved
        foreach (Slot slot in _allSlots)
        {
            if (slot.TileStored != null)
            {
                ItemStack stack = slot.TileStored.StackStored;


                ItemStackData itemData = new ItemStackData
                {
                    // We save the ID (String), not the ScriptableObject itself!
                    ItemID = stack.ItemStored.ItemID,
                    QuantityStored = stack.QuantityStored
                };

                // Add it to the list to be saved
                saveData.ItemStacks.Add(itemData);
            }
        }

        // Hand off the data to the file system
        await _repository.SaveInventoryAsync(saveData);

        Debug.Log("Inventory Saved Successfully!");
    }

    private void CheckInventoryDataExists()
    {
        if (_repository.FileExists())
        {
            Debug.Log("Inventory data file found. Ready to load inventory.");
        }
        else
        {
            Debug.LogWarning("No inventory data file found.");
        }
    }
}