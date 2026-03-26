using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _clearInventoryButton;

    private void Start()
    {
        if (_clearInventoryButton)
            _clearInventoryButton.onClick.AddListener(OnClearClicked);
    }

    private void OnDestroy()
    {
        if (_clearInventoryButton)
            _clearInventoryButton.onClick.RemoveListener(OnClearClicked);
    }

    private void OnClearClicked()
    {
        ServiceLocator.Get<InventoryManager>().EmptyAllSlots();
    }
}