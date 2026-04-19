public enum PlacementResult // Result of attempting to place a tile into a slot
{
    MovedToEmpty,
    MergedPartially,
    MergedFully,
    FailedDiffItems,
    FailedStackFull
}