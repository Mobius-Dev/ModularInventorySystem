using UnityEngine;

/// <summary>
/// Provides functionality to merge and split stacks within an
/// inventory system.
/// </summary>
public static class StackUtility
{
    public static PlacementResult AttemptMerge(ItemStack stackA, ItemStack stackB)
    {
        // stackB is the source stack we want to merge into stackA (the target stack)
        if (stackA.ItemStored.ItemID == stackB.ItemStored.ItemID)
        {
            int totalQuantity = stackA.QuantityStored + stackB.QuantityStored;

            if (totalQuantity <= stackA.ItemStored.MaxStackSize)
            {
                stackA.QuantityStored = totalQuantity;
                stackB.QuantityStored = 0;
                return PlacementResult.MergedFully;
            }
            else
            {
                int spaceLeft = stackA.ItemStored.MaxStackSize - stackA.QuantityStored;

                if (spaceLeft == 0)
                {
                    return PlacementResult.FailedStackFull; // Target is already full. Merge failed.
                }

                stackA.QuantityStored += spaceLeft;
                stackB.QuantityStored -= spaceLeft;
                return PlacementResult.MergedPartially; // Partially merged
            }
        }
        return PlacementResult.FailedDiffItems; // Different items. Merge failed.
    }
    public static bool AttemptSplit(ItemStack originalStack, out ItemStack newStack)
    {
        if (originalStack.QuantityStored > 1)
        {
            int splitQuantity = originalStack.QuantityStored / 2;
            originalStack.QuantityStored -= splitQuantity;
            newStack = new ItemStack(originalStack.ItemStored, splitQuantity);
            return true;
        }

        newStack = null;
        return false;
    }
}

