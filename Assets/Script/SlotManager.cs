using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    // A singleton which holds knowledge of and runs operations on all inventory slots
    public static SlotManager Instance { get; private set; }

    [SerializeField] private List<Slot> _allSlots = new();
    
    public void RegisterSlot(Slot slot)
    {
        //Called by an empty slot at awake to register itself

        if (!_allSlots.Contains(slot)) _allSlots.Add(slot);
        else Debug.LogWarning($"{slot.gameObject.name} tried to register multiple times!");
    }
    public Slot SelectBestSlot(Content content)
    {
        // Find and return the best suited slot for given content

        Slot neareastSlot = _allSlots
            .OrderBy(item => (item.transform.position - content.transform.position).sqrMagnitude)
            .ToList()[0]; // Find the slot nearest to where the dragged content tile is

        if (ValidateSlot(neareastSlot, content)) return neareastSlot; // Validate this slot before returning it
        else return null;
    }

    private void Awake()
    {
        // Singleton enforcement

        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private bool ValidateSlot(Slot slot, Content contentToValidate)
    {
        Content contentInSlot = slot.ReadContent;

        if (contentInSlot)
        {
            // Slot is occupied, check if stacking is possible

            if (contentInSlot.ItemStored.ItemID == contentToValidate.ItemStored.ItemID
                && contentInSlot.QuantityStored + contentToValidate.QuantityStored < contentInSlot.ItemStored.MaxStackSize)
            {
                // Same item and merge won't exceed max stack size, valid for stacking
                return true;
            }

            else return false;
        }

        // TODO implement checks for special slot types (e.g. equipment slots)

        else return true; // Slot is empty, valid
    }
}