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
    private SpawnManager _spawnManager;

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

    public void Initialize(InventoryManager inventoryManager, DragManager dragManager, SpawnManager spawnManager)
    {
        _inventoryManager = inventoryManager;
        _dragManager = dragManager;
        _spawnManager = spawnManager;
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

        // Splitting logic
        if (InputUtility.IsSplitModifierPressed())
        {
            if (StackUtility.AttemptSplit(this.StackStored, out ItemStack splitStack))
            {
                NotificationBus.PostMessage($"Split stack into {this.StackStored.QuantityStored} and {splitStack.QuantityStored}.");

                Tile splitTile = _spawnManager.SpawnTileFromSplitting(
                    gameObject, splitStack, transform.parent);

                splitTile.OriginalSlot = OriginalSlot;
                eventData.pointerDrag = splitTile.gameObject;
                _dragManager.StartDragging(splitTile, eventData);
            }
            else
            {
                NotificationBus.PostMessage($"Cannot split {this.StackStored.ItemStored.ItemDisplayName}. Only 1 item left.");
            }
        }
        else
        {
            // Normal drag logic
            _inventoryManager.ReleaseSlotFromTile(this);
            _dragManager.StartDragging(this, eventData);
        }
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

    private void HandleQuantityChanged(int quantity)
    {
        _itemCount.text = quantity.ToString();
    }
}
