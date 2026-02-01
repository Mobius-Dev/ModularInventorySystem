using UnityEngine;

public class StackManager : MonoBehaviour
{
    // A singleton which manages Stack related operations

    public static StackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public bool AttemptMerge(Stack stackA, Stack stackB)
    {
        // Attempt to merge stackB into stackA, executive if valid

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
                // TODO Partial merge
                //int spaceLeft = stackA.ItemStored.MaxStackSize - stackA.QuantityStored;
                //stackA.QuantityStored += spaceLeft;
                //stackB.QuantityStored -= spaceLeft;
                return false;
            }
        }
        return false; // Requirements for merging not met
    }
}
