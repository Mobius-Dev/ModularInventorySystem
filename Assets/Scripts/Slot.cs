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
            _tileStored = value;

            // Whenever a slot is given a tile, center the tile in the slot by making it a child and snapping its position
            if (_tileStored != null)
            {
                _tileStored.transform.SetParent(this.transform);
                _tileStored.transform.localPosition = Vector3.zero; // Snap!
            }
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Check if the thing dropped was actually a Tile
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out Tile draggedTile))
        {
            // Tell the InventoryManager to attempt placing it exactly here
            ServiceLocator.Get<InventoryManager>().HandleTileDrop(draggedTile, this);
        }
    }
}