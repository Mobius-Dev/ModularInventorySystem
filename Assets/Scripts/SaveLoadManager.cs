using UnityEngine;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;

public class SaveLoadManager
{
    private InventoryRepository _repository;
    private InventoryManager _inventoryManager;
    public SaveLoadManager(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;

        IJsonFileReader reader = new LocalJsonFileReader();
        IJsonFileWriter writer = new LocalJsonFileWriter();
        _repository = new InventoryRepository(reader, writer);

        if (_repository.FileExists())
        {
            Debug.Log("Inventory data file found. Ready to load.");
        }
    }

    public async Task LoadInventoryDataAsync()
    {
        try
        {
            InventorySaveData data = await _repository.LoadInventoryAsync();

            if (data != null)
            {
                // Tell InventoryManager to reconstruct the inventory based on the loaded data
                _inventoryManager.ReconstructInventory(data);
                NotificationBus.PostMessage("Inventory Loaded Successfully");
            }
            else
            {
                NotificationBus.PostMessage("Failed to load inventory data. No file found or corrupted.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadManager] Critical failure loading inventory: {ex.Message}");
            NotificationBus.PostMessage("CRITICAL ERROR: Save file is corrupted or unreadable.");
        }
    }

    public async Task SaveInventoryDataAsync()
    {
        try
        {
            // Ask the InventoryManager for the current state of the inventory
            InventorySaveData saveData = GenerateSaveData();

            await _repository.SaveInventoryAsync(saveData);

            Debug.Log("Inventory Saved Successfully!");
            NotificationBus.PostMessage("Inventory Saved Successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveLoadManager] Critical failure saving inventory: {ex.Message}");
            NotificationBus.PostMessage("CRITICAL ERROR: Could not save inventory.");
        }
    }

    private InventorySaveData GenerateSaveData()
    {
        InventorySaveData saveData = new InventorySaveData();
        IReadOnlyList<Slot> allSlots = _inventoryManager.AllSlots;

        // Iterate through slots, find the ones with tiles, and create ItemStackData
        for (int i = 0; i < allSlots.Count; i++)
        {
            Slot slot = allSlots[i];

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
}