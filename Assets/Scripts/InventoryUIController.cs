using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _clearInventoryButton;
    [SerializeField] private Button _loadDataButton;
    [SerializeField] private Button _saveDataButton;

    private void Start()
    {
        if (_clearInventoryButton) _clearInventoryButton.onClick.AddListener(OnClearClicked);
        if (_loadDataButton) _loadDataButton.onClick.AddListener(OnLoadClicked);
        if (_saveDataButton) _saveDataButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnDestroy()
    {
        if (_clearInventoryButton) _clearInventoryButton.onClick.RemoveAllListeners();
        if (_loadDataButton) _loadDataButton.onClick.RemoveAllListeners();
        if (_saveDataButton) _saveDataButton.onClick.RemoveAllListeners();
    }

    private void OnClearClicked()
    {
        ServiceLocator.Get<InventoryManager>().EmptyAllSlots();
    }

    private async void OnLoadClicked()
    {
        _loadDataButton.interactable = false;

        try
        {
            await ServiceLocator.Get<InventoryManager>().LoadInventoryDataAsync();
        }
        finally
        {
            // Guaranteed to unlock the button no matter what happens during the load operation
            if (_loadDataButton != null)
            {
                _loadDataButton.interactable = true;
            }
        }
    }

    private async void OnSaveClicked()
    {
        _saveDataButton.interactable = false;

        try
        {
            await ServiceLocator.Get<InventoryManager>().SaveInventoryDataAsync();
        }
        finally
        {
            if (_saveDataButton != null)
            {
                _saveDataButton.interactable = true;
            }
        }
    }
}