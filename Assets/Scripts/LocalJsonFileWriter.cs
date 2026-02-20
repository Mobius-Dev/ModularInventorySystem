using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public interface IJsonFileWriter
{
    // We use a generic <T> so we can save other data in the future, if we ever want to
    Task WriteAsync<T>(string filePath, T dataToWrite);
}

public class LocalJsonFileWriter : IJsonFileWriter
{
    public async Task WriteAsync<T>(string filePath, T dataToWrite)
    {
        try
        {
            // Serialize the data to a JSON string
            // Formatting.Indented makes it pretty-print (readable)
            string jsonContent = JsonConvert.SerializeObject(dataToWrite, Formatting.Indented);

            // Write to file asynchronously
            // StreamWriter automatically creates the file if it doesn't exist
            using (var writer = new StreamWriter(filePath, false)) // false = overwrite, don't append
            {
                await writer.WriteAsync(jsonContent);
            }

            Debug.Log($"[File Writer] Successfully saved to: {filePath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[File Writer] Failed to save {filePath}: {ex.Message}");
        }
    }
}