using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Provides methods for loading and saving inventory data to a JSON file using an injected file reader.
/// </summary>
/// <remarks>The inventory data is stored in a file named "inventory_data.json" located in the application's
/// persistent data path. If the inventory file does not exist or is empty, loading returns a new, empty inventory. This
/// class abstracts file operations so that consumers do not need to manage file access directly.</remarks>
public class InventoryRepository
{
    private readonly IJsonFileReader _fileReader;
    private readonly string _saveFileName = "inventory_data.json";

    public InventoryRepository(IJsonFileReader fileReader)
    {
        _fileReader = fileReader;
    }

    public async Task<InventorySaveData> LoadInventoryAsync()
    {
        // Get the full path to the inventory save file
        string path = GetPath();

        var data = await _fileReader.ReadAsync<InventorySaveData>(path);
        return data;
    }

    public bool FileExists()
    {
        return File.Exists(GetPath());
    }

    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, _saveFileName);
    }
}