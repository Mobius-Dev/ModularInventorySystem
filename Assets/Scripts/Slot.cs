using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Represents a single slot in the inventory system. Each slot can hold one tile, and is responsible for centering the tile within itself when a tile is assigned to it.
/// </summary>
public class Slot : MonoBehaviour, IDropHandler
{
    private Tile _tileStored;

    public Tile TileStored
    {
        get => _tileStored;
        set
        {
            // If this slot already held a tile, and we are changing it, unregister the OLD tile
            if (_tileStored != null && _tileStored != value)
            {
                ServiceLocator.Get<InventoryManager>().UnregisterTileLocation(_tileStored);
            }

            // Update the backing field
            _tileStored = value;

            // If we are receiving a NEW tile, snap it and register it
            if (_tileStored != null)
            {
                _tileStored.transform.SetParent(this.transform);
                _tileStored.transform.localPosition = Vector3.zero; // Snap!

                ServiceLocator.Get<InventoryManager>().RegisterTileLocation(_tileStored, this);
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Check if the thing dropped was actually a Tile
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out Tile draggedTile))
        {
            // Tell the InventoryManager to attempt placing it exactly here
            ServiceLocator.Get<InventoryManager>().HandleTileDrop(this, draggedTile);
        }
    }
}