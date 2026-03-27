using UnityEngine;
using UnityEngine.EventSystems;

public class DragManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private RectTransform _dragLayer;

    private Tile _currentTile;
    private Vector2 _offset;

    public void HandleDragStart(Tile sourceTile, PointerEventData eventData)
    {
        // Splitting logic
        if (ServiceLocator.Get<InputManager>().IsSplitModifierPressed())
        {
            if (StackUtility.AttemptSplit(sourceTile.StackStored, out ItemStack splitStack))
            {
                NotificationBus.PostMessage($"Split stack into {sourceTile.StackStored.QuantityStored} and {splitStack.QuantityStored}.");

                // Ask SpawnManager to create the new half
                Tile splitTile = ServiceLocator.Get<SpawnManager>().SpawnTileFromSplitting(
                    sourceTile, splitStack, sourceTile.transform.parent);

                splitTile.OriginalSlot = sourceTile.OriginalSlot;

                // Tell the Unity Event System that we are now dragging the NEW tile, not the old one
                eventData.pointerDrag = splitTile.gameObject;

                StartDragging(splitTile, eventData);
                return;
            }
            else
            {
                NotificationBus.PostMessage($"Cannot split {sourceTile.StackStored.ItemStored.ItemDisplayName}. Only 1 item left.");
                // If we can't split, we just fall through and drag the whole tile normally
            }
        }

        // Normal drag logic (if no split modifier, or if splitting failed)
        ServiceLocator.Get<InventoryManager>().ReleaseSlotFromTile(sourceTile);
        StartDragging(sourceTile, eventData);
    }

    private void StartDragging(Tile tile, PointerEventData eventData)
    {
        _currentTile = tile;

        // Let the mouse click pass through the tile so the Slots can detect the Drop
        _currentTile.SetRaycastBlocking(false);

        tile.transform.SetParent(_dragLayer);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragLayer,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePos
        );

        _offset = (Vector2)tile.transform.localPosition - localMousePos;
    }

    public void UpdatePosition(PointerEventData eventData)
    {
        if (_currentTile == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragLayer, eventData.position, eventData.pressEventCamera, out Vector2 localMousePos))
        {
            _currentTile.transform.localPosition = localMousePos + _offset;
        }
    }

    public void FinishDragging(Tile tile)
    {
        if (tile == null) return;

        // Turn raycasts back on so the tile can be clicked again later
        tile.SetRaycastBlocking(true);

        // If the tile is still on the _dragLayer, it means the user dropped it in empty space
        if (tile.transform.parent == _dragLayer)
        {
            ServiceLocator.Get<InventoryManager>().SnapTileBack(tile, tile.OriginalSlot);
        }

        _currentTile = null;
    }
}