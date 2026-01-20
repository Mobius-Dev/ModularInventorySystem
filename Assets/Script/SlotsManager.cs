using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotsManager : MonoBehaviour
{
    // A singleton which holds knowledge of and runs operations on all inventory slots

    [SerializeField] private float _maxDistanceForSlotProximity = 0;
    [SerializeField] private List<Slot> _allSlots = new();
    public static SlotsManager Instance { get; private set; }
    public void RegisterSlot(Slot slot)
    {
        //Called by an empty slot at awake to register itself

        if (!_allSlots.Contains(slot)) _allSlots.Add(slot);
        else Debug.LogWarning($"{slot.gameObject.name} tried to register multiple times!");
    }
    public Slot SelectBestSlot(Content content)
    {
        // Find and return the best suited slot for given content

        // Create a sorted list of slots to determine the order of checking their validity
        // TODO in the future, if the slot directly under the conten tile isn't valid, then prioritize nearest stacking possibility rather than an empty slot

        List<Slot> sortedSlots = CreateSortedList(content.transform.position);
        Debug.Log(sortedSlots.Count.ToString());

        foreach (Slot slot in sortedSlots)
        {
            if (ValidateSlot(slot, content))
            {
                return slot;
            }
        }

        return null;
    }

    private void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private List<Slot> CreateSortedList(Vector3 coordsToSortBy)
    {
        List<Slot> sortedSlots = _allSlots
            .OrderBy(item => (item.transform.position - coordsToSortBy).sqrMagnitude) // We use squared magnitude rather than distance as an optimization
            .ToList();

        return sortedSlots;
    }

    private bool ValidateSlot(Slot slot, Content content)
    {
        // For now just check if slot is empty no other requirements

        if (slot.ReadContent) return false; // Slot occupied, disqualify
        // else check for other requirements, none for now
        else return true;
    }
}