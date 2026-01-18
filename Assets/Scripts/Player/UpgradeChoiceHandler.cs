using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages per-player upgrade selection state.
    /// Each player has their own instance tracking their upgrade choices.
    /// </summary>
    public class UpgradeChoiceHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AutoAttackWeapon playerWeapon;
        [SerializeField] private CharacterMover playerController;

        [Header("Upgrade Choice State")]
        [SerializeField] private bool isChoosingUpgrade = false;
        [SerializeField] private bool hasChosenUpgrade = false;
        [SerializeField] private float choiceTimer = 0f;
        [SerializeField] private float choiceTimeLimit = 3f;

        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logChoices = true;

        private List<CharacterUpgrade> pendingUpgradeChoices = new List<CharacterUpgrade>();
        private CharacterUpgrade selectedUpgrade = null;

        // Public accessors
        public bool IsChoosingUpgrade => isChoosingUpgrade;
        public bool HasChosenUpgrade => hasChosenUpgrade;
        public float ChoiceTimer => choiceTimer;
        public List<CharacterUpgrade> PendingChoices => pendingUpgradeChoices;
        public void SetChoiceTimeLimit(float limitSeconds) => choiceTimeLimit = Mathf.Max(0.1f, limitSeconds);

        private void Awake()
        {
            // Auto-find components if not assigned
            if (playerWeapon == null)
                playerWeapon = GetComponent<AutoAttackWeapon>();
            if (playerController == null)
                playerController = GetComponent<CharacterMover>();
        }

        private void Update()
        {
            if (!isChoosingUpgrade || hasChosenUpgrade) return;

            // Count down choice timer
            choiceTimer += Time.unscaledDeltaTime;

            // Handle keyboard input for selection
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectUpgrade(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectUpgrade(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SelectUpgrade(2);
            }
        }

        /// <summary>
        /// Start the upgrade choice process with 3 random upgrades.
        /// </summary>
        public void StartUpgradeChoice(List<CharacterUpgrade> choices)
        {
            if (choices == null || choices.Count != 3)
            {
                Debug.LogError("[UpgradeChoiceHandler] Must provide exactly 3 upgrade choices!");
                return;
            }

            pendingUpgradeChoices = new List<CharacterUpgrade>(choices);
            isChoosingUpgrade = true;
            hasChosenUpgrade = false;
            choiceTimer = 0f;
            selectedUpgrade = null;

            if (logChoices)
            {
                Debug.Log($"[UpgradeChoiceHandler] {gameObject.name} has 3 seconds to choose:");
                for (int i = 0; i < pendingUpgradeChoices.Count; i++)
                {
                    Debug.Log($"  [{i + 1}] {pendingUpgradeChoices[i]}");
                }
            }
        }

        /// <summary>
        /// Player selects an upgrade by index (0, 1, or 2).
        /// </summary>
        private void SelectUpgrade(int choiceIndex)
        {
            if (!isChoosingUpgrade || hasChosenUpgrade)
                return;

            if (choiceIndex < 0 || choiceIndex >= pendingUpgradeChoices.Count)
            {
                Debug.LogWarning($"[UpgradeChoiceHandler] Invalid choice index: {choiceIndex}");
                return;
            }

            selectedUpgrade = pendingUpgradeChoices[choiceIndex];
            hasChosenUpgrade = true;

            if (logChoices)
            {
                Debug.Log($"[UpgradeChoiceHandler] {gameObject.name} chose: {selectedUpgrade}");
            }
        }

        /// <summary>
        /// Auto-select a random upgrade (called when timer expires without choice).
        /// </summary>
        public void AutoSelectRandomUpgrade()
        {
            if (hasChosenUpgrade || pendingUpgradeChoices.Count == 0)
                return;

            int randomIndex = Random.Range(0, pendingUpgradeChoices.Count);
            selectedUpgrade = pendingUpgradeChoices[randomIndex];
            hasChosenUpgrade = true;

            if (logChoices)
            {
                Debug.Log($"[UpgradeChoiceHandler] {gameObject.name} time expired! Auto-selected: {selectedUpgrade}");
            }
        }

        /// <summary>
        /// Apply the selected upgrade to this player.
        /// </summary>
        public void ApplySelectedUpgrade()
        {
            if (selectedUpgrade == null)
            {
                Debug.LogWarning($"[UpgradeChoiceHandler] {gameObject.name} has no upgrade to apply!");
                return;
            }

            selectedUpgrade.ApplyToPlayer(playerWeapon, playerController);

            if (logChoices)
            {
                Debug.Log($"[UpgradeChoiceHandler] {gameObject.name} applied upgrade: {selectedUpgrade}");
            }

            // Clear choice state
            ClearUpgradeState();
        }

        /// <summary>
        /// Clear all upgrade choice state.
        /// </summary>
        public void ClearUpgradeState()
        {
            isChoosingUpgrade = false;
            hasChosenUpgrade = false;
            choiceTimer = 0f;
            pendingUpgradeChoices.Clear();
            selectedUpgrade = null;
        }

        /// <summary>
        /// Reset all player modifiers (for new run).
        /// </summary>
        public void ResetModifiers()
        {
            if (playerWeapon != null)
                playerWeapon.ResetModifiers();
            if (playerController != null)
                playerController.ResetModifier();

            ClearUpgradeState();
        }

        private void OnGUI()
        {
            if (!showDebugOverlay || !isChoosingUpgrade) return;

            // Show upgrade choices for this player
            int boxWidth = 420;
            int boxHeight = 170;
            int xPos = Screen.width / 2 - boxWidth / 2;
            int yPos = Screen.height / 2 - boxHeight / 2;

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Level Up - Choose Upgrade");

            int yOffset = yPos + 25;
            float timeRemaining = choiceTimeLimit - choiceTimer;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Time: {timeRemaining:F1}s");
            yOffset += 20;

            for (int i = 0; i < pendingUpgradeChoices.Count; i++)
            {
                string choiceText = $"[{i + 1}] {pendingUpgradeChoices[i]}";
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), choiceText);
                yOffset += 20;
            }

            yOffset += 5;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Press 1, 2, or 3 to select");

            if (hasChosenUpgrade)
            {
                yOffset += 20;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"✓ Chosen: {selectedUpgrade.upgradeName}");
            }
        }
    }
}
