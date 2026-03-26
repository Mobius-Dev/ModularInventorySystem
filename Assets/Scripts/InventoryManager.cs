using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class ussed to manage the inventory system as a whole. It keeps track of all the slots in the inventory, and provied methods for placing, merging and removing tiles, and regenerating from save data.
/// It also maintains a dictionary mapping Tiles to their current Slot for O(1) lookup of tile locations.
/// </summary>
public class InventoryManager
{
    private List<Slot> _allSlots = new();
    private Dictionary<Tile, Slot> _tileToSlotMap = new Dictionary<Tile, Slot>();

    public InventoryManager(List<Slot> slots)
    {
        _allSlots = slots;

        foreach (Slot slot in _allSlots)
        {
            // Pass a reference to self to each slot
            slot.Initialize(this);
        }
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
            Debug.LogError($"Could not find a slot containing {tile.name}");
        }
    }

    public Slot GetSlotWithTile(Tile tile)
    {
        // O(1) instantaneous lookup
        if (_tileToSlotMap.TryGetValue(tile, out Slot foundSlot))
        {
            return foundSlot;
        }
        return null;
    }

    public void RegisterTileLocation(Tile tile, Slot slot)
    {
        if (tile == null) return;
        _tileToSlotMap[tile] = slot;
    }

    public void UnregisterTileLocation(Tile tile)
    {
        if (tile == null) return;
        _tileToSlotMap.Remove(tile);
    }

    public void PlaceTileFromSpawn(Tile tileToPlace)
    {
        // Find empty slot; Last instead of First so items start appearing in the scene starting from top-left corner, not bottom-right
        Slot targetSlot = _allSlots.LastOrDefault(slot => slot.TileStored == null);

        if (!targetSlot)
        {
            Debug.LogWarning($"Tried to place spawned tile {tileToPlace.name} into an empty slot but found none!");
            NotificationBus.PostMessage($"No empty slots available for {tileToPlace.StackStored.ItemStored.ItemDisplayName}!");
            UnityEngine.Object.Destroy(tileToPlace.gameObject);
            return;
        }

        targetSlot.TileStored = tileToPlace;
        NotificationBus.PostMessage($"Spawned a new {tileToPlace.StackStored.ItemStored.ItemDisplayName} into slot {targetSlot.name}");
    }

    public void HandleTileDrop(Slot targetSlot, Tile tileToPlace)
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
                Debug.LogError($"Could not fully return tile {tileToPlace.name} to its fallback slot {fallbackSlot.name}. This should never happen!");
                break;
        }
    }

    public void DestroyTile(Tile tileToDestroy)
    {
        if (tileToDestroy)
        {
            UnityEngine.Object.Destroy(tileToDestroy.gameObject);
        }
    }
    public void EmptyAllSlots()
    {
        foreach (Slot slot in _allSlots)
        {
            if (slot.TileStored != null)
            {
                UnityEngine.Object.Destroy(slot.TileStored.gameObject);
                slot.TileStored = null;
            }
        }
        NotificationBus.PostMessage("Emptied all inventory slots");
    }

    public bool HasEmptySlot()
    {
        return _allSlots.Any(slot => slot.TileStored == null);
    }
    public InventorySaveData GenerateSaveData()
    {
        InventorySaveData saveData = new InventorySaveData();

        // Iterate through slots, find the ones with tiles, and create ItemStackData
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

        return saveData;
    }

    public void ReconstructInventory(InventorySaveData data)
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

    private void PlaceTileFromLoad(Tile tileToPlace, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _allSlots.Count)
        {
            Debug.LogError($"Invalid slot index {slotIndex} for placing loaded tile {tileToPlace.name}. This should never happen if the save/load system is working correctly.");
            UnityEngine.Object.Destroy(tileToPlace.gameObject);
            return;
        }

        Slot targetSlot = _allSlots[slotIndex];
        if (targetSlot.TileStored != null)
        {
            Debug.LogError($"Trying to place loaded tile {tileToPlace.name} into slot {targetSlot.name} but it's already occupied by {targetSlot.TileStored.name}. This should never happen if the save/load system is working correctly. Destroying the tile to prevent issues.");
            UnityEngine.Object.Destroy(tileToPlace.gameObject);
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
            UnityEngine.Object.Destroy(targetTile.gameObject);
            return PlacementResult.MergedFully;
        }

        return PlacementResult.MergedPartially;
    }
}