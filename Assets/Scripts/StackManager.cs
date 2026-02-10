using UnityEngine;

/// <summary>
/// Provides centralized management for item stacks, including functionality to merge and split stacks within an
/// inventory system.
/// </summary>
public class StackManager : MonoBehaviour
{
    public static StackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public bool AttemptMerge(ItemStack stackA, ItemStack stackB)
    {
        // Attempt to merge stackB into stackA

        if (stackA.ItemStored.ItemID == stackB.ItemStored.ItemID)
        {
            int totalQuantity = stackA.QuantityStored + stackB.QuantityStored;

            if (totalQuantity <= stackA.ItemStored.MaxStackSize)
            {
                stackA.QuantityStored = totalQuantity;
                stackB.QuantityStored = 0; // Emptied stackB
                return true;
            }
            else
            {
                int spaceLeft = stackA.ItemStored.MaxStackSize - stackA.QuantityStored;
                stackA.QuantityStored += spaceLeft;
                stackB.QuantityStored -= spaceLeft;
                return true;
            }
        }
        // Requirements for merging not met
        return false;
    }

    public bool AttemptSplit(ItemStack originalStack, out ItemStack newStack)
    {
        // Attempt to split the original stack into two stacks of equal quantity

        if (originalStack.QuantityStored > 1)
        {
            int splitQuantity = originalStack.QuantityStored / 2;
            originalStack.QuantityStored -= splitQuantity;
            newStack = new ItemStack(originalStack.ItemStored, splitQuantity);
            return true;
        }

        newStack = null;
        return false; // Not enough quantity to split
    }
}
