using System;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages the game's phase state machine and transitions between phases.
    /// Provides events for other systems to react to phase changes.
    /// </summary>
    public class RunPhaseController : MonoBehaviour
    {
        public static RunPhaseController Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private RunPhase currentPhase = RunPhase.Landing;

        [Header("Debug Settings")]
        [SerializeField] private bool enableKeyboardTriggers = false;
        [Tooltip("Auto-transition through phases with timers (for testing)")]
        [SerializeField] private bool autoTransition = false;
        [SerializeField] private float autoTransitionDelay = 5f;
        [SerializeField] private bool forceDisableDebugInput = true;

        [Header("Phase Timers (Read Only)")]
        [SerializeField] private float phaseStartTime;
        [SerializeField] private float phaseElapsedTime;

        private float autoTransitionTimer;

        // Events - other systems can subscribe to these
        public event Action<RunPhase, RunPhase> OnPhaseChanged; // old phase, new phase
        public event Action OnLandingEnter;
        public event Action OnLandingExit;
        public event Action OnExpeditionEnter;
        public event Action OnExpeditionExit;
        public event Action OnRunBackEnter;
        public event Action OnRunBackExit;
        public event Action OnPrepEnter;
        public event Action OnPrepExit;
        public event Action OnFinalStandEnter;
        public event Action OnFinalStandExit;
        public event Action OnEndSuccessEnter;
        public event Action OnEndFailEnter;

        public RunPhase CurrentPhase => currentPhase;
        public float PhaseElapsedTime => phaseElapsedTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (forceDisableDebugInput)
            {
                enableKeyboardTriggers = false;
                autoTransition = false;
            }
        }

        private void Start()
        {
            // Start in Landing phase
            TransitionToPhase(RunPhase.Landing);
        }

        private void Update()
        {
            phaseElapsedTime = Time.time - phaseStartTime;

            // Auto-transition for testing
            if (autoTransition)
            {
                autoTransitionTimer += Time.deltaTime;
                if (autoTransitionTimer >= autoTransitionDelay)
                {
                    autoTransitionTimer = 0f;
                    TransitionToNextPhase();
                }
            }

            // Keyboard triggers for manual testing
            if (enableKeyboardTriggers)
            {
                HandleKeyboardInput();
            }
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TransitionToPhase(RunPhase.Landing);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TransitionToPhase(RunPhase.Expedition);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TransitionToPhase(RunPhase.RunBack);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TransitionToPhase(RunPhase.Prep);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                TransitionToPhase(RunPhase.FinalStand);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                TransitionToPhase(RunPhase.EndSuccess);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                TransitionToPhase(RunPhase.EndFail);
            }
        }

        /// <summary>
        /// Transition to a specific phase.
        /// </summary>
        public void TransitionToPhase(RunPhase newPhase)
        {
            if (currentPhase == newPhase)
            {
                Debug.LogWarning($"Already in {newPhase} phase. Ignoring transition.");
                return;
            }

            RunPhase oldPhase = currentPhase;

            // Exit current phase
            ExitPhase(oldPhase);

            // Update phase
            currentPhase = newPhase;
            phaseStartTime = Time.time;
            phaseElapsedTime = 0f;
            autoTransitionTimer = 0f;

            // Enter new phase
            EnterPhase(newPhase);

            // Notify listeners
            OnPhaseChanged?.Invoke(oldPhase, newPhase);

            Debug.Log($"Phase Transition: {oldPhase} → {newPhase}");
        }

        /// <summary>
        /// Transition to the next logical phase in the sequence.
        /// </summary>
        public void TransitionToNextPhase()
        {
            RunPhase nextPhase = currentPhase switch
            {
                RunPhase.Landing => RunPhase.Expedition,
                RunPhase.Expedition => RunPhase.RunBack,
                RunPhase.RunBack => RunPhase.Prep,
                RunPhase.Prep => RunPhase.FinalStand,
                RunPhase.FinalStand => RunPhase.EndSuccess,
                RunPhase.EndSuccess => RunPhase.Landing,
                RunPhase.EndFail => RunPhase.Landing,
                _ => currentPhase
            };

            TransitionToPhase(nextPhase);
        }

        /// <summary>
        /// Transition to failure end state.
        /// </summary>
        public void TriggerDefeat()
        {
            TransitionToPhase(RunPhase.EndFail);
        }

        /// <summary>
        /// Transition to success end state.
        /// </summary>
        public void TriggerVictory()
        {
            TransitionToPhase(RunPhase.EndSuccess);
        }

        private void EnterPhase(RunPhase phase)
        {
            switch (phase)
            {
                case RunPhase.Landing:
                    EnterLanding();
                    break;
                case RunPhase.Expedition:
                    EnterExpedition();
                    break;
                case RunPhase.RunBack:
                    EnterRunBack();
                    break;
                case RunPhase.Prep:
                    EnterPrep();
                    break;
                case RunPhase.FinalStand:
                    EnterFinalStand();
                    break;
                case RunPhase.EndSuccess:
                    EnterEndSuccess();
                    break;
                case RunPhase.EndFail:
                    EnterEndFail();
                    break;
            }
        }

        private void ExitPhase(RunPhase phase)
        {
            switch (phase)
            {
                case RunPhase.Landing:
                    ExitLanding();
                    break;
                case RunPhase.Expedition:
                    ExitExpedition();
                    break;
                case RunPhase.RunBack:
                    ExitRunBack();
                    break;
                case RunPhase.Prep:
                    ExitPrep();
                    break;
                case RunPhase.FinalStand:
                    ExitFinalStand();
                    break;
                case RunPhase.EndSuccess:
                    ExitEndSuccess();
                    break;
                case RunPhase.EndFail:
                    ExitEndFail();
                    break;
            }
        }

        // Phase Enter/Exit Methods - placeholders for future implementation

        private void EnterLanding()
        {
            Debug.Log("[Landing] Entered - Safe bubble active, ready up players");
            OnLandingEnter?.Invoke();
        }

        private void ExitLanding()
        {
            Debug.Log("[Landing] Exited - Player left safe bubble");
            OnLandingExit?.Invoke();
        }

        private void EnterExpedition()
        {
            Debug.Log("[Expedition] Entered - Vampire Survivors style exploration begins");
            OnExpeditionEnter?.Invoke();
        }

        private void ExitExpedition()
        {
            Debug.Log("[Expedition] Exited - Last Critical Part picked up");
            OnExpeditionExit?.Invoke();
        }

        private void EnterRunBack()
        {
            Debug.Log("[RunBack] Entered - Chase mode! Hunters spawning!");
            OnRunBackEnter?.Invoke();
        }

        private void ExitRunBack()
        {
            Debug.Log("[RunBack] Exited - Reached ship");
            OnRunBackExit?.Invoke();
        }

        private void EnterPrep()
        {
            Debug.Log("[Prep] Entered - 15-20s to place deployables");
            OnPrepEnter?.Invoke();
        }

        private void ExitPrep()
        {
            Debug.Log("[Prep] Exited - Prep time over, Final Stand begins");
            OnPrepExit?.Invoke();
        }

        private void EnterFinalStand()
        {
            Debug.Log("[FinalStand] Entered - Defend ship + repair systems");
            OnFinalStandEnter?.Invoke();
        }

        private void ExitFinalStand()
        {
            Debug.Log("[FinalStand] Exited - Moving to end state");
            OnFinalStandExit?.Invoke();
        }

        private void EnterEndSuccess()
        {
            Debug.Log("[EndSuccess] Entered - VICTORY! Ship launched!");
            OnEndSuccessEnter?.Invoke();
        }

        private void ExitEndSuccess()
        {
            Debug.Log("[EndSuccess] Exited");
        }

        private void EnterEndFail()
        {
            Debug.Log("[EndFail] Entered - DEFEAT! Ship destroyed or team wiped");
            OnEndFailEnter?.Invoke();
        }

        private void ExitEndFail()
        {
            Debug.Log("[EndFail] Exited");
        }

        // Public helper methods for debugging in Inspector or other scripts

        public string GetPhaseDebugInfo()
        {
            return $"Phase: {currentPhase} | Elapsed: {phaseElapsedTime:F1}s";
        }

        private void OnGUI()
        {
            if (!enableKeyboardTriggers) return;

            // Simple debug overlay
            GUI.Box(new Rect(10, 10, 300, 180), "Phase State Machine (Debug)");
            GUI.Label(new Rect(20, 35, 280, 20), $"Current Phase: {currentPhase}");
            GUI.Label(new Rect(20, 55, 280, 20), $"Elapsed Time: {phaseElapsedTime:F2}s");
            GUI.Label(new Rect(20, 80, 280, 20), "Keyboard Controls:");
            GUI.Label(new Rect(20, 100, 280, 20), "1-7: Jump to phase");
            GUI.Label(new Rect(20, 120, 280, 20), "SPACE: Next phase");
            GUI.Label(new Rect(20, 140, 280, 20), "Auto: " + (autoTransition ? "ON" : "OFF"));

            if (autoTransition)
            {
                float remaining = autoTransitionDelay - autoTransitionTimer;
                GUI.Label(new Rect(20, 160, 280, 20), $"Next transition: {remaining:F1}s");
            }
        }
    }
}
