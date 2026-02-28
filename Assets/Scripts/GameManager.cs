using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private ItemDatabase _itemDatabase;
    public ItemDatabase ItemDatabase
    {
        get
        {
            if (_itemDatabase == null)
            {
                Debug.LogError("GameManager lacks an item database reference!", this);
            }
            return _itemDatabase;
        }
    }
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
}
