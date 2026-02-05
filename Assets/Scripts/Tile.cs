using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // This class handles visual representation of an item stack in the inventory UI
    public Stack StackStored { get; private set; }

    public Slot LastOccupiedSlot;
    // If dragging we need to remember where the tile came from in case no new valid slot for this item is found
    // TODO remove hack this shouldnt ever be set manually

    [SerializeField] private Image _image; //Image element to show the item's icon
    [SerializeField] private TextMeshProUGUI _itemCount; //Text element to show the quantity of items in this tile

    [Header("Debug")]
    [SerializeField] ItemDef _itemDef;
    [Header("Force count")]
    [Tooltip("Sets this to positive value to enforce a given item count on awake")]
    [SerializeField] private int _forceCount = -1;

    private void Awake()
    {
        if (!_itemCount) Debug.LogError(($"{gameObject.name} is missing its item count text element!"));
        if (!_image) Debug.LogError(($"{gameObject.name} is missing its image element!"));

        Initialize();
    }

    public void Initialize()
    {
        if (StackStored == null) // TODO Remove Hack
        {
            AssignStack(new Stack(_itemDef, 1));
        }
        if (_forceCount > 0)
        {
            StackStored.QuantityStored = _forceCount;
            _forceCount = -1; // TODO remove hack
        }

        _image.sprite = StackStored.ItemStored.Sprite;
    }

    public void AssignStack(Stack newStack)
    {
        // Unsubscribe from old stack to prevent memory leaks/bugs
        if (StackStored != null)
            StackStored.OnQuantityChanged -= RefreshText;

        // Assign new stack and subscribe to its events
        StackStored = newStack;

        if (StackStored != null)
        {
            StackStored.OnQuantityChanged += RefreshText;

            // Update immediately for the first time
            RefreshText(StackStored.QuantityStored);
        }
        else
        {
            Debug.LogError("Tried to assign a null stack!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Slot _lastOccupied = InventoryManager.Instance.GetSlotWithTile(this);

        if (InputManager.Instance.IsSplitModifierPressed() && StackManager.Instance.AttemptSplit(this.StackStored, out Stack splitStack))
        {
            Tile splitTile = SpawnManager.Instance.SpawnTileFromSplitting(this.gameObject, splitStack, this.transform.parent);
            DragManager.Instance.StartDragging(splitTile, _lastOccupied, eventData);
        }
        else
        {
            InventoryManager.Instance.ReleaseSlotFromTile(this);
            DragManager.Instance.StartDragging(this, _lastOccupied, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragManager.Instance.UpdatePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragManager.Instance.FinishDragging(eventData);
    }

    private void RefreshText(int quantity)
    {
        _itemCount.text = quantity.ToString();
    }
}
