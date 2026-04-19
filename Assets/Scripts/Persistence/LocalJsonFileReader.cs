using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Defines a contract for asynchronously reading and deserializing JSON files into objects of a specified type.
/// </summary>
public interface IJsonFileReader
{
    Task<T> ReadAsync<T>(string filePath);
}

/// <summary>
/// Provides functionality for asynchronously reading and deserializing JSON data from a local file.
/// </summary>
public class LocalJsonFileReader : IJsonFileReader
{
    public async Task<T> ReadAsync<T>(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogWarning($"[File Reader] Cannot find file at: {filePath}");
            return default; // Return null
        }

        try
        {
            // We use StreamReader inside a 'using' block to ensure the file is closed properly
            using (var reader = new StreamReader(filePath))
            {
                string content = await reader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
        catch (Exception ex)
        {
            // Catch ANY error (Corrupt JSON, Disk failure, etc)
            Debug.LogError($"[File Reader] Error reading {filePath}: {ex.Message}");
            return default;
        }
    }
}