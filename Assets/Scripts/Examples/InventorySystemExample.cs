using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Example script showing how other systems interact with SharedInventorySystem.
    /// Demonstrates item pickup logic, cargo queries, and event handling.
    /// </summary>
    public class InventorySystemExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool logCargoEvents = true;

        private void OnEnable()
        {
            if (SharedInventorySystem.Instance != null)
            {
                SubscribeToCargoEvents();
            }
        }

        private void OnDisable()
        {
            if (SharedInventorySystem.Instance != null)
            {
                UnsubscribeFromCargoEvents();
            }
        }

        private void SubscribeToCargoEvents()
        {
            SharedInventorySystem.Instance.OnItemAdded += HandleItemAdded;
            SharedInventorySystem.Instance.OnItemRemoved += HandleItemRemoved;
            SharedInventorySystem.Instance.OnCargoFull += HandleCargoFull;
            SharedInventorySystem.Instance.OnCargoCleared += HandleCargoCleared;
        }

        private void UnsubscribeFromCargoEvents()
        {
            if (SharedInventorySystem.Instance == null) return;

            SharedInventorySystem.Instance.OnItemAdded -= HandleItemAdded;
            SharedInventorySystem.Instance.OnItemRemoved -= HandleItemRemoved;
            SharedInventorySystem.Instance.OnCargoFull -= HandleCargoFull;
            SharedInventorySystem.Instance.OnCargoCleared -= HandleCargoCleared;
        }

        private void Update()
        {
            if (SharedInventorySystem.Instance == null) return;

            // Example: Keyboard shortcuts to simulate item pickups
            if (Input.GetKeyDown(KeyCode.P))
            {
                SimulatePickupCriticalPart();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                SimulatePickupSalvage();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                SimulateDropItem();
            }
        }

        // Event handlers

        private void HandleItemAdded(InventoryItemType itemType)
        {
            if (!logCargoEvents) return;

            Debug.Log($"[CargoExample] Item added to cargo: {itemType}");

            // Example: Check if this completes an objective
            if (itemType == InventoryItemType.PowerCore || itemType == InventoryItemType.FuelGel)
            {
                CheckCriticalPartsObjective();
            }

            // Example: Apply cargo weight penalty
            ApplyCargoWeightPenalty();
        }

        private void HandleItemRemoved(InventoryItemType itemType)
        {
            if (!logCargoEvents) return;

            Debug.Log($"[CargoExample] Item removed from cargo: {itemType}");

            // Example: Update cargo weight penalty
            ApplyCargoWeightPenalty();
        }

        private void HandleCargoFull()
        {
            if (!logCargoEvents) return;

            Debug.Log("[CargoExample] CARGO FULL! Players must drop items or return to ship.");

            // Example: In real implementation, this might:
            // - Show UI warning
            // - Apply maximum movement penalty
            // - Trigger voice line
        }

        private void HandleCargoCleared()
        {
            if (!logCargoEvents) return;

            Debug.Log("[CargoExample] Cargo cleared - ready for new run!");
        }

        // Simulation methods (replace with actual pickup triggers in real implementation)

        private void SimulatePickupCriticalPart()
        {
            // Example: Player tries to pick up a Power Core
            if (SharedInventorySystem.Instance.TryAddItem(InventoryItemType.PowerCore))
            {
                Debug.Log("[CargoExample] Picked up Power Core! (Press P)");
                // In real implementation: destroy pickup object, play sound, etc.
            }
            else
            {
                Debug.Log("[CargoExample] Cannot pick up Power Core - cargo full!");
                // In real implementation: show UI feedback, play error sound
            }
        }

        private void SimulatePickupSalvage()
        {
            // Example: Player tries to pick up salvage
            if (SharedInventorySystem.Instance.TryAddItem(InventoryItemType.ScrapMetal))
            {
                Debug.Log("[CargoExample] Picked up Scrap Metal! (Press O)");

                // Example: Register salvage pickup with PressureSystem
                if (PressureSystem.Instance != null)
                {
                    PressureSystem.Instance.RegisterSalvagePickup();
                }
            }
            else
            {
                Debug.Log("[CargoExample] Cannot pick up salvage - cargo full!");
            }
        }

        private void SimulateDropItem()
        {
            // Example: Player drops the first Power Core in cargo
            if (SharedInventorySystem.Instance.RemoveItem(InventoryItemType.PowerCore))
            {
                Debug.Log("[CargoExample] Dropped Power Core! (Press I)");
                // In real implementation: spawn world pickup, play animation
            }
            else
            {
                Debug.Log("[CargoExample] No Power Core to drop!");
            }
        }

        // Gameplay integration examples

        private void CheckCriticalPartsObjective()
        {
            // Example: Check if all required Critical Parts have been collected
            int powerCores = SharedInventorySystem.Instance.CountItem(InventoryItemType.PowerCore);
            int fuelGels = SharedInventorySystem.Instance.CountItem(InventoryItemType.FuelGel);

            if (logCargoEvents)
            {
                Debug.Log($"[CargoExample] Critical Parts progress: {powerCores}/2 Power Cores, {fuelGels}/1 Fuel Gel");
            }

            // Example: If objective complete, trigger Run Back
            if (powerCores >= 2 && fuelGels >= 1)
            {
                Debug.Log("[CargoExample] All Critical Parts collected! Ready to trigger Run Back!");
                // In real implementation: RunPhaseController.Instance.TransitionToPhase(RunPhase.RunBack);
            }
        }

        private void ApplyCargoWeightPenalty()
        {
            // Example: Calculate movement penalty based on cargo fill percentage
            float fillPercentage = SharedInventorySystem.Instance.GetFillPercentage();
            float movementPenalty = fillPercentage * 0.15f; // Up to 15% slower at full cargo

            if (logCargoEvents && fillPercentage > 0)
            {
                Debug.Log($"[CargoExample] Cargo weight penalty: -{movementPenalty * 100:F0}% movement speed");
            }

            // In real implementation: apply to player movement controller
            // playerMovement.speedMultiplier = 1f - movementPenalty;
        }

        // Helper method: Calculate Defense Points from salvage
        private int CalculateDefensePoints()
        {
            if (SharedInventorySystem.Instance == null) return 0;

            // According to design doc: 1 Salvage = 1 DP
            int scrapMetal = SharedInventorySystem.Instance.CountItem(InventoryItemType.ScrapMetal);
            int alienTech = SharedInventorySystem.Instance.CountItem(InventoryItemType.AlienTech);
            int rareComponents = SharedInventorySystem.Instance.CountItem(InventoryItemType.RareComponents);

            return scrapMetal + alienTech + rareComponents;
        }

        private void OnGUI()
        {
            if (SharedInventorySystem.Instance == null) return;

            // Additional debug info showing gameplay implications
            GUI.Box(new Rect(340, 330, 280, 120), "Cargo Gameplay Example");

            int yOffset = 355;
            GUI.Label(new Rect(350, yOffset, 260, 20), "Test Controls:");
            yOffset += 20;
            GUI.Label(new Rect(350, yOffset, 260, 20), "P: Pick up PowerCore");
            yOffset += 20;
            GUI.Label(new Rect(350, yOffset, 260, 20), "O: Pick up Salvage");
            yOffset += 20;
            GUI.Label(new Rect(350, yOffset, 260, 20), "I: Drop PowerCore");
            yOffset += 25;

            // Show gameplay calculations
            int defensePoints = CalculateDefensePoints();
            GUI.Label(new Rect(350, yOffset, 260, 20), $"Defense Points: {defensePoints}");
            yOffset += 20;

            float penalty = SharedInventorySystem.Instance.GetFillPercentage() * 15f;
            GUI.Label(new Rect(350, yOffset, 260, 20), $"Move Penalty: -{penalty:F0}%");
        }
    }
}
