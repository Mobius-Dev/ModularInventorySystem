using System;

/// <summary>
/// Represents a collection of a specific item and tracks its quantity within the stack.
/// </summary>
public class ItemStack
{
    public ItemDef ItemStored { get; private set; }

    private int _quantityStored;

    public int QuantityStored
    {
        get => _quantityStored;
        set
        {
            if (_quantityStored == value) return; // Optimization: don't notify if value hasn't changed
            _quantityStored = value;
            OnQuantityChanged?.Invoke(_quantityStored);
        }
    }

    public event Action<int> OnQuantityChanged;

    public ItemStack(ItemDef item, int quantity = 1)
    {
        ItemStored = item;
        QuantityStored = quantity;
    }
}
