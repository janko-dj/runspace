namespace GameCore
{
    /// <summary>
    /// Represents the different phases of a game run.
    /// Phases follow this sequence: Landing → Expedition → RunBack → Prep → FinalStand → EndSuccess/EndFail
    /// </summary>
    public enum RunPhase
    {
        Landing,      // Safe bubble, players ready up
        Expedition,   // Vampire Survivors exploration, gathering Critical Parts
        RunBack,      // Chase mode, triggered by last Critical Part pickup
        Prep,         // 15-20s to place deployables before Final Stand
        FinalStand,   // Defend ship + repair ship systems
        EndSuccess,   // Victory - ship launched
        EndFail       // Defeat - ship destroyed or team wiped
    }
}
