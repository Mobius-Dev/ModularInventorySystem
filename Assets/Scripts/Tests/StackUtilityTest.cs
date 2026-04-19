using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Tests for the StackUtility class, which handles merging of item stacks in the inventory system, using AAA (Arrange, Act, Assert) pattern for clarity and maintainability
/// </summary>
public class StackUtilityTests
{
    // A helper method to quickly create dummy ItemDefs for our tests
    private ItemDef CreateDummyItem(string id, int maxStackSize)
    {
        ItemDef dummyItem = ScriptableObject.CreateInstance<ItemDef>();
        dummyItem.ItemID = id;
        dummyItem.MaxStackSize = maxStackSize;
        return dummyItem;
    }

    [Test]
    public void AttemptMerge_WithEnoughSpace_MergesFully()
    {
        ItemDef woodItem = CreateDummyItem("wood", 10);
        ItemStack targetStack = new ItemStack(woodItem, 5);
        ItemStack sourceStack = new ItemStack(woodItem, 3);

        PlacementResult result = StackUtility.AttemptMerge(targetStack, sourceStack);

        Assert.AreEqual(PlacementResult.MergedFully, result, "Should return (MergedFully) when there is enough space.");
        Assert.AreEqual(8, targetStack.QuantityStored, "Target stack should have 8 items.");
        Assert.AreEqual(0, sourceStack.QuantityStored, "Source stack should be empty after full merge.");
    }

    [Test]
    public void AttemptMerge_WithNoSpace_ReturnsFalse()
    {
        ItemDef woodItem = CreateDummyItem("wood", 10);
        ItemStack targetStack = new ItemStack(woodItem, 10); // Target is already full
        ItemStack sourceStack = new ItemStack(woodItem, 5);

        PlacementResult result = StackUtility.AttemptMerge(targetStack, sourceStack);

        Assert.AreEqual(PlacementResult.FailedStackFull, result, "Merge should fail (return FailedStackFull) when target is full.");
        Assert.AreEqual(10, targetStack.QuantityStored, "Target stack should remain at 10.");
        Assert.AreEqual(5, sourceStack.QuantityStored, "Source stack should remain at 5.");
    }

    [Test]
    public void AttemptMerge_DifferentItems_ReturnsFalse()
    {
        ItemDef woodItem = CreateDummyItem("wood", 10);
        ItemDef stoneItem = CreateDummyItem("stone", 10);

        ItemStack targetStack = new ItemStack(woodItem, 5);
        ItemStack sourceStack = new ItemStack(stoneItem, 5);

        PlacementResult result = StackUtility.AttemptMerge(targetStack, sourceStack);

        Assert.AreEqual(PlacementResult.FailedDiffItems, result, "Merge should fail (return FailedDiffItems) when item IDs do not match.");
        Assert.AreEqual(5, targetStack.QuantityStored);
        Assert.AreEqual(5, sourceStack.QuantityStored);
    }

    [Test]
    public void AttemptMerge_WithPartialSpace_MergesPartially()
    {
        ItemDef woodItem = CreateDummyItem("wood", 10);
        ItemStack targetStack = new ItemStack(woodItem, 8); // Room for 2
        ItemStack sourceStack = new ItemStack(woodItem, 5); // Trying to add 5

        PlacementResult result = StackUtility.AttemptMerge(targetStack, sourceStack);

        Assert.AreEqual(PlacementResult.MergedPartially, result, "Merge should return MergedPartially for a partial merge.");
        Assert.AreEqual(10, targetStack.QuantityStored, "Target stack should be maxed out at 10.");
        Assert.AreEqual(3, sourceStack.QuantityStored, "Source stack should have 3 items leftover.");
    }
}