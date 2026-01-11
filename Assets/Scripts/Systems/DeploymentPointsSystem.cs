using System;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages Defense Points (DP) currency for deploying turrets and defenses.
    /// Converts Salvage cargo items into DP.
    /// </summary>
    public class DeploymentPointsSystem : MonoBehaviour
    {
        public static DeploymentPointsSystem Instance { get; private set; }

        [Header("Defense Points")]
        [Tooltip("Current defense points available")]
        [SerializeField] private int currentDefensePoints = 0;

        [Header("Conversion Settings")]
        [Tooltip("How many DP per salvage item")]
        [SerializeField] private int dpPerSalvageItem = 10;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logDPEvents = true;

        // Events
        public event Action<int, int> OnDefensePointsChanged; // old amount, new amount

        public int CurrentDefensePoints => currentDefensePoints;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (logDPEvents)
            {
                Debug.Log($"[DeploymentPointsSystem] Initialized with {currentDefensePoints} DP");
            }
        }

        /// <summary>
        /// Add defense points.
        /// </summary>
        public void AddDefensePoints(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[DeploymentPointsSystem] Attempted to add invalid amount: {amount}");
                return;
            }

            int oldAmount = currentDefensePoints;
            currentDefensePoints += amount;

            if (logDPEvents)
            {
                Debug.Log($"[DeploymentPointsSystem] +{amount} DP | Total: {currentDefensePoints}");
            }

            OnDefensePointsChanged?.Invoke(oldAmount, currentDefensePoints);
        }

        public void ResetDefensePoints()
        {
            int oldAmount = currentDefensePoints;
            currentDefensePoints = 0;
            OnDefensePointsChanged?.Invoke(oldAmount, currentDefensePoints);
        }

        /// <summary>
        /// Spend defense points if enough are available.
        /// </summary>
        /// <returns>True if successful, false if not enough DP</returns>
        public bool SpendDefensePoints(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[DeploymentPointsSystem] Attempted to spend invalid amount: {amount}");
                return false;
            }

            if (currentDefensePoints < amount)
            {
                if (logDPEvents)
                {
                    Debug.LogWarning($"[DeploymentPointsSystem] Not enough DP! Need {amount}, have {currentDefensePoints}");
                }
                return false;
            }

            int oldAmount = currentDefensePoints;
            currentDefensePoints -= amount;

            if (logDPEvents)
            {
                Debug.Log($"[DeploymentPointsSystem] -{amount} DP | Remaining: {currentDefensePoints}");
            }

            OnDefensePointsChanged?.Invoke(oldAmount, currentDefensePoints);
            return true;
        }

        /// <summary>
        /// Convert all salvage cargo items to Defense Points.
        /// </summary>
        public void ConvertSalvageToDP()
        {
            if (SharedInventorySystem.Instance == null)
            {
                Debug.LogError("[DeploymentPointsSystem] SharedInventorySystem not found! Cannot convert salvage.");
                return;
            }

            int salvageCount = GetSalvageItemCount();

            if (salvageCount <= 0)
            {
                if (logDPEvents)
                {
                    Debug.Log("[DeploymentPointsSystem] No salvage to convert.");
                }
                return;
            }

            // Remove all salvage from cargo
            RemoveSalvageItems(salvageCount);

            // Calculate DP gained
            int dpGained = salvageCount * dpPerSalvageItem;

            // Add DP
            AddDefensePoints(dpGained);

            if (logDPEvents)
            {
                Debug.Log($"[DeploymentPointsSystem] Converted {salvageCount} salvage → {dpGained} DP ({dpPerSalvageItem} DP per item)");
            }
        }

        /// <summary>
        /// Check if player has enough DP.
        /// </summary>
        public bool HasEnoughDP(int amount)
        {
            return currentDefensePoints >= amount;
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Defense Points: {currentDefensePoints}";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Debug overlay
            int boxWidth = 280;
            int boxHeight = 80;
            int xPos = Screen.width - boxWidth - 10;
            int yPos = 250; // Below RunCompletionSystem overlay

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Defense Points (Debug)");

            int yOffset = yPos + 25;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Current DP: {currentDefensePoints}");
            yOffset += 20;

            if (SharedInventorySystem.Instance != null)
            {
                int salvageCount = GetSalvageItemCount();
                int potentialDP = salvageCount * dpPerSalvageItem;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Salvage: {salvageCount} ({potentialDP} DP)");
            }
            yOffset += 20;

            // Test button to convert salvage
            if (GUI.Button(new Rect(xPos + 10, yOffset, 120, 20), "Convert Salvage"))
            {
                ConvertSalvageToDP();
            }
        }

        private int GetSalvageItemCount()
        {
            if (SharedInventorySystem.Instance == null)
            {
                return 0;
            }

            return SharedInventorySystem.Instance.CountItem(InventoryItemType.ScrapMetal)
                   + SharedInventorySystem.Instance.CountItem(InventoryItemType.AlienTech)
                   + SharedInventorySystem.Instance.CountItem(InventoryItemType.RareComponents);
        }

        private void RemoveSalvageItems(int count)
        {
            if (count <= 0 || SharedInventorySystem.Instance == null)
            {
                return;
            }

            int remaining = count;
            InventoryItemType[] salvageTypes =
            {
                InventoryItemType.ScrapMetal,
                InventoryItemType.AlienTech,
                InventoryItemType.RareComponents
            };

            foreach (InventoryItemType itemType in salvageTypes)
            {
                while (remaining > 0 && SharedInventorySystem.Instance.RemoveItem(itemType))
                {
                    remaining--;
                }

                if (remaining == 0)
                {
                    break;
                }
            }
        }
    }
}
