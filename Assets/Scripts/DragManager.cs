using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Singleton used to handle dragging of UI elements (Tiles) in the inventory system. It manages the drag lifecycle (start, update, finish) an
/// ensures that dragged items are rendered on top of other UI elements. It also handles dropping items into the trash bin for deletion.
/// </summary>
public class DragManager : MonoBehaviour
{
    public static DragManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private RectTransform _dragLayer;

    private Tile _currentTile;
    private Slot _draggingFrom;
    private Vector2 _offset;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void StartDragging(Tile tile, Slot draggingFrom, PointerEventData eventData)
    {
        _currentTile = tile;
        _draggingFrom = draggingFrom;

        // Move item to the "Drag Layer" so it draws on top of everything
        tile.transform.SetParent(_dragLayer);

        // Calculate Offset (The distance between the Mouse and the Item's center)
        // This ensures the item doesn't "snap" its center to the mouse cursor

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

        // Convert Mouse Screen Position -> World Space UI Position
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragLayer,             // The reference frame
            eventData.position,     // The mouse position
            eventData.pressEventCamera, // The camera that saw the click
            out Vector2 localMousePos)) // The result
        {
            // Apply the new position + the offset we calculated earlier
            _currentTile.transform.localPosition = localMousePos + _offset;
        }
    }

    public void FinishDragging(PointerEventData eventData)
    {
        if (_currentTile == null) return;

        var inventoryManager = InventoryManager.Instance;

        if (IsMouseOverTrash(eventData))
        {
            // If we release tile above trash bin area, destroy it and free slot
            inventoryManager.DestroyTile(_currentTile);
        }
        else
        {
            inventoryManager.PlaceTileFromDrag(_currentTile, _draggingFrom);
        }

        // Get ready to drag another item
        _currentTile = null;
        _draggingFrom = null;
    }

    private bool IsMouseOverTrash(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if we hit the Trash Bin object
            if (result.gameObject.TryGetComponent(out InventoryTrashBin trashBin))
            {
                return true;
            }
        }

        return false;
    }
}