using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _loadDataButton;
    [SerializeField] private Button _saveDataButton;

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
            await ServiceLocator.Get<SaveLoadManager>().LoadInventoryDataAsync();
        }
        finally
        {
            // Guaranteed to unlock the button no matter what happens
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
            await ServiceLocator.Get<SaveLoadManager>().SaveInventoryDataAsync();
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