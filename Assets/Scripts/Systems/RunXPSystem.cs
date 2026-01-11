using System;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Tracks player XP and level progression during a run.
    /// XP resets between runs (no permanent progression).
    /// </summary>
    public class RunXPSystem : MonoBehaviour
    {
        public static RunXPSystem Instance { get; private set; }

        [Header("Current Progress")]
        [SerializeField] private int currentXP = 0;
        [SerializeField] private int currentLevel = 1;

        [Header("Level Thresholds")]
        [Tooltip("XP required for each level. Index 0 = level 2, index 1 = level 3, etc.")]
        [SerializeField] private int[] xpThresholds = new int[]
        {
            10,   // Level 2
            25,   // Level 3
            50,   // Level 4
            80,   // Level 5
            120   // Level 6 (if you want more than 5 levels)
        };

        [Header("Settings")]
        [Tooltip("Maximum level player can reach in a run (0 = unlimited)")]
        [SerializeField] private int maxLevel = 0;

        [Header("Debug")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logXPGains = true;

        // Events
        public event Action<int> OnLevelUp; // passes new level
        public event Action<int> OnXPGained; // passes XP amount

        // Public accessors
        public int CurrentXP => currentXP;
        public int CurrentLevel => currentLevel;
        public int MaxLevel => maxLevel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
            }
        }

        private void OnDisable()
        {
            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnLandingEnter -= HandleLandingEnter;
            }
        }

        private void HandleLandingEnter()
        {
            // Reset XP and level for new run
            ResetProgress();
        }

        /// <summary>
        /// Add XP and check for level-up.
        /// </summary>
        public void AddXP(int amount)
        {
            if (IsMaxLevel())
            {
                // Already at max level, don't gain more XP
                return;
            }

            currentXP += amount;

            if (logXPGains)
            {
                Debug.Log($"[RunXPSystem] Gained {amount} XP → Total: {currentXP}/{GetXPForNextLevel()}");
            }

            OnXPGained?.Invoke(amount);

            // Check for level-up
            CheckLevelUp();
        }

        private void CheckLevelUp()
        {
            if (IsMaxLevel())
            {
                return;
            }

            int requiredXP = GetXPForNextLevel();

            if (currentXP >= requiredXP)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            currentLevel++;

            Debug.Log($"[RunXPSystem] LEVEL UP! Now level {currentLevel}");

            OnLevelUp?.Invoke(currentLevel);
        }

        /// <summary>
        /// Get XP required for next level.
        /// </summary>
        public int GetXPForNextLevel()
        {
            if (IsMaxLevel())
            {
                return int.MaxValue; // Already at max
            }

            int thresholdIndex = currentLevel - 1; // Level 2 is at index 0

            if (thresholdIndex >= 0 && thresholdIndex < xpThresholds.Length)
            {
                return xpThresholds[thresholdIndex];
            }

            // Fallback if thresholds array is too short
            return 999999;
        }

        /// <summary>
        /// Get XP progress toward next level (0-1).
        /// </summary>
        public float GetProgressToNextLevel()
        {
            if (IsMaxLevel())
            {
                return 1f; // Max level reached
            }

            int requiredXP = GetXPForNextLevel();
            return Mathf.Clamp01((float)currentXP / requiredXP);
        }

        /// <summary>
        /// Reset XP and level (called when starting new run).
        /// </summary>
        public void ResetProgress()
        {
            currentXP = 0;
            currentLevel = 1;

            Debug.Log("[RunXPSystem] Progress reset for new run");
        }

        /// <summary>
        /// Check if player is at max level.
        /// </summary>
        public bool IsMaxLevel()
        {
            return maxLevel > 0 && currentLevel >= maxLevel;
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            GUI.Box(new Rect(320, 550, 300, 80), "XP System");

            int yOffset = 575;
            GUI.Label(new Rect(330, yOffset, 280, 20), $"Level: {currentLevel}/{maxLevel}");
            yOffset += 20;

            if (currentLevel < maxLevel)
            {
                int required = GetXPForNextLevel();
                GUI.Label(new Rect(330, yOffset, 280, 20), $"XP: {currentXP}/{required}");
                yOffset += 20;

                float progress = GetProgressToNextLevel();
                GUI.Label(new Rect(330, yOffset, 280, 20), $"Progress: {progress * 100:F0}%");
            }
            else
            {
                GUI.Label(new Rect(330, yOffset, 280, 20), "MAX LEVEL");
            }
        }
    }
}
