using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _loadDataButton;
    [SerializeField] private Button _saveDataButton;

    // Dependencies
    private SaveLoadManager _saveLoadManager;

    public void Initialize(SaveLoadManager saveLoadManager)
    {
        _saveLoadManager = saveLoadManager;
    }

    private void Start()
    {
        if (_loadDataButton) _loadDataButton.onClick.AddListener(OnLoadClicked);
        if (_saveDataButton) _saveDataButton.onClick.AddListener(OnSaveClicked);
    }

    private void OnDestroy()
    {
        if (_loadDataButton) _loadDataButton.onClick.RemoveListener(OnLoadClicked);
        if (_saveDataButton) _saveDataButton.onClick.RemoveListener(OnSaveClicked);
    }

    private async void OnLoadClicked()
    {
        _loadDataButton.interactable = false;
        try
        {
            await _saveLoadManager.LoadInventoryDataAsync(); // NO MORE SERVICE LOCATOR
        }
        finally
        {
            if (_loadDataButton != null) _loadDataButton.interactable = true;
        }
    }

    private async void OnSaveClicked()
    {
        _saveDataButton.interactable = false;
        try
        {
            await _saveLoadManager.SaveInventoryDataAsync(); // NO MORE SERVICE LOCATOR
        }
        finally
        {
            if (_saveDataButton != null) _saveDataButton.interactable = true;
        }
    }
}