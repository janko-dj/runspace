using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Predefined turret placement socket.
    /// Player can activate to spawn turret if they have enough Defense Points.
    /// Only works during Prep and FinalStand phases.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DeploymentSocket : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Turret prefab to spawn at this socket")]
        [SerializeField] private GameObject turretPrefab;

        [Tooltip("Defense Points cost to place turret")]
        [SerializeField] private int turretCost = 50;

        [Tooltip("Key to press to activate socket")]
        [SerializeField] private KeyCode activationKey = KeyCode.T;

        [Tooltip("Player tag for detection")]
        [SerializeField] private string playerTag = "Player";

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logPlacementEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isPlayerNearby = false;
        [SerializeField] private bool turretPlaced = false;

        private GameObject placedTurret;
        private Collider triggerCollider;

        public bool IsOccupied => turretPlaced;

        private void Awake()
        {
            // Ensure we have a trigger collider
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                Debug.LogError("[DeploymentSocket] No Collider found! Adding BoxCollider.", this);
                triggerCollider = gameObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            // Skip if turret already placed
            if (turretPlaced)
                return;

            // Skip if player not nearby
            if (!isPlayerNearby)
                return;

            // Check if phase allows placement
            if (!IsValidPhase())
                return;

            // Check for activation key press
            if (Input.GetKeyDown(activationKey))
            {
                TryPlaceTurret();
            }
        }

        /// <summary>
        /// Check if current phase allows turret placement.
        /// </summary>
        private bool IsValidPhase()
        {
            if (RunPhaseController.Instance == null)
                return false;

            RunPhase currentPhase = RunPhaseController.Instance.CurrentPhase;
            return currentPhase == RunPhase.Prep || currentPhase == RunPhase.FinalStand;
        }

        /// <summary>
        /// Attempt to place turret at this socket.
        /// </summary>
        private void TryPlaceTurret()
        {
            // Check if DeploymentPointsSystem exists
            if (DeploymentPointsSystem.Instance == null)
            {
                Debug.LogError("[DeploymentSocket] DeploymentPointsSystem not found!");
                return;
            }

            // Check if player has enough DP
            if (!DeploymentPointsSystem.Instance.HasEnoughDP(turretCost))
            {
                if (logPlacementEvents)
                {
                    Debug.LogWarning($"[DeploymentSocket] Not enough DP! Need {turretCost}, have {DeploymentPointsSystem.Instance.CurrentDefensePoints}");
                }
                return;
            }

            // Check if turret prefab is assigned
            if (turretPrefab == null)
            {
                Debug.LogError("[DeploymentSocket] No turret prefab assigned!", this);
                return;
            }

            // Spend DP
            bool success = DeploymentPointsSystem.Instance.SpendDefensePoints(turretCost);
            if (!success)
            {
                Debug.LogError("[DeploymentSocket] Failed to spend DP!");
                return;
            }

            // Spawn turret
            placedTurret = Instantiate(turretPrefab, transform.position, transform.rotation);
            turretPlaced = true;

            if (logPlacementEvents)
            {
                Debug.Log($"[DeploymentSocket] Turret placed at {transform.position} for {turretCost} DP");
            }

            // Monitor turret destruction to reset socket
            DefenseEmplacement turret = placedTurret.GetComponent<DefenseEmplacement>();
            if (turret != null)
            {
                StartCoroutine(MonitorTurretHealth(turret));
            }
        }

        /// <summary>
        /// Monitor turret health and reset socket when destroyed.
        /// </summary>
        private System.Collections.IEnumerator MonitorTurretHealth(DefenseEmplacement turret)
        {
            while (turret != null && turret.IsAlive)
            {
                yield return new WaitForSeconds(0.5f);
            }

            // Turret was destroyed
            if (logPlacementEvents)
            {
                Debug.Log($"[DeploymentSocket] Turret at {transform.position} was destroyed. Socket available again.");
            }

            turretPlaced = false;
            placedTurret = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            isPlayerNearby = true;

            if (logPlacementEvents && !turretPlaced && IsValidPhase())
            {
                Debug.Log($"[DeploymentSocket] Player near socket. Press [{activationKey}] to place turret ({turretCost} DP)");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            isPlayerNearby = false;
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            if (turretPlaced)
                return "Socket: Occupied";
            else if (isPlayerNearby)
                return $"Socket: Press [{activationKey}] ({turretCost} DP)";
            else
                return "Socket: Available";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay)
                return;

            // Only show if player is nearby or turret is placed
            if (!isPlayerNearby && !turretPlaced)
                return;

            // Only show during valid phases
            if (!turretPlaced && !IsValidPhase())
                return;

            // Calculate screen position above the socket
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

            if (screenPos.z <= 0)
                return; // Behind camera

            int boxWidth = 280;
            int boxHeight = 80;
            int xPos = (int)screenPos.x - boxWidth / 2;
            int yPos = Screen.height - (int)screenPos.y - boxHeight;

            if (turretPlaced)
            {
                // Show occupied status
                GUI.color = Color.gray;
                GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Turret Socket");
                GUI.color = Color.white;

                int yOffset = yPos + 25;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "✓ Turret Deployed");
            }
            else
            {
                // Show placement prompt
                GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Turret Socket");

                int yOffset = yPos + 25;

                if (DeploymentPointsSystem.Instance != null)
                {
                    bool hasEnoughDP = DeploymentPointsSystem.Instance.HasEnoughDP(turretCost);

                    if (hasEnoughDP)
                    {
                        GUI.color = Color.green;
                        GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Press [{activationKey}] to deploy turret");
                        GUI.color = Color.white;
                    }
                    else
                    {
                        GUI.color = Color.red;
                        GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Not enough DP!");
                        GUI.color = Color.white;
                    }

                    yOffset += 20;
                    GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Cost: {turretCost} DP");
                    yOffset += 20;
                    GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Available: {DeploymentPointsSystem.Instance.CurrentDefensePoints} DP");
                }
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            // Draw socket position
            if (turretPlaced)
            {
                Gizmos.color = Color.gray; // Occupied
            }
            else
            {
                Gizmos.color = Color.green; // Available
            }

            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.cyan;
            if (triggerCollider != null && triggerCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, 2f);
            }
        }
    }
}
