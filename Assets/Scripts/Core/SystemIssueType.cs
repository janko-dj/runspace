namespace GameCore
{
    /// <summary>
    /// Types of ship problems that can occur during Prep and FinalStand phases.
    /// Each problem requires repair at a specific station.
    /// </summary>
    public enum SystemIssueType
    {
        PowerFailure,      // Power systems offline
        HullBreach,        // Ship hull damaged
        NavigationError,   // Navigation computer malfunctioning
        LifeSupport,       // Life support systems failing
        CommunicationLoss  // Communication systems down
    }
}
