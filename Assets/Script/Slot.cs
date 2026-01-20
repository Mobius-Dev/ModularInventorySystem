using UnityEngine;

public class Slot : MonoBehaviour
{
    // This class handles an slot in the inventory which can store a tile representing a Content
    private BoxCollider2D _collider; // TODO is this really necessary? i.e do we ever need to click an empty slot and have it react
    [SerializeField] private Content _content;

    public Content ReadContent => _content;

    // TODO requirements, i.e this slot only accepts weapon-type Content
    public void OccupySlot(Content content)
    {
        if (_content)
        {
            Debug.LogError($"Slot {gameObject.name} already occupied!"); // TODO proper error handling
            return;
        }

        _content = content;
        _collider.enabled = false;
    }

    public void ReleaseSlot()
    {
        _content = null;
        _collider.enabled = true;
    }

    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();

        SlotsManager.Instance.RegisterSlot(this);
    }
}