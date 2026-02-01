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
        //Called by an instance if Slot when it is created to register itself with the manager

        if (!_allSlots.Contains(slot)) _allSlots.Add(slot);
        else Debug.LogWarning($"{slot.gameObject.name} tried to register multiple times!");
    }

    public void PlaceTile(Tile tileToPlace)
    {
        // Identify the closest slot to the dragged content tile and place it there, merging stacks if possible
        // Snap back to last occupied slot if no valid slot found

        Slot closestSlot = _allSlots
            .OrderBy(item => (item.transform.position - tileToPlace.transform.position).sqrMagnitude)
            .ToList()[0]; // Find the slot closest to where the dragged content tile is

        Slot selectedSlot = null;

        if (!closestSlot.TileStored)
        {
            // Closest slot is empty, place the tile here
            // TODO check if the slot is valid for this type of content
            selectedSlot = closestSlot;
        }
        else if (StackManager.Instance.AttemptMerge(closestSlot.TileStored.StackStored, tileToPlace.StackStored))
        {
            // Closest slot is not empty, but tried to merge stacks and succeeded, remove the dragged tile and exit
            closestSlot.TileStored.RefreshTile();
            Destroy(tileToPlace.gameObject);
            return;
        }
        else
        {
            // Closest slot is not empty, merge attempt has failed, fallback to last occupied slot if no valid slot found
            selectedSlot = tileToPlace.LastOccupiedSlot;
        }

        selectedSlot.TileStored = tileToPlace; // Update the slot to store the placed tile
        tileToPlace.LastOccupiedSlot = selectedSlot;
        tileToPlace.transform.SetParent(selectedSlot.transform, false); // Move the content tile to be a child of the slot
        tileToPlace.transform.localPosition = Vector3.zero; // Center the content tile in the slot
    }

    private void Awake()
    {
        // Singleton enforcement

        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
}