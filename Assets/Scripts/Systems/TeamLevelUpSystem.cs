using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages async per-player upgrade choices during team level-ups.
    /// Each player has 3 seconds to choose from 3 upgrades.
    /// Game continues running (no pause).
    /// </summary>
    public class TeamLevelUpSystem : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Time limit for players to choose upgrades")]
        [SerializeField] private float choiceTimeLimit = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logUpgrades = true;

        [Header("Runtime State (Read Only)")]
        [SerializeField] private bool isResolvingUpgrades = false;
        [SerializeField] private float resolutionTimer = 0f;

        private List<UpgradeChoiceHandler> allPlayerManagers = new List<UpgradeChoiceHandler>();

        private void OnEnable()
        {
            if (RunXPSystem.Instance != null)
            {
                RunXPSystem.Instance.OnLevelUp += HandleTeamLevelUp;
            }

            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
            }
        }

        private void Start()
        {
            // Fallback subscription
            if (RunXPSystem.Instance != null)
            {
                RunXPSystem.Instance.OnLevelUp += HandleTeamLevelUp;
            }

            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
            }
        }

        private void OnDisable()
        {
            if (RunXPSystem.Instance != null)
            {
                RunXPSystem.Instance.OnLevelUp -= HandleTeamLevelUp;
            }

            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnLandingEnter -= HandleLandingEnter;
            }
        }

        private void Update()
        {
            if (!isResolvingUpgrades) return;

            resolutionTimer += Time.deltaTime;

            // Check if all players have chosen OR time has expired
            if (AllPlayersHaveChosen() || resolutionTimer >= choiceTimeLimit)
            {
                ResolveUpgrades();
            }
        }

        private void HandleTeamLevelUp(int newLevel)
        {
            if (logUpgrades)
            {
                Debug.Log($"[TeamLevelUpSystem] Team reached level {newLevel}! Starting async upgrade selection...");
            }

            // Find all UpgradeChoiceHandler instances
            FindAllPlayerManagers();

            if (allPlayerManagers.Count == 0)
            {
                Debug.LogWarning("[TeamLevelUpSystem] No PlayerUpgradeManagers found! Cannot offer upgrades.");
                return;
            }

            // Generate unique upgrade choices for EACH player
            foreach (UpgradeChoiceHandler manager in allPlayerManagers)
            {
                List<CharacterUpgrade> choices = GenerateUpgradeChoices();
                manager.StartUpgradeChoice(choices);
            }

            // Start resolution timer (NO PAUSE!)
            isResolvingUpgrades = true;
            resolutionTimer = 0f;

            if (logUpgrades)
            {
                Debug.Log($"[TeamLevelUpSystem] {allPlayerManagers.Count} players have {choiceTimeLimit} seconds to choose!");
            }
        }

        private void FindAllPlayerManagers()
        {
            allPlayerManagers.Clear();
            allPlayerManagers.AddRange(Object.FindObjectsByType<UpgradeChoiceHandler>(FindObjectsSortMode.None));
        }

        private List<CharacterUpgrade> GenerateUpgradeChoices()
        {
            // Create pool of possible upgrades
            List<CharacterUpgrade> upgradePool = new List<CharacterUpgrade>
            {
                new UpgradeAttackRange(1f),
                new UpgradeAttackRange(0.5f),
                new UpgradeAttackSpeed(1.2f), // 20% faster
                new UpgradeAttackSpeed(1.1f), // 10% faster
                new UpgradeDamage(5f),
                new UpgradeDamage(3f),
                new UpgradeMoveSpeed(1f),
                new UpgradeMoveSpeed(0.5f),
                new UpgradeBalanced()
            };

            // Randomly select 3 upgrades
            List<CharacterUpgrade> choices = new List<CharacterUpgrade>();
            for (int i = 0; i < 3 && upgradePool.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, upgradePool.Count);
                choices.Add(upgradePool[randomIndex]);
                upgradePool.RemoveAt(randomIndex); // Don't offer duplicates
            }

            return choices;
        }

        private bool AllPlayersHaveChosen()
        {
            foreach (UpgradeChoiceHandler manager in allPlayerManagers)
            {
                if (!manager.HasChosenUpgrade)
                    return false;
            }
            return true;
        }

        private void ResolveUpgrades()
        {
            if (logUpgrades)
            {
                Debug.Log("[TeamLevelUpSystem] Resolving upgrades...");
            }

            // For each player:
            // - If they chose, apply their choice
            // - If they didn't choose, auto-select random
            foreach (UpgradeChoiceHandler manager in allPlayerManagers)
            {
                if (!manager.HasChosenUpgrade)
                {
                    // Time expired without choice - auto-select random
                    manager.AutoSelectRandomUpgrade();
                }

                // Apply the upgrade to this player
                manager.ApplySelectedUpgrade();
            }

            // Clear resolution state
            isResolvingUpgrades = false;
            resolutionTimer = 0f;

            if (logUpgrades)
            {
                Debug.Log("[TeamLevelUpSystem] All upgrades applied! Game continues.");
            }
        }

        private void HandleLandingEnter()
        {
            // Reset all player modifiers for new run
            FindAllPlayerManagers();

            foreach (UpgradeChoiceHandler manager in allPlayerManagers)
            {
                manager.ResetModifiers();
            }

            // Clear any pending upgrades
            isResolvingUpgrades = false;
            resolutionTimer = 0f;

            if (logUpgrades)
            {
                Debug.Log("[TeamLevelUpSystem] New run - all player upgrades reset");
            }
        }

        private void OnGUI()
        {
            if (!showDebugOverlay || !isResolvingUpgrades) return;

            // Show global resolution status
            int boxWidth = 300;
            int boxHeight = 100;
            int centerX = Screen.width / 2 - boxWidth / 2;
            int yPos = 10;

            GUI.Box(new Rect(centerX, yPos, boxWidth, boxHeight), "TEAM LEVEL UP!");

            int yOffset = yPos + 30;
            float timeRemaining = choiceTimeLimit - resolutionTimer;
            GUI.Label(new Rect(centerX + 10, yOffset, boxWidth - 20, 20), $"Time Remaining: {timeRemaining:F1}s");
            yOffset += 25;

            int chosenCount = 0;
            foreach (UpgradeChoiceHandler manager in allPlayerManagers)
            {
                if (manager.HasChosenUpgrade)
                    chosenCount++;
            }

            GUI.Label(new Rect(centerX + 10, yOffset, boxWidth - 20, 20), $"Players Chosen: {chosenCount}/{allPlayerManagers.Count}");
            yOffset += 20;

            if (AllPlayersHaveChosen())
            {
                GUI.Label(new Rect(centerX + 10, yOffset, boxWidth - 20, 20), "✓ All players ready!");
            }
        }
    }
}
