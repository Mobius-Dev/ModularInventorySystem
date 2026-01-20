using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
public class Content : MonoBehaviour
{
    // This class handles either a single item, or a stack of items, and its visual representation in the inventory as a Tile       

    //public Item what item is stored here
    //public int how many of that item is stored here

    [SerializeField] private Slot _lastOccupied; //If dragging we need to remember where the tile came from in case no new valid slot for this item is found
    // TODO remove hack this shouldnt ever be set manually
    private Vector3 _dragOffset;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (!_lastOccupied) Debug.LogError($"{gameObject.name} is a stray content tile, this is not allowed!");
    }

    private void OnMouseDown()
    {
        _dragOffset = transform.position - GetMouseWorldPosition();
        gameObject.transform.SetParent(null);
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPosition() + _dragOffset;
    }

    private void OnMouseUp()
    {
        _lastOccupied.ReleaseSlot();

        Slot selectedSlot = SlotsManager.Instance.SelectBestSlot(this); //Find the slot most valid for this content tile
        if (!selectedSlot) selectedSlot = _lastOccupied; //if there are no valid ones, instead snap back to previously occupied slot

        selectedSlot.OccupySlot(this);
        gameObject.transform.SetParent(selectedSlot.transform);
        gameObject.transform.localPosition = Vector3.zero;
        _lastOccupied = selectedSlot;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        //Keep the depth of the object unchanged

        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);

        return _mainCamera.ScreenToWorldPoint(mousePoint);
    }
}
