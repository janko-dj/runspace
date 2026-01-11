namespace GameCore
{
    /// <summary>
    /// Types of items that can be stored in the shared cargo system.
    /// Based on design spec:
    /// - Critical Parts: required to trigger Run Back (e.g., Power Core, Fuel Gel)
    /// - Salvage: optional loot that converts to Defense Points
    /// </summary>
    public enum InventoryItemType
    {
        None,           // Empty slot

        // Critical Parts - required objectives
        PowerCore,
        FuelGel,

        // Salvage - optional loot for Defense Points
        ScrapMetal,
        AlienTech,
        RareComponents,

        // Future expansion placeholder
        MedicalSupplies,
        EnergyCell
    }
}
