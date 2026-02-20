using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<ItemStackData> ItemStacks = new List<ItemStackData>(); // List of all the stacks in the inventory, which we will use to reconstruct the inventory when loading
}

[Serializable]
public class ItemStackData // A simple serializable class representing an item stack. This is what we will actually save to JSON for each stack in the inventory
{
    public string ItemID;
    public int ItemQuantity;
}