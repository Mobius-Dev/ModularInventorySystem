using UnityEngine;

/// <summary>
/// A factory class containing functionality for spawning game tiles in the environment.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject _tilePrefab;

    public void SpawnItem(ItemDef itemToSpawn, int quantityToSpawn)
    {
        if (itemToSpawn == null)
        {
            Debug.LogWarning("Spawning a tile but no item selected.");
            return;
        }

        if (itemToSpawn.MaxStackSize < quantityToSpawn)
        {
            quantityToSpawn = itemToSpawn.MaxStackSize;
            Debug.LogWarning($"Requested quantity exceeds max stack size. Spawning {quantityToSpawn} instead.");
        }

        GameObject newTileObj = Instantiate(_tilePrefab);
        Tile newTile = newTileObj.GetComponent<Tile>();
        ItemStack debugStack = new ItemStack(itemToSpawn, quantityToSpawn);

        newTile.AssignStack(debugStack);

        ServiceLocator.Get<InventoryManager>().PlaceTileFromSpawn(newTile);
    }

    public Tile SpawnTileFromSplitting(GameObject tileObjToClone, ItemStack stackToAssign, Transform parentTransform)
    {
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
}