using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton used to manage the inventory system as a whole. It keeps track of all the slots in the inventory, handles placing and removing tiles from slots
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<Slot> _allSlots = new();
    private InventoryRepository _repository;

    private void Awake()
    {
        // Dependency Injection (Manual)
        // We create the tools we need.
        IJsonFileReader reader = new LocalJsonFileReader();
        IJsonFileWriter writer = new LocalJsonFileWriter();
        _repository = new InventoryRepository(reader, writer);

        CheckInventoryDataExists();
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
            NotificationBus.PostMessage($"No empty slots available for {tileToPlace.StackStored.ItemStored.ItemDisplayName}!");
            Destroy(tileToPlace.gameObject);
            return;
        }

        emptySlot.TileStored = tileToPlace;
        NotificationBus.PostMessage($"Spawned a new {tileToPlace.StackStored.ItemStored.ItemDisplayName} into slot {emptySlot.name}");
    }

    public void HandleTileDrop(Tile tileToPlace, Slot targetSlot)
    {
        PlacementResult placementResult = TryPlaceTileAt(targetSlot, tileToPlace);
        string itemName = tileToPlace.StackStored.ItemStored.ItemDisplayName;

        switch (placementResult)
        {
            case PlacementResult.MergedFully:
                NotificationBus.PostMessage($"Fully merged {itemName} into {targetSlot.name}.");
                return;

            case PlacementResult.MovedToEmpty:
                targetSlot.TileStored = tileToPlace;
                NotificationBus.PostMessage($"Placed {itemName} into {targetSlot.name}.");
                break;

            case PlacementResult.MergedPartially:
                NotificationBus.PostMessage($"Partially merged {itemName}. Leftovers returned.");
                SnapTileBack(tileToPlace, tileToPlace.OriginalSlot);
                return;

            case PlacementResult.Failed:
            default:
                NotificationBus.PostMessage($"Could not place {itemName} in {targetSlot.name}.");
                SnapTileBack(tileToPlace, tileToPlace.OriginalSlot);
                break;
        }
    }

    public void SnapTileBack(Tile tileToPlace, Slot fallbackSlot)
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

    public void DestroyTile(Tile tileToDestroy)
    {
        if (tileToDestroy)
        {
            Destroy(tileToDestroy.gameObject);
        }
    }
    public void EmptyAllSlots()
    {
        foreach (Slot slot in _allSlots)
        {
            if (slot.TileStored != null)
            {
                Destroy(slot.TileStored.gameObject);
                slot.TileStored = null;
            }
        }
        NotificationBus.PostMessage("Emptied all inventory slots");
    }

    public async Task LoadInventoryDataAsync()
    {
        Debug.Log("Loading Inventory...");

        try
        {
            InventorySaveData data = await _repository.LoadInventoryAsync();

            if (data != null)
            {
                ReconstructInventory(data);
                NotificationBus.PostMessage("Inventory Loaded Successfully");
            }
            else
            {
                NotificationBus.PostMessage("Failed to load inventory data. No file found or file was corrupted.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InventoryManager] Critical failure loading inventory: {ex.Message}");
            NotificationBus.PostMessage("CRITICAL ERROR: Save file is corrupted or unreadable.");
        }
    }

    public async Task SaveInventoryDataAsync()
    {
        try
        {
            InventorySaveData saveData = new InventorySaveData();

            // Iterate through slots, find the ones with tiles, and create ItemStackData for each to be saved
            for (int i = 0; i < _allSlots.Count; i++)
            {
                Slot slot = _allSlots[i];

                if (slot.TileStored != null)
                {
                    ItemStack stack = slot.TileStored.StackStored;

                    ItemStackData itemData = new ItemStackData
                    {
                        ItemID = stack.ItemStored.ItemID,
                        SlotIndex = i,
                        QuantityStored = stack.QuantityStored
                    };

                    saveData.ItemStacks.Add(itemData);
                }
            }

            await _repository.SaveInventoryAsync(saveData);

            Debug.Log("Inventory Saved Successfully!");
            NotificationBus.PostMessage("Inventory Saved Successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[InventoryManager] Critical failure saving inventory: {ex.Message}");
            NotificationBus.PostMessage("CRITICAL ERROR: Could not save inventory.");
        }
    }

    private void PlaceTileFromLoad(Tile tileToPlace, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _allSlots.Count)
        {
            Debug.LogError($"Invalid slot index {slotIndex} for placing loaded tile {tileToPlace.name}. This should never happen if the save/load system is working correctly.", this);
            Destroy(tileToPlace.gameObject);
            return;
        }

        Slot targetSlot = _allSlots[slotIndex];
        if (targetSlot.TileStored != null)
        {
            Debug.LogError($"Trying to place loaded tile {tileToPlace.name} into slot {targetSlot.name} but it's already occupied by {targetSlot.TileStored.name}. This should never happen if the save/load system is working correctly. Destroying the tile to prevent issues.", this);
            Destroy(tileToPlace.gameObject);
            return;
        }

        targetSlot.TileStored = tileToPlace;
    }

    private PlacementResult TryPlaceTileAt(Slot targetSlot, Tile targetTile)
    {
        // Handle Empty Slot (Guard Clause)
        if (!targetSlot.TileStored)
        {
            return PlacementResult.MovedToEmpty;
        }

        // Handle Occupied Slot (Attempt Merge)
        if (!StackUtility.AttemptMerge(targetSlot.TileStored.StackStored, targetTile.StackStored))
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
    private void ReconstructInventory(InventorySaveData data)
    {
        // Clear existing inventory before reconstruction
        EmptyAllSlots();

        // We iterate through the saved item stacks, reconstruct the corresponding ItemStack and Tile for each, and place them in the inventory.
        foreach (var itemData in data.ItemStacks)
        {
            ItemDef realItemDef = ServiceLocator.Get<ItemDatabase>().GetItemByID(itemData.ItemID);

            ItemStack newStack = new ItemStack(realItemDef, itemData.QuantityStored);

            Tile reconstructedTile = ServiceLocator.Get<SpawnManager>().SpawnTileFromLoad(newStack);

            PlaceTileFromLoad(reconstructedTile, itemData.SlotIndex);
        }
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

#if UNITY_EDITOR
    [ContextMenu("Find And Register All Slots")]
    private void SetupSlotsFromEditor()
    {
        var foundSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);

        _allSlots = foundSlots.OrderBy(s => s.transform.GetSiblingIndex()).Reverse().ToList();

        UnityEditor.EditorUtility.SetDirty(this);

        Debug.Log($"Successfully found and registered {_allSlots.Count} Slots!", this);
    }
#endif
}