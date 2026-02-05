using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragManager : MonoBehaviour
{
    // A singleton which is used to handle dragging of UI elements
    public static DragManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private RectTransform _dragLayer;

    private Tile _currentTile;
    private Slot _draggingFrom;
    private Transform _originalParent;
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
        _originalParent = tile.transform.parent;

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

        if (IsMouseOverTrash(eventData))
        {
            // If we release tile above trash bin area, destroy it and free slot
            InventoryManager.Instance.DestroyTile(_currentTile);
        }
        else
        {
            InventoryManager.Instance.PlaceTileFromDrag(_currentTile, _draggingFrom);
        }

        _currentTile = null; 
        _draggingFrom = null; // Ready to drag another item
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
