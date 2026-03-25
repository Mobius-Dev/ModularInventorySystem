using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button _spawnTileButton;
    [SerializeField] private TMP_Dropdown _itemSelectionDropdown;

    [Header("Settings")]
    [Min(1)]
    [SerializeField] private int _quantityToSpawn = 1;

    // We store the items matching the dropdown index so we know what they selected
    private List<ItemDef> _dropdownItems = new List<ItemDef>();

    private void Start()
    {
        if (_spawnTileButton) _spawnTileButton.onClick.AddListener(OnSpawnButtonClicked);

        PopulateDropdown();
    }

    private void OnDestroy()
    {
        if (_spawnTileButton) _spawnTileButton.onClick.RemoveAllListeners();
    }

    private void PopulateDropdown()
    {
        _itemSelectionDropdown.ClearOptions();
        _dropdownItems.Clear();

        List<TMP_Dropdown.OptionData> uiOptions = new List<TMP_Dropdown.OptionData>();
        var allItems = ServiceLocator.Get<ItemDatabase>().AllItems;

        foreach (var item in allItems)
        {
            uiOptions.Add(new TMP_Dropdown.OptionData(item.ItemDisplayName));
            _dropdownItems.Add(item);
        }

        _itemSelectionDropdown.AddOptions(uiOptions);
    }

    private void OnSpawnButtonClicked()
    {
        int selectedIndex = _itemSelectionDropdown.value;

        if (selectedIndex >= 0 && selectedIndex < _dropdownItems.Count)
        {
            ItemDef selectedItem = _dropdownItems[selectedIndex];

            // Ask the Logic manager to do the actual spawning
            ServiceLocator.Get<SpawnManager>().SpawnItem(selectedItem, _quantityToSpawn);
        }
    }
}