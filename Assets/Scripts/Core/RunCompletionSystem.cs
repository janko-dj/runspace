using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages victory condition during FinalStand phase.
    /// Triggers victory when all required repairs are complete.
    /// </summary>
    public class RunCompletionSystem : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the ship zone trigger")]
        [SerializeField] private BaseZone shipZone;

        [Header("Countdown Settings")]
        [Tooltip("Time in seconds all players must stay in zone to trigger victory")]
        [SerializeField] private float countdownDuration = 8f;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logCountdownEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private bool isCountingDown = false;
        [SerializeField] private float countdownTimer = 0f;

        private RunPhase currentPhase = RunPhase.Landing;

        private void OnEnable()
        {
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
            }
        }

        private void Start()
        {
            // Fallback subscription
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
                currentPhase = RunPhaseController.Instance.CurrentPhase;
                UpdateActiveState();
            }

            // Validation
            if (shipZone == null)
            {
                Debug.LogError("[RunCompletionSystem] No BaseZone assigned! Victory system will not work.", this);
            }

            // Auto-find BaseZone if not assigned
            if (shipZone == null)
            {
            shipZone = Object.FindFirstObjectByType<BaseZone>();
                if (shipZone != null)
                {
                    Debug.Log("[RunCompletionSystem] Auto-found BaseZone reference");
                }
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromPhaseEvents();
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (currentPhase == RunPhase.FinalStand)
            {
                CheckFinalStandVictory();
            }
        }

        private void CheckFinalStandVictory()
        {
            // Check if all ship problems are resolved
            if (SystemIssueSystem.Instance == null)
                return;

            if (SystemIssueSystem.Instance.AllProblemsResolved)
            {
                if (logCountdownEvents)
                {
                    Debug.Log("[RunCompletionSystem] All repairs complete! Triggering VICTORY!");
                }

                TriggerVictory();
            }
        }

        private void SubscribeToPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged += HandlePhaseChanged;
            RunPhaseController.Instance.OnFinalStandEnter += HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit += HandleFinalStandExit;
        }

        private void UnsubscribeFromPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged -= HandlePhaseChanged;
            RunPhaseController.Instance.OnFinalStandEnter -= HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit -= HandleFinalStandExit;
        }

        private void HandlePhaseChanged(RunPhase oldPhase, RunPhase newPhase)
        {
            currentPhase = newPhase;
            UpdateActiveState();
        }

        private void HandleFinalStandEnter()
        {
            isActive = true;
            ResetCountdown();
            Debug.Log("[RunCompletionSystem] FinalStand started - Victory condition ACTIVE (repair all systems)");
        }

        private void HandleFinalStandExit()
        {
            isActive = false;
            ResetCountdown();
            Debug.Log("[RunCompletionSystem] FinalStand ended - Victory condition INACTIVE");
        }

        private void UpdateActiveState()
        {
            isActive = (currentPhase == RunPhase.FinalStand);
        }

        private void StartCountdown()
        {
            isCountingDown = true;
            countdownTimer = 0f;

            if (logCountdownEvents)
            {
                Debug.Log($"[RunCompletionSystem] All players in zone! Countdown started ({countdownDuration}s)");
            }
        }

        private void CancelCountdown()
        {
            isCountingDown = false;
            countdownTimer = 0f;

            if (logCountdownEvents)
            {
                Debug.Log("[RunCompletionSystem] Player left zone! Countdown cancelled");
            }
        }

        private void ResetCountdown()
        {
            isCountingDown = false;
            countdownTimer = 0f;
        }

        private void TriggerVictory()
        {
            if (logCountdownEvents)
            {
                Debug.Log("[RunCompletionSystem] Countdown complete! Triggering VICTORY!");
            }

            // Prevent repeated triggers
            isActive = false;
            isCountingDown = false;

            // Trigger victory through RunPhaseController
            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.TriggerVictory();
            }
            else
            {
                Debug.LogError("[RunCompletionSystem] Cannot trigger victory - RunPhaseController not found!");
            }
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            if (!isActive)
                return "INACTIVE (not in FinalStand)";

            if (isCountingDown)
            {
                float remaining = countdownDuration - countdownTimer;
                return $"COUNTING DOWN: {remaining:F1}s";
            }

            return "Waiting for repairs to complete";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            if (!isActive) return;

            // Debug overlay
            int boxWidth = 320;
            int boxHeight = 120;
            int xPos = Screen.width - boxWidth - 10;
            int yPos = 120; // Below BaseZone overlay

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Victory Condition (Debug)");

            int yOffset = yPos + 25;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Phase: {currentPhase}");
            yOffset += 20;

            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Active: {(isActive ? "YES" : "NO")}");
            yOffset += 20;

            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Waiting for repairs to complete...");
        }
    }
}
