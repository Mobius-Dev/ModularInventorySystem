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
        OriginalSlot = ServiceLocator.Get<InventoryManager>().GetSlotWithTile(this);

        // Splitting logic
        if (InputUtility.IsSplitModifierPressed() &&
            StackUtility.AttemptSplit(this.StackStored, out ItemStack splitStack))
        {
            Tile splitTile = ServiceLocator.Get<SpawnManager>().SpawnTileFromSplitting(
                gameObject, splitStack, transform.parent);

            // The split tile's "Original" slot is the one we pulled it from
            splitTile.OriginalSlot = OriginalSlot;

            // Set the event data's pointerDrag to the new split tile, instead of the original that's left in the slot
            eventData.pointerDrag = splitTile.gameObject;

            ServiceLocator.Get<DragManager>().StartDragging(splitTile, eventData);
        }
        else
        {
            ServiceLocator.Get<InventoryManager>().ReleaseSlotFromTile(this);
            ServiceLocator.Get<DragManager>().StartDragging(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        ServiceLocator.Get<DragManager>().UpdatePosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ServiceLocator.Get<DragManager>().FinishDragging(this);
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
