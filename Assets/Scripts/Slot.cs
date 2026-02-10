using UnityEngine;

/// <summary>
/// Represents a single slot in the inventory system. Each slot can hold one tile, and is responsible for centering the tile within itself when a tile is assigned to it.
/// </summary>
public class Slot : MonoBehaviour
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
    private void Start()
    {
        InventoryManager.Instance.RegisterSlot(this);
    }
}