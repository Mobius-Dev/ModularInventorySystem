using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a draggable inventory tile that displays an item stack and supports drag-and-drop operations for
/// inventory management.
/// </summary>
public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemStack StackStored { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image _image; // Image element to show the item's icon
    [SerializeField] private TextMeshProUGUI _itemCount; // Text element to show the quantity of items in this tile
    [SerializeField] private TextMeshProUGUI _itemName; // Text element to show the name of the item in this tile (optional)

    private void Awake()
    {
        if (_itemCount == null) Debug.LogError($"{gameObject.name} is missing Item Count Text!", this);
        if (_image == null) Debug.LogError($"{gameObject.name} is missing Image Component!", this);
        if (_itemName == null) Debug.LogWarning($"{gameObject.name} is missing Item Name Text! This is optional, but can be useful for debugging.", this);
    }
    private void OnDestroy()
    {
        if (StackStored != null)
        {
            StackStored.OnQuantityChanged -= HandleQuantityChanged;
        }
    }

    public void AssignStack(ItemStack newStack)
    {
        // Unsubscribe from old stack to prevent "Zombie" event calls
        if (StackStored != null)
        {
            StackStored.OnQuantityChanged -= HandleQuantityChanged;
        }

        StackStored = newStack;

        if (StackStored != null)
        {
            // Subscribe
            StackStored.OnQuantityChanged += HandleQuantityChanged;

            // Update Visuals immediately
            _image.sprite = StackStored.ItemStored.Sprite;
            _itemName.text = StackStored.ItemStored.ItemDisplayName;
            HandleQuantityChanged(StackStored.QuantityStored);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Slot lastOccupied = InventoryManager.Instance.GetSlotWithTile(this);

        // Check for Split (Shift + Drag)
        if (InputManager.Instance.IsSplitModifierPressed() &&
            StackManager.Instance.AttemptSplit(this.StackStored, out ItemStack splitStack))
        {
            // Spawn the visual representation of the split stack
            // Note: We pass the parent of the current tile to keep hierarchy clean
            Tile splitTile = SpawnManager.Instance.SpawnTileFromSplitting(
                gameObject,
                splitStack,
                transform.parent);

            DragManager.Instance.StartDragging(splitTile, lastOccupied, eventData);
        }
        else
        {
            // Standard Drag
            InventoryManager.Instance.ReleaseSlotFromTile(this);
            DragManager.Instance.StartDragging(this, lastOccupied, eventData);
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

    private void HandleQuantityChanged(int quantity)
    {
        _itemCount.text = quantity.ToString();
    }
}
