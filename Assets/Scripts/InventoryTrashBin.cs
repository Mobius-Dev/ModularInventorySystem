using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryTrashBin : MonoBehaviour, IDropHandler
{
    // Dependencies
    private InventoryManager _inventoryManager;

    public void Initialize(InventoryManager inventoryManager)
    {
        _inventoryManager = inventoryManager;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out Tile draggedTile))
        {
            _inventoryManager.DestroyTile(draggedTile);
        }
    }
}