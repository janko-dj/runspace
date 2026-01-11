using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages the shared team cargo inventory.
    /// Cargo is shared across all players and persists across phases within a run.
    /// Clears only when entering Landing phase (new run).
    /// </summary>
    public class SharedInventorySystem : MonoBehaviour
    {
        public static SharedInventorySystem Instance { get; private set; }

        [Header("Cargo Configuration")]
        [Tooltip("Maximum number of cargo slots available")]
        [SerializeField] private int maxSlots = 6;

        [Header("Current Cargo (Read Only)")]
        [SerializeField] private InventoryItemType[] cargoSlots;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logCargoChanges = true;

        // Events for other systems to react to cargo changes
        public event Action<InventoryItemType> OnItemAdded;
        public event Action<InventoryItemType> OnItemRemoved;
        public event Action OnCargoCleared;
        public event Action OnCargoFull;

        // Public accessors
        public int MaxSlots => maxSlots;
        public int CurrentCount => GetCurrentCount();
        public bool IsCargoFull => IsFull();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize cargo slots
            InitializeCargo();
        }

        private void OnEnable()
        {
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
            }
        }

        private void Start()
        {
            // Fallback: try subscribing again if not done in OnEnable
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromPhaseEvents();
        }

        private void InitializeCargo()
        {
            cargoSlots = new InventoryItemType[maxSlots];
            ClearCargo();
        }

        private void SubscribeToPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;
            RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
        }

        private void UnsubscribeFromPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;
            RunPhaseController.Instance.OnLandingEnter -= HandleLandingEnter;
        }

        private void HandleLandingEnter()
        {
            // Clear cargo when starting a new run
            ClearCargo();

            if (logCargoChanges)
            {
                Debug.Log("[SharedCargo] Entering Landing - Cargo cleared for new run");
            }
        }

        /// <summary>
        /// Attempt to add an item to cargo. Returns true if successful.
        /// </summary>
        public bool TryAddItem(InventoryItemType itemType)
        {
            if (itemType == InventoryItemType.None)
            {
                Debug.LogWarning("[SharedCargo] Cannot add None item type");
                return false;
            }

            if (IsFull())
            {
                if (logCargoChanges)
                {
                    Debug.LogWarning($"[SharedCargo] Cargo is full! Cannot add {itemType}");
                }
                OnCargoFull?.Invoke();
                return false;
            }

            // Find first empty slot
            for (int i = 0; i < cargoSlots.Length; i++)
            {
                if (cargoSlots[i] == InventoryItemType.None)
                {
                    cargoSlots[i] = itemType;

                    if (logCargoChanges)
                    {
                        Debug.Log($"[SharedCargo] Added {itemType} to slot {i} ({GetCurrentCount()}/{maxSlots})");
                    }

                    OnItemAdded?.Invoke(itemType);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove the first occurrence of the specified item type from cargo.
        /// Returns true if item was found and removed.
        /// </summary>
        public bool RemoveItem(InventoryItemType itemType)
        {
            if (itemType == InventoryItemType.None)
            {
                Debug.LogWarning("[SharedCargo] Cannot remove None item type");
                return false;
            }

            // Find first matching item
            for (int i = 0; i < cargoSlots.Length; i++)
            {
                if (cargoSlots[i] == itemType)
                {
                    cargoSlots[i] = InventoryItemType.None;

                    if (logCargoChanges)
                    {
                        Debug.Log($"[SharedCargo] Removed {itemType} from slot {i} ({GetCurrentCount()}/{maxSlots})");
                    }

                    OnItemRemoved?.Invoke(itemType);
                    return true;
                }
            }

            if (logCargoChanges)
            {
                Debug.LogWarning($"[SharedCargo] Could not find {itemType} to remove");
            }

            return false;
        }

        /// <summary>
        /// Check if cargo is full (all slots occupied).
        /// </summary>
        public bool IsFull()
        {
            foreach (var slot in cargoSlots)
            {
                if (slot == InventoryItemType.None)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the number of occupied cargo slots.
        /// </summary>
        public int GetCurrentCount()
        {
            int count = 0;
            foreach (var slot in cargoSlots)
            {
                if (slot != InventoryItemType.None)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get the number of free cargo slots.
        /// </summary>
        public int GetFreeSlots()
        {
            return maxSlots - GetCurrentCount();
        }

        /// <summary>
        /// Clear all cargo (used when entering Landing phase).
        /// </summary>
        public void ClearCargo()
        {
            for (int i = 0; i < cargoSlots.Length; i++)
            {
                cargoSlots[i] = InventoryItemType.None;
            }

            OnCargoCleared?.Invoke();
        }

        /// <summary>
        /// Check if cargo contains a specific item type.
        /// </summary>
        public bool ContainsItem(InventoryItemType itemType)
        {
            foreach (var slot in cargoSlots)
            {
                if (slot == itemType)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Count how many of a specific item type are in cargo.
        /// </summary>
        public int CountItem(InventoryItemType itemType)
        {
            int count = 0;
            foreach (var slot in cargoSlots)
            {
                if (slot == itemType)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Get all items currently in cargo (excludes empty slots).
        /// </summary>
        public List<InventoryItemType> GetAllItems()
        {
            List<InventoryItemType> items = new List<InventoryItemType>();
            foreach (var slot in cargoSlots)
            {
                if (slot != InventoryItemType.None)
                {
                    items.Add(slot);
                }
            }
            return items;
        }

        /// <summary>
        /// Get cargo fill percentage (0-1).
        /// </summary>
        public float GetFillPercentage()
        {
            return (float)GetCurrentCount() / maxSlots;
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Cargo: {GetCurrentCount()}/{maxSlots} ({GetFillPercentage() * 100:F0}%) | Full: {IsFull()}";
        }

        /// <summary>
        /// Get a formatted string of all cargo contents.
        /// </summary>
        public string GetCargoContents()
        {
            List<string> items = new List<string>();
            for (int i = 0; i < cargoSlots.Length; i++)
            {
                if (cargoSlots[i] != InventoryItemType.None)
                {
                    items.Add($"[{i}] {cargoSlots[i]}");
                }
            }
            return items.Count > 0 ? string.Join(", ", items) : "Empty";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Debug overlay
            int boxHeight = 160 + (maxSlots * 20);
            GUI.Box(new Rect(10, 330, 320, boxHeight), "Shared Cargo System (Debug)");

            int yOffset = 355;
            GUI.Label(new Rect(20, yOffset, 300, 20), $"Capacity: {GetCurrentCount()}/{maxSlots} ({GetFillPercentage() * 100:F0}%)");
            yOffset += 20;
            GUI.Label(new Rect(20, yOffset, 300, 20), $"Status: {(IsFull() ? "FULL" : $"{GetFreeSlots()} slots free")}");
            yOffset += 25;

            // Show cargo slots
            GUI.Label(new Rect(20, yOffset, 300, 20), "Cargo Slots:");
            yOffset += 20;

            for (int i = 0; i < cargoSlots.Length; i++)
            {
                string slotText = cargoSlots[i] == InventoryItemType.None ? "[Empty]" : cargoSlots[i].ToString();
                GUI.Label(new Rect(30, yOffset, 280, 20), $"Slot {i}: {slotText}");
                yOffset += 20;
            }

            yOffset += 5;

            // Test buttons
            if (GUI.Button(new Rect(20, yOffset, 140, 20), "Add PowerCore"))
            {
                TryAddItem(InventoryItemType.PowerCore);
            }
            if (GUI.Button(new Rect(170, yOffset, 140, 20), "Add Salvage"))
            {
                TryAddItem(InventoryItemType.ScrapMetal);
            }

            yOffset += 25;

            if (GUI.Button(new Rect(20, yOffset, 140, 20), "Remove PowerCore"))
            {
                RemoveItem(InventoryItemType.PowerCore);
            }
            if (GUI.Button(new Rect(170, yOffset, 140, 20), "Clear All"))
            {
                ClearCargo();
            }
        }
    }
}
