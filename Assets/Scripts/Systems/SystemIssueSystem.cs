using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Manages ship problems during Prep and FinalStand phases.
    /// Generates random problems and tracks repair progress.
    /// </summary>
    public class SystemIssueSystem : MonoBehaviour
    {
        public static SystemIssueSystem Instance { get; private set; }

        [Header("Problem Generation")]
        [Tooltip("Minimum number of problems to generate")]
        [SerializeField] private int minProblems = 3;

        [Tooltip("Maximum number of problems to generate")]
        [SerializeField] private int maxProblems = 3;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logProblemEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private List<SystemIssue> activeProblems = new List<SystemIssue>();
        [SerializeField] private bool allProblemsResolved = false;

        private RunPhase currentPhase = RunPhase.Landing;

        // Public accessors
        public bool IsActive => isActive;
        public List<SystemIssue> ActiveProblems => activeProblems;
        public bool AllProblemsResolved => allProblemsResolved;
        public int TotalProblems => activeProblems.Count;
        public int ResolvedProblems => activeProblems.Count(p => p.isResolved);

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
        }

        private void Start()
        {
            // Fallback subscription
            if (RunPhaseController.Instance != null)
            {
                SubscribeToPhaseEvents();
                currentPhase = RunPhaseController.Instance.CurrentPhase;
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

            // Check if all problems are resolved
            CheckAllProblemsResolved();
        }

        private void SubscribeToPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged += HandlePhaseChanged;
            RunPhaseController.Instance.OnPrepEnter += HandlePrepEnter;
            RunPhaseController.Instance.OnFinalStandEnter += HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit += HandleFinalStandExit;
            RunPhaseController.Instance.OnLandingEnter += HandleLandingEnter;
        }

        private void UnsubscribeFromPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged -= HandlePhaseChanged;
            RunPhaseController.Instance.OnPrepEnter -= HandlePrepEnter;
            RunPhaseController.Instance.OnFinalStandEnter -= HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit -= HandleFinalStandExit;
            RunPhaseController.Instance.OnLandingEnter -= HandleLandingEnter;
        }

        private void HandlePhaseChanged(RunPhase oldPhase, RunPhase newPhase)
        {
            currentPhase = newPhase;
            UpdateActiveState();
        }

        private void HandlePrepEnter()
        {
            GenerateProblems();
            isActive = true;
            Debug.Log("[SystemIssueSystem] Prep started - Ship problems ACTIVE");
        }

        private void HandleFinalStandEnter()
        {
            if (activeProblems.Count == 0)
            {
                GenerateProblems();
            }
            isActive = true;
            Debug.Log("[SystemIssueSystem] FinalStand started - Repair critical!");
        }

        private void HandleFinalStandExit()
        {
            isActive = false;
            Debug.Log("[SystemIssueSystem] FinalStand ended - System INACTIVE");
        }

        private void HandleLandingEnter()
        {
            ResetProblems();
            isActive = false;
            Debug.Log("[SystemIssueSystem] Landing - Problems cleared for new run");
        }

        private void UpdateActiveState()
        {
            isActive = (currentPhase == RunPhase.Prep || currentPhase == RunPhase.FinalStand);
        }

        /// <summary>
        /// Generate a random set of ship problems.
        /// </summary>
        private void GenerateProblems()
        {
            // Clear existing problems
            activeProblems.Clear();
            allProblemsResolved = false;

            // ONLY use problem types that have physical RepairStations in the scene
            // (PowerFailure, HullBreach, NavigationError)
            // LifeSupport and CommunicationLoss don't have stations yet
            SystemIssueType[] availableTypes = new SystemIssueType[]
            {
                SystemIssueType.PowerFailure,
                SystemIssueType.HullBreach,
                SystemIssueType.NavigationError
            };

            // Determine how many problems to create
            int problemCount = Random.Range(minProblems, maxProblems + 1);

            // Clamp to available types (can't create more problems than types)
            problemCount = Mathf.Min(problemCount, availableTypes.Length);

            // Shuffle and select random problems
            List<SystemIssueType> shuffledTypes = availableTypes.OrderBy(x => Random.value).ToList();

            for (int i = 0; i < problemCount; i++)
            {
                SystemIssue problem = new SystemIssue(shuffledTypes[i]);
                activeProblems.Add(problem);
            }

            if (logProblemEvents)
            {
                Debug.Log($"[SystemIssueSystem] Generated {problemCount} ship problems:");
                foreach (SystemIssue problem in activeProblems)
                {
                    Debug.Log($"  - {problem.problemType}");
                }
            }
        }

        /// <summary>
        /// Mark a specific problem as resolved.
        /// </summary>
        public void ResolveProblem(SystemIssueType problemType)
        {
            SystemIssue problem = activeProblems.FirstOrDefault(p => p.problemType == problemType);

            if (problem == null)
            {
                Debug.LogWarning($"[SystemIssueSystem] No active problem of type {problemType}");
                return;
            }

            problem.Resolve();
            CheckAllProblemsResolved();
        }

        /// <summary>
        /// Check if all problems are resolved.
        /// </summary>
        private void CheckAllProblemsResolved()
        {
            if (activeProblems.Count == 0)
            {
                allProblemsResolved = false;
                return;
            }

            bool wasResolved = allProblemsResolved;
            allProblemsResolved = activeProblems.All(p => p.isResolved);

            // Log when all problems become resolved
            if (allProblemsResolved && !wasResolved)
            {
                Debug.Log("[SystemIssueSystem] ✓ Ship fully repaired! All systems operational!");
            }
        }

        /// <summary>
        /// Get a specific problem by type.
        /// </summary>
        public SystemIssue GetProblem(SystemIssueType problemType)
        {
            return activeProblems.FirstOrDefault(p => p.problemType == problemType);
        }

        /// <summary>
        /// Check if a specific problem exists and is unresolved.
        /// </summary>
        public bool HasUnresolvedProblem(SystemIssueType problemType)
        {
            SystemIssue problem = GetProblem(problemType);
            return problem != null && !problem.isResolved;
        }

        /// <summary>
        /// Reset all problems (for new run).
        /// </summary>
        private void ResetProblems()
        {
            activeProblems.Clear();
            allProblemsResolved = false;
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            if (activeProblems.Count == 0)
                return "No problems generated";

            return $"Resolved: {ResolvedProblems}/{TotalProblems} | All Fixed: {(allProblemsResolved ? "YES" : "NO")}";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay || !isActive) return;

            // Debug overlay
            int boxWidth = 350;
            int boxHeight = 100 + (activeProblems.Count * 20);
            int xPos = 10;
            int yPos = 200;

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Ship Problems (Debug)");

            int yOffset = yPos + 25;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Phase: {currentPhase}");
            yOffset += 20;

            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Problems: {ResolvedProblems}/{TotalProblems} resolved");
            yOffset += 20;

            // List all problems
            foreach (SystemIssue problem in activeProblems)
            {
                if (problem.isResolved)
                {
                    GUI.color = Color.green;
                    GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"✓ {problem.problemType}");
                    GUI.color = Color.white;
                }
                else
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"⚠ {problem.problemType}");
                    GUI.color = Color.white;
                }
                yOffset += 20;
            }

            yOffset += 5;

            // Highlight when all resolved
            if (allProblemsResolved)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "✓ SHIP FULLY REPAIRED!");
                GUI.color = Color.white;
            }
        }
    }
}
