using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Example script showing how other systems can subscribe to phase changes.
    /// Attach this to any GameObject to see phase transition events in action.
    /// </summary>
    public class PhaseEventListenerExample : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool logPhaseChanges = true;
        [SerializeField] private bool logSpecificPhases = true;

        private void OnEnable()
        {
            if (RunPhaseController.Instance == null)
            {
                Debug.LogWarning("RunPhaseController not found. Make sure it exists in the scene.");
                return;
            }

            // Subscribe to general phase change event
            RunPhaseController.Instance.OnPhaseChanged += HandlePhaseChanged;

            // Subscribe to specific phase events (optional)
            if (logSpecificPhases)
            {
                RunPhaseController.Instance.OnExpeditionEnter += HandleExpeditionStart;
                RunPhaseController.Instance.OnRunBackEnter += HandleRunBackStart;
                RunPhaseController.Instance.OnFinalStandEnter += HandleFinalStandStart;
            }
        }

        private void OnDisable()
        {
            if (RunPhaseController.Instance == null) return;

            // Unsubscribe to prevent memory leaks
            RunPhaseController.Instance.OnPhaseChanged -= HandlePhaseChanged;

            if (logSpecificPhases)
            {
                RunPhaseController.Instance.OnExpeditionEnter -= HandleExpeditionStart;
                RunPhaseController.Instance.OnRunBackEnter -= HandleRunBackStart;
                RunPhaseController.Instance.OnFinalStandEnter -= HandleFinalStandStart;
            }
        }

        private void HandlePhaseChanged(RunPhase oldPhase, RunPhase newPhase)
        {
            if (logPhaseChanges)
            {
                Debug.Log($"[PhaseListener] Phase changed: {oldPhase} → {newPhase}");
            }

            // Example: React to specific transitions
            if (newPhase == RunPhase.EndSuccess)
            {
                Debug.Log("[PhaseListener] Victory! Time to celebrate!");
            }
            else if (newPhase == RunPhase.EndFail)
            {
                Debug.Log("[PhaseListener] Defeat! Better luck next time!");
            }
        }

        private void HandleExpeditionStart()
        {
            Debug.Log("[PhaseListener] Expedition started - time to explore and gather!");
        }

        private void HandleRunBackStart()
        {
            Debug.Log("[PhaseListener] Run Back started - GET TO THE SHIP!");
        }

        private void HandleFinalStandStart()
        {
            Debug.Log("[PhaseListener] Final Stand started - defend and repair!");
        }
    }
}
