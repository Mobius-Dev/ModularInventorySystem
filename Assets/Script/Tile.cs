using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // This class handles visual representation of an item stack in the inventory UI

    public Stack StackStored { get; private set; }

    public Slot LastOccupiedSlot;
    // If dragging we need to remember where the tile came from in case no new valid slot for this item is found
    // TODO remove hack this shouldnt ever be set manually

    [SerializeField] private TextMeshProUGUI _itemCount; //Text element to show the quantity of items in this tile

    [UnitHeaderInspectable("Debug")]
    [SerializeField] ItemDef _itemDef; // TODO Hack

    private void Awake()
    {
        if (!LastOccupiedSlot) Debug.LogError($"{gameObject.name} is a stray content tile, this is not allowed!");
        if (!_itemCount) Debug.LogError(($"{gameObject.name} is missing its item count text element!"));

        // TODO Hack
        if (StackStored == null)
        {
            StackStored = new Stack(_itemDef, 1);
        }

        RefreshTile();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragManager.Instance.StartDragging(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instance.UpdatePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragManager.Instance.FinishDragging(eventData);
    }

    public void RefreshTile()
    {
        //Update the visual representation of this tile based on its current data

        _itemCount.text = StackStored.QuantityStored.ToString();
    }
}
