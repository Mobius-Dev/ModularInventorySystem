using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a draggable inventory tile that displays an item stack and supports drag-and-drop operations for
/// inventory management.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class Tile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemStack StackStored { get; private set; }

    public Slot OriginalSlot { get; set; }

    [Header("UI References")]
    [SerializeField] private Image _image; // Image element to show the item's icon
    [SerializeField] private TextMeshProUGUI _itemCount; // Text element to show the quantity of items in this tile
    [SerializeField] private TextMeshProUGUI _itemName; // Text element to show the name of the item in this tile (optional)

    // Dependency Fields
    private InventoryManager _inventoryManager;
    private DragManager _dragManager;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
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

    public void Initialize(InventoryManager inventoryManager, DragManager dragManager)
    {
        _inventoryManager = inventoryManager;
        _dragManager = dragManager;
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
        OriginalSlot = _inventoryManager.GetSlotWithTile(this);

        _dragManager.HandleDragStart(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _dragManager.UpdatePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragManager.FinishDragging(this);
    }

    public void SetRaycastBlocking(bool blocks)
    {
        _canvasGroup.blocksRaycasts = blocks;
    }

    public void Clear()
    {
        // Unsubscribe to prevent memory leaks and zombie events
        if (StackStored != null)
        {
            StackStored.OnQuantityChanged -= HandleQuantityChanged;
            StackStored = null;
        }

        OriginalSlot = null;

        // Reset visuals to blank/default
        _image.sprite = null;
        _itemName.text = "";
        _itemCount.text = "";
    }

    private void HandleQuantityChanged(int quantity)
    {
        _itemCount.text = quantity.ToString();
    }
}
