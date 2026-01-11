using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages the threat escalation system.
    /// ThreatLevel: live danger during Expedition/RunBack (affects spawn intensity)
    /// ThreatDebt: accumulated debt that scales Final Stand difficulty
    /// </summary>
    public class PressureSystem : MonoBehaviour
    {
        public static PressureSystem Instance { get; private set; }

        [Header("Threat Values (Read Only)")]
        [SerializeField] private float threatLevel = 0f;
        [SerializeField] private float threatDebt = 0f;
        [SerializeField] private int enemyKillCount = 0;

        [Header("Tunable Parameters")]
        [Tooltip("Base rate at which ThreatLevel increases per second")]
        [SerializeField] private float baseThreatRate = 1f;

        [Tooltip("Rate at which ThreatDebt accumulates per second during active phases")]
        [SerializeField] private float threatDebtRate = 0.5f;

        [Tooltip("ThreatDebt added per enemy kill")]
        [SerializeField] private float debtPerKill = 2f;

        [Tooltip("ThreatDebt added per salvage pickup")]
        [SerializeField] private float debtPerSalvage = 5f;

        [Header("Mission Modifiers")]
        [Tooltip("Multiplier applied to base threat growth (1 = default)")]
        [SerializeField] private float threatGrowthMultiplier = 1f;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logThreatChanges = false;

        private bool isAccumulatingThreat = false;
        private RunPhase currentPhase = RunPhase.Landing;

        // Public accessors
        public float ThreatLevel => threatLevel;
        public float ThreatDebt => threatDebt;
        public bool IsAccumulatingThreat => isAccumulatingThreat;
        public int EnemyKillCount => enemyKillCount;

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
                SubscribeToPhaseEvents();
            }
            else
            {
                Debug.LogWarning("[PressureSystem] RunPhaseController not found at OnEnable. Will attempt to subscribe later.");
            }
        }

        private void Start()
        {
            // Fallback: try subscribing again in Start if not done in OnEnable
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromPhaseEvents();
        }

        private void Update()
        {
            if (!isAccumulatingThreat) return;

            float deltaTime = Time.deltaTime;

            // Increase ThreatLevel
            float previousThreatLevel = threatLevel;
            threatLevel += baseThreatRate * threatGrowthMultiplier * deltaTime;

            // Increase ThreatDebt
            float previousThreatDebt = threatDebt;
            threatDebt += threatDebtRate * deltaTime;

            // Optional logging for debugging
            if (logThreatChanges && Time.frameCount % 60 == 0) // Log every ~60 frames
            {
                Debug.Log($"[PressureSystem] ThreatLevel: {threatLevel:F2} | ThreatDebt: {threatDebt:F2}");
            }
        }

        private void SubscribeToPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            // Subscribe to phase change event
            RunPhaseController.Instance.OnPhaseChanged += HandlePhaseChanged;

            // Subscribe to specific phase enters for clarity
            RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
            RunPhaseController.Instance.OnExpeditionEnter += HandleExpeditionEnter;
            RunPhaseController.Instance.OnRunBackEnter += HandleRunBackEnter;
            RunPhaseController.Instance.OnPrepEnter += HandlePrepEnter;
            RunPhaseController.Instance.OnFinalStandEnter += HandleFinalStandEnter;
        }

        private void UnsubscribeFromPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged -= HandlePhaseChanged;
            RunPhaseController.Instance.OnLandingEnter -= HandleLandingEnter;
            RunPhaseController.Instance.OnExpeditionEnter -= HandleExpeditionEnter;
            RunPhaseController.Instance.OnRunBackEnter -= HandleRunBackEnter;
            RunPhaseController.Instance.OnPrepEnter -= HandlePrepEnter;
            RunPhaseController.Instance.OnFinalStandEnter -= HandleFinalStandEnter;
        }

        private void HandlePhaseChanged(RunPhase oldPhase, RunPhase newPhase)
        {
            currentPhase = newPhase;
        }

        private void HandleLandingEnter()
        {
            // Reset ThreatLevel when returning to Landing (new run)
            float previousLevel = threatLevel;
            threatLevel = 0f;
            enemyKillCount = 0;
            isAccumulatingThreat = false;

            Debug.Log($"[PressureSystem] Landing entered - ThreatLevel reset to 0 (was {previousLevel:F2})");
            Debug.Log($"[PressureSystem] ThreatDebt preserved: {threatDebt:F2}");
        }

        private void HandleExpeditionEnter()
        {
            // Start accumulating threat during Expedition
            isAccumulatingThreat = true;
            Debug.Log("[PressureSystem] Expedition entered - Threat accumulation ACTIVE");
        }

        private void HandleRunBackEnter()
        {
            // Continue accumulating threat during RunBack (chase mode)
            isAccumulatingThreat = true;
            Debug.Log("[PressureSystem] RunBack entered - Threat accumulation ACTIVE (chase mode)");
        }

        private void HandlePrepEnter()
        {
            // Pause threat accumulation during Prep phase
            isAccumulatingThreat = false;
            Debug.Log($"[PressureSystem] Prep entered - Threat accumulation PAUSED | ThreatLevel: {threatLevel:F2} | ThreatDebt: {threatDebt:F2}");
        }

        private void HandleFinalStandEnter()
        {
            // Pause threat accumulation during Final Stand
            isAccumulatingThreat = false;
            Debug.Log($"[PressureSystem] FinalStand entered - Threat accumulation PAUSED | Final ThreatDebt: {threatDebt:F2}");
        }

        // Public methods for external systems to register events

        /// <summary>
        /// Register an enemy kill. Adds debt to ThreatDebt.
        /// </summary>
        public void RegisterEnemyKill()
        {
            threatDebt += debtPerKill;
            enemyKillCount++;

            if (logThreatChanges)
            {
                Debug.Log($"[PressureSystem] Enemy killed! ThreatDebt +{debtPerKill} → {threatDebt:F2}");
            }
        }

        /// <summary>
        /// Register a salvage pickup. Adds debt to ThreatDebt.
        /// </summary>
        public void RegisterSalvagePickup()
        {
            threatDebt += debtPerSalvage;

            if (logThreatChanges)
            {
                Debug.Log($"[PressureSystem] Salvage picked up! ThreatDebt +{debtPerSalvage} → {threatDebt:F2}");
            }
        }

        /// <summary>
        /// Manually add to ThreatLevel (for special events, cargo weight, etc.)
        /// </summary>
        public void AddThreatLevel(float amount)
        {
            threatLevel += amount;

            if (logThreatChanges)
            {
                Debug.Log($"[PressureSystem] ThreatLevel increased by {amount} → {threatLevel:F2}");
            }
        }

        /// <summary>
        /// Manually add to ThreatDebt (for special events)
        /// </summary>
        public void AddThreatDebt(float amount)
        {
            threatDebt += amount;

            if (logThreatChanges)
            {
                Debug.Log($"[PressureSystem] ThreatDebt increased by {amount} → {threatDebt:F2}");
            }
        }

        /// <summary>
        /// Get threat level in a categorized form (Low/Medium/High)
        /// Useful for UI and spawn intensity decisions
        /// </summary>
        public string GetThreatLevelCategory()
        {
            if (threatLevel < 50f) return "Low";
            if (threatLevel < 150f) return "Medium";
            if (threatLevel < 300f) return "High";
            return "Critical";
        }

        /// <summary>
        /// Get debug info string
        /// </summary>
        public string GetDebugInfo()
        {
            return $"ThreatLevel: {threatLevel:F1} ({GetThreatLevelCategory()}) | ThreatDebt: {threatDebt:F1} | Active: {isAccumulatingThreat}";
        }

        public void SetStartingThreat(int startingThreat)
        {
            threatLevel = Mathf.Max(0f, startingThreat);
        }

        public void SetThreatGrowthMultiplier(float multiplier)
        {
            threatGrowthMultiplier = Mathf.Max(0f, multiplier);
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Debug overlay
            GUI.Box(new Rect(10, 200, 320, 120), "Threat System (Debug)");
            GUI.Label(new Rect(20, 225, 300, 20), $"Phase: {currentPhase}");
            GUI.Label(new Rect(20, 245, 300, 20), $"ThreatLevel: {threatLevel:F2} ({GetThreatLevelCategory()})");
            GUI.Label(new Rect(20, 265, 300, 20), $"ThreatDebt: {threatDebt:F2}");
            GUI.Label(new Rect(20, 285, 300, 20), $"Accumulating: {(isAccumulatingThreat ? "YES" : "NO")}");

            // Test buttons
            if (GUI.Button(new Rect(20, 305, 140, 20), "Register Kill"))
            {
                RegisterEnemyKill();
            }
            if (GUI.Button(new Rect(170, 305, 140, 20), "Register Salvage"))
            {
                RegisterSalvagePickup();
            }
        }
    }
}
