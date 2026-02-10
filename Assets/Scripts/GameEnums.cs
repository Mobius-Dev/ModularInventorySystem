public enum ItemID // Specific item identifiers
{
    Gun_Pistol,
    Material_Metal,
    Material_Wood,
    Consumable_Medkit
}

public enum PlacementResult // Result of attempting to place a tile into a slot
{
    Failed,
    MovedToEmpty,
    MergedPartially,
    MergedFully
}