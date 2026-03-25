using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryTrashBin : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out Tile draggedTile))
        {
            ServiceLocator.Get<InventoryManager>().DestroyTile(draggedTile);
        }
    }
}