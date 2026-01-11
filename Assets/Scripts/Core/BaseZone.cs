using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Trigger zone that detects when players enter/exit the ship area.
    /// Used during RunBack phase to track when all players have returned.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BaseZone : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Tag used to identify player objects (default: Player)")]
        [SerializeField] private string playerTag = "Player";

        [Tooltip("Expected number of players (auto-detected at Start if 0)")]
        [SerializeField] private int totalPlayerCount = 0;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logPlayerEnterExit = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private int playersInsideCount = 0;

        private HashSet<GameObject> playersInside = new HashSet<GameObject>();
        private Collider triggerCollider;

        // Public accessors
        public int PlayersInsideCount => playersInsideCount;
        public int TotalPlayerCount => totalPlayerCount;
        public bool AreAllPlayersInside => playersInsideCount >= totalPlayerCount && totalPlayerCount > 0;

        private void Awake()
        {
            // Ensure we have a trigger collider
            triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null)
            {
                Debug.LogError("[BaseZone] No Collider component found! Adding BoxCollider.", this);
                triggerCollider = gameObject.AddComponent<BoxCollider>();
            }

            triggerCollider.isTrigger = true;
        }

        private void Start()
        {
            // Auto-detect player count if not set
            if (totalPlayerCount <= 0)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
                totalPlayerCount = players.Length;
                Debug.Log($"[BaseZone] Auto-detected {totalPlayerCount} players with tag '{playerTag}'");
            }

            if (totalPlayerCount <= 0)
            {
                Debug.LogWarning("[BaseZone] No players detected! Make sure player objects have the correct tag.", this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the entering object is a player
            if (!other.CompareTag(playerTag))
                return;

            GameObject player = other.gameObject;

            // Add to set (prevents duplicates if player has multiple colliders)
            if (playersInside.Add(player))
            {
                playersInsideCount = playersInside.Count;

                if (logPlayerEnterExit)
                {
                    Debug.Log($"[BaseZone] {player.name} entered ship zone. Players inside: {playersInsideCount}/{totalPlayerCount}");
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Check if the exiting object is a player
            if (!other.CompareTag(playerTag))
                return;

            GameObject player = other.gameObject;

            // Remove from set
            if (playersInside.Remove(player))
            {
                playersInsideCount = playersInside.Count;

                if (logPlayerEnterExit)
                {
                    Debug.Log($"[BaseZone] {player.name} left ship zone. Players inside: {playersInsideCount}/{totalPlayerCount}");
                }
            }
        }

        /// <summary>
        /// Clear all tracked players (useful for phase resets).
        /// </summary>
        public void ClearPlayers()
        {
            playersInside.Clear();
            playersInsideCount = 0;
            Debug.Log("[BaseZone] Cleared all tracked players");
        }

        /// <summary>
        /// Get list of player names currently inside.
        /// </summary>
        public string GetPlayersInsideDebugInfo()
        {
            if (playersInside.Count == 0)
                return "No players inside";

            List<string> names = new List<string>();
            foreach (GameObject player in playersInside)
            {
                names.Add(player.name);
            }
            return string.Join(", ", names);
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Debug overlay
            int boxWidth = 320;
            int boxHeight = 100;
            int xPos = Screen.width - boxWidth - 10;
            int yPos = 10;

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Ship Zone (Debug)");

            int yOffset = yPos + 25;
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"Players Inside: {playersInsideCount}/{totalPlayerCount}");
            yOffset += 20;

            // Highlight when all players are inside
            if (AreAllPlayersInside)
            {
                GUI.color = Color.green;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "✓ ALL PLAYERS IN ZONE!");
                GUI.color = Color.white;
            }
            else
            {
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Waiting for all players...");
            }
            yOffset += 20;

            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), GetPlayersInsideDebugInfo());
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            // Draw trigger zone bounds
            if (triggerCollider == null)
                triggerCollider = GetComponent<Collider>();

            if (triggerCollider != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green
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
    }
}
