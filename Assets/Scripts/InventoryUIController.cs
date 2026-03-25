using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all UI interactions for the inventory system, completely separated from core logic.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _clearInventoryButton;
    [SerializeField] private Button _loadDataButton;
    [SerializeField] private Button _saveDataButton;

    private void Start()
    {
        if (_clearInventoryButton)
        {
            _clearInventoryButton.onClick.AddListener(() =>
                ServiceLocator.Get<InventoryManager>().EmptyAllSlots());
        }

        if (_loadDataButton)
        {
            _loadDataButton.onClick.AddListener(() =>
                ServiceLocator.Get<InventoryManager>().LoadInventoryData());
        }

        if (_saveDataButton)
        {
            _saveDataButton.onClick.AddListener(() =>
                ServiceLocator.Get<InventoryManager>().SaveInventoryData());
        }
    }
    private void OnDestroy()
    {
        if (_clearInventoryButton) _clearInventoryButton.onClick.RemoveAllListeners();
        if (_loadDataButton) _loadDataButton.onClick.RemoveAllListeners();
        if (_saveDataButton) _saveDataButton.onClick.RemoveAllListeners();
    }
}