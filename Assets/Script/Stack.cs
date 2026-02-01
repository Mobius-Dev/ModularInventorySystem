using UnityEngine;

public class Stack
{
    public ItemDef ItemStored { get; private set; }
    public int QuantityStored;

    public Stack(ItemDef item, int quantity = 1)
    {
        ItemStored = item;
        QuantityStored = quantity;
    }
}
