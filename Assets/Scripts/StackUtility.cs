using UnityEngine;

/// <summary>
/// Provides functionality to merge and split stacks within an
/// inventory system.
/// </summary>
public static class StackUtility
{
    public static bool AttemptMerge(ItemStack stackA, ItemStack stackB)
    {
        // Attempt to merge stackB into stackA

        if (stackA.ItemStored.ItemID == stackB.ItemStored.ItemID)
        {
            int totalQuantity = stackA.QuantityStored + stackB.QuantityStored;

            // If the total quantity fits within the max stack size, merge completely
            if (totalQuantity <= stackA.ItemStored.MaxStackSize)
            {
                stackA.QuantityStored = totalQuantity;
                stackB.QuantityStored = 0; // Emptied stackB
                NotificationBus.PostMessage($"Fully merged stacks: {stackA.ItemStored.ItemDisplayName} now has {stackA.QuantityStored} items.");
                return true;
            }
            // Otherwise, fill stackA to max and reduce stackB accordingly
            else
            {
                int spaceLeft = stackA.ItemStored.MaxStackSize - stackA.QuantityStored;
                stackA.QuantityStored += spaceLeft;
                stackB.QuantityStored -= spaceLeft;

                if (spaceLeft == 0)
                {
                    NotificationBus.PostMessage($"Can't merge, {stackA.ItemStored.ItemDisplayName} is already at max stack size of {stackA.ItemStored.MaxStackSize}");
                    
                }
                else
                {
                    NotificationBus.PostMessage($"Partially merged stacks: {stackA.ItemStored.ItemDisplayName} now has {stackA.QuantityStored} items, {stackB.QuantityStored} items remain in stackB.");
                }
                return true;
            }
        }
        // Requirements for merging not met
        NotificationBus.PostMessage($"Can't merge two different items: {stackA.ItemStored.ItemDisplayName}, {stackB.ItemStored.ItemDisplayName}");
        return false;
    }

    public static bool AttemptSplit(ItemStack originalStack, out ItemStack newStack)
    {
        // Attempt to split the original stack into two stacks of equal quantity

        if (originalStack.QuantityStored > 1)
        {
            int splitQuantity = originalStack.QuantityStored / 2;
            originalStack.QuantityStored -= splitQuantity;
            newStack = new ItemStack(originalStack.ItemStored, splitQuantity);
            NotificationBus.PostMessage($"Split stack of {originalStack.ItemStored.ItemDisplayName} into two stacks of {originalStack.QuantityStored} and {newStack.QuantityStored} items");
            return true;
        }

        newStack = null;
        NotificationBus.PostMessage($"Can't split stack of {originalStack.ItemStored.ItemDisplayName} because it only has a single item");
        return false; // Not enough quantity to split
    }
}
