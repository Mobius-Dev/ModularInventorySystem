using UnityEngine;
using System;
using System.Threading.Tasks;

public class SaveLoadManager
{
    private InventoryRepository _repository;

    public SaveLoadManager()
    {
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
                // Tell InventoryManager to do its thing with the loaded data
                ServiceLocator.Get<InventoryManager>().ReconstructInventory(data);
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
            InventorySaveData saveData = ServiceLocator.Get<InventoryManager>().GenerateSaveData();

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
}