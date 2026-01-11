using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Repair station for a specific ship problem.
    /// Player must hold interaction key for specified duration to repair.
    /// Progress resets if interrupted.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class IssueRepairStation : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Which ship problem this station repairs")]
        [SerializeField] private SystemIssueType problemType;

        [Tooltip("Time in seconds to complete repair")]
        [SerializeField] private float repairDuration = 3f;

        [Tooltip("Interaction key to hold for repair")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        [Tooltip("Player tag for detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logRepairEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isPlayerNearby = false;
        [SerializeField] private bool isRepairing = false;
        [SerializeField] private bool isRepaired = false;
        [SerializeField] private float repairProgress = 0f;

        private GameObject nearbyPlayer;
        private CharacterHealth nearbyPlayerHealth;
        private Collider triggerCollider;

        public bool IsRepaired => isRepaired;
        public float RepairProgress => repairProgress;
        public float RepairProgressPercent => (repairProgress / repairDuration) * 100f;

        private void Awake()
        {
            // Ensure we have a trigger collider
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                Debug.LogError("[IssueRepairStation] No Collider found! Adding BoxCollider.", this);
                triggerCollider = gameObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            // Skip if already repaired
            if (isRepaired)
                return;

            // Check if we're in correct phase (FinalStand only)
            if (!IsValidPhase())
                return;

            // Check if this problem actually exists in SystemIssueSystem
            if (!IsProblemActive())
                return;

            // Check if player is holding interaction key
            if (isPlayerNearby && Input.GetKey(interactionKey))
            {
                // Start or continue repair
                if (!isRepairing)
                {
                    StartRepair();
                }

                UpdateRepair();
            }
            else
            {
                // Stop repairing if key released or player left
                if (isRepairing)
                {
                    CancelRepair();
                }
            }
        }

        /// <summary>
        /// Check if current phase allows repairs.
        /// </summary>
        private bool IsValidPhase()
        {
            if (RunPhaseController.Instance == null)
                return false;

            RunPhase currentPhase = RunPhaseController.Instance.CurrentPhase;
            // Only allow repairs during FinalStand (not Prep)
            return currentPhase == RunPhase.FinalStand;
        }

        /// <summary>
        /// Check if this station's problem is active in SystemIssueSystem.
        /// </summary>
        private bool IsProblemActive()
        {
            if (SystemIssueSystem.Instance == null)
                return false;

            if (!SystemIssueSystem.Instance.IsActive)
                return false;

            // Check if our problem type exists in active problems
            foreach (var problem in SystemIssueSystem.Instance.ActiveProblems)
            {
                if (problem.problemType == problemType && !problem.isResolved)
                    return true;
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player entered trigger
            if (!other.CompareTag(playerTag))
                return;

            isPlayerNearby = true;
            nearbyPlayer = other.gameObject;

            // Get CharacterHealth component and subscribe to damage events
            nearbyPlayerHealth = other.GetComponent<CharacterHealth>();
            if (nearbyPlayerHealth != null)
            {
                nearbyPlayerHealth.OnDamaged += HandlePlayerDamaged;
            }

            if (logRepairEvents && !isRepaired && IsValidPhase() && IsProblemActive())
            {
                Debug.Log($"[IssueRepairStation] {nearbyPlayer.name} approached {problemType} station. Press {interactionKey} to repair.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Check if player left trigger
            if (!other.CompareTag(playerTag))
                return;

            // Unsubscribe from damage events
            if (nearbyPlayerHealth != null)
            {
                nearbyPlayerHealth.OnDamaged -= HandlePlayerDamaged;
                nearbyPlayerHealth = null;
            }

            isPlayerNearby = false;
            nearbyPlayer = null;

            if (isRepairing && !isRepaired)
            {
                CancelRepair();
            }
        }

        private void StartRepair()
        {
            isRepairing = true;
            repairProgress = 0f;

            if (logRepairEvents)
            {
                Debug.Log($"[IssueRepairStation] Started repairing {problemType} (hold {interactionKey} for {repairDuration}s)");
            }
        }

        private void UpdateRepair()
        {
            repairProgress += Time.deltaTime;

            // Check if repair is complete
            if (repairProgress >= repairDuration)
            {
                CompleteRepair();
            }
        }

        private void CancelRepair()
        {
            if (logRepairEvents)
            {
                Debug.Log($"[IssueRepairStation] Repair of {problemType} interrupted! Progress lost ({repairProgress:F1}s/{repairDuration}s)");
            }

            isRepairing = false;
            repairProgress = 0f;
        }

        /// <summary>
        /// Handle player taking damage - interrupt repair if active.
        /// </summary>
        private void HandlePlayerDamaged(float damageAmount)
        {
            // Only interrupt if currently repairing
            if (!isRepairing)
                return;

            if (logRepairEvents)
            {
                Debug.Log($"[IssueRepairStation] Repair of {problemType} INTERRUPTED BY DAMAGE! Player took {damageAmount} damage. Progress lost ({repairProgress:F1}s/{repairDuration}s)");
            }

            isRepairing = false;
            repairProgress = 0f;
        }

        private void CompleteRepair()
        {
            isRepairing = false;
            isRepaired = true;
            repairProgress = repairDuration;

            if (logRepairEvents)
            {
                Debug.Log($"[IssueRepairStation] ✓ {problemType} repair COMPLETE!");
            }

            // Notify SystemIssueSystem
            if (SystemIssueSystem.Instance != null)
            {
                SystemIssueSystem.Instance.ResolveProblem(problemType);
            }
            else
            {
                Debug.LogError("[IssueRepairStation] SystemIssueSystem not found! Cannot mark problem as resolved.");
            }
        }

        /// <summary>
        /// Reset station to unrepaired state (for new runs).
        /// </summary>
        public void ResetStation()
        {
            isRepaired = false;
            isRepairing = false;
            repairProgress = 0f;
            Debug.Log($"[IssueRepairStation] {problemType} station reset");
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            if (isRepaired)
                return $"{problemType}: ✓ REPAIRED";

            if (isRepairing)
                return $"{problemType}: Repairing... {repairProgress:F1}s/{repairDuration}s ({RepairProgressPercent:F0}%)";

            if (isPlayerNearby)
                return $"{problemType}: Press {interactionKey} to repair";

            return $"{problemType}: ⚠ NEEDS REPAIR";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Only show if valid phase and problem is active
            if (!IsValidPhase() || !IsProblemActive())
            {
                // Exception: show if already repaired (to confirm completion)
                if (!isRepaired)
                    return;
            }

            // Only show if player is nearby
            if (!isPlayerNearby && !isRepaired)
                return;

            // Calculate screen position above the station
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

            if (screenPos.z <= 0)
                return; // Behind camera

            int boxWidth = 300;
            int boxHeight = 80;
            int xPos = (int)screenPos.x - boxWidth / 2;
            int yPos = Screen.height - (int)screenPos.y - boxHeight;

            // Background box
            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), $"{problemType} Station");

            int yOffset = yPos + 25;

            if (isRepaired)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "✓ REPAIRED");
                GUI.color = Color.white;
            }
            else if (isRepairing)
            {
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Repairing: {repairProgress:F1}s / {repairDuration}s");
                yOffset += 20;

                // Progress bar
                float progressWidth = (boxWidth - 20) * (repairProgress / repairDuration);
                GUI.color = Color.yellow;
                GUI.Box(new Rect(xPos + 10, yOffset, progressWidth, 20), "");
                GUI.color = Color.white;
                GUI.Box(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "");
            }
            else if (isPlayerNearby)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Press [{interactionKey}] to repair");
                GUI.color = Color.white;
                yOffset += 20;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Takes {repairDuration}s (hold key)");
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            // Draw repair station bounds
            if (triggerCollider == null)
                triggerCollider = GetComponent<Collider>();

            if (triggerCollider != null)
            {
                // Color based on state
                if (isRepaired)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green - repaired
                }
                else if (isRepairing)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Yellow - in progress
                }
                else
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red - needs repair
                }

                Gizmos.matrix = transform.localToWorldMatrix;

                if (triggerCollider is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (triggerCollider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range more prominently when selected
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
