using UnityEngine;

public class Slot : MonoBehaviour
{
    // This class handles an slot in the inventory which can store a tile representing a Stack of items

    private Tile _tileStored;

    public Tile TileStored
    {
        get => _tileStored;
        set
        {
            _tileStored = value;

            // Whenever a slot is given a tile, the SLOT ensures the tile behaves
            if (_tileStored != null)
            {
                _tileStored.transform.SetParent(this.transform);
                _tileStored.transform.localPosition = Vector3.zero; // Snap!
                // Note: We don't need to refresh the text, because the Tile handles that itself now!
            }
        }
    }

    // TODO requirements, i.e this slot only accepts weapon-type Content
    private void Awake()
    {
        InventoryManager.Instance.RegisterSlot(this);
    }
}