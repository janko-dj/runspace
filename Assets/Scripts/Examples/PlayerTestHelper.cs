using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Helper script for testing player systems.
    /// Demonstrates how to integrate player systems with cargo and provides test utilities.
    /// </summary>
    public class PlayerTestHelper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterMover playerController;
        [SerializeField] private AutoAttackWeapon playerWeapon;

        [Header("Cargo Integration Example")]
        [SerializeField] private bool applyCargoSpeedPenalty = true;

        [Header("Debug")]
        [SerializeField] private bool showCombatStats = true;

        private float baseSpeed;
        private int totalKills = 0;

        private void Start()
        {
            // Try to find player components if not assigned
            if (playerController == null)
                playerController = Object.FindFirstObjectByType<CharacterMover>();
            if (playerWeapon == null)
                playerWeapon = Object.FindFirstObjectByType<AutoAttackWeapon>();

            // Store base speed for cargo penalty calculations
            if (playerController != null)
            {
                baseSpeed = playerController.GetCurrentSpeed();
            }
        }

        private void Update()
        {
            // Example: Apply cargo weight penalty to movement speed
            if (applyCargoSpeedPenalty && playerController != null && SharedInventorySystem.Instance != null)
            {
                ApplyCargoSpeedPenalty();
            }

            // Track kills for stats
            UpdateCombatStats();
        }

        private void ApplyCargoSpeedPenalty()
        {
            // Get cargo fill percentage (0-1)
            float fillPercentage = SharedInventorySystem.Instance.GetFillPercentage();

            // Calculate penalty: up to 15% slower when cargo is full
            float penaltyMultiplier = 1f - (fillPercentage * 0.15f);

            // Apply modified speed
            float modifiedSpeed = baseSpeed * penaltyMultiplier;
            playerController.SetMoveSpeed(modifiedSpeed);
        }

        private void UpdateCombatStats()
        {
            // This is just an example - in a real implementation,
            // you'd track kills via events from ChaserEnemy deaths
        }

        private void OnGUI()
        {
            if (!showCombatStats) return;

            GUI.Box(new Rect(10, 750, 300, 80), "Player Stats");

            int yOffset = 775;

            if (SharedInventorySystem.Instance != null && applyCargoSpeedPenalty)
            {
                float penalty = SharedInventorySystem.Instance.GetFillPercentage() * 15f;
                GUI.Label(new Rect(20, yOffset, 280, 20), $"Speed Penalty: -{penalty:F0}%");
                yOffset += 20;
            }

            if (playerController != null)
            {
                GUI.Label(new Rect(20, yOffset, 280, 20), $"Current Speed: {playerController.GetCurrentSpeed():F1}");
                yOffset += 20;
            }

            if (playerWeapon != null)
            {
                ChaserEnemy[] enemies = Object.FindObjectsByType<ChaserEnemy>(FindObjectsSortMode.None);
                GUI.Label(new Rect(20, yOffset, 280, 20), $"Enemies Alive: {enemies.Length}");
            }
        }
    }
}
