using UnityEngine;

public class Slot : MonoBehaviour
{
    // This class handles an slot in the inventory which can store a tile representing a Stack of items

    public Tile TileStored;

    // TODO requirements, i.e this slot only accepts weapon-type Content
    private void Start()
    {
        SlotManager.Instance.RegisterSlot(this);
    }
}