using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Spawns enemies during active phases (Expedition, RunBack).
    /// Spawn rate increases with ThreatLevel from PressureSystem.
    /// </summary>
    public class EncounterSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [Tooltip("Enemy prefab to spawn (must have ChaserEnemy component)")]
        [SerializeField] private GameObject enemyPrefab;

        [Tooltip("Target that spawned enemies will move toward")]
        [SerializeField] private Transform enemyTarget;

        [Tooltip("Radius around this spawner to spawn enemies")]
        [SerializeField] private float spawnRadius = 15f;

        [Tooltip("Base time between spawns (modified by ThreatLevel)")]
        [SerializeField] private float baseSpawnInterval = 2f;

        [Header("RunBack Escalation")]
        [Tooltip("Spawn rate multiplier during RunBack (1.5 = 50% faster spawning)")]
        [SerializeField] private float runBackSpawnRateMultiplier = 1.5f;

        [Tooltip("Spawn radius multiplier during RunBack (0.6 = 40% closer spawns)")]
        [SerializeField] private float runBackSpawnRadiusMultiplier = 0.6f;

        [Tooltip("Enemy move speed multiplier during RunBack (1.2 = 20% faster enemies)")]
        [SerializeField] private float runBackEnemySpeedMultiplier = 1.2f;

        [Header("FinalStand Pressure")]
        [Tooltip("Spawn rate multiplier during FinalStand (0.7 = 30% slower spawning than Expedition)")]
        [SerializeField] private float finalStandSpawnRateMultiplier = 0.7f;

        [Tooltip("Spawn radius multiplier during FinalStand (0.5 = 50% closer to ship)")]
        [SerializeField] private float finalStandSpawnRadiusMultiplier = 0.5f;

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logSpawns = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isSpawning = false;
        [SerializeField] private bool isRunBackMode = false;
        [SerializeField] private bool isFinalStandMode = false;
        [SerializeField] private float currentSpawnInterval;
        [SerializeField] private float spawnTimer;
        [SerializeField] private int totalSpawned = 0;

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
            }

            // Validation
            if (enemyPrefab == null)
            {
                Debug.LogError("[EncounterSpawner] No enemy prefab assigned!", this);
            }

            if (enemyTarget == null)
            {
                Debug.LogWarning("[EncounterSpawner] No enemy target assigned. Enemies won't know where to go!", this);
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromPhaseEvents();
        }

        private void Update()
        {
            if (!isSpawning) return;

            // Calculate current spawn interval based on ThreatLevel
            UpdateSpawnInterval();

            // Spawn timer
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= currentSpawnInterval)
            {
                SpawnEnemy();
                spawnTimer = 0f;
            }
        }

        private void SubscribeToPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged += HandlePhaseChanged;
            RunPhaseController.Instance.OnExpeditionEnter += HandleExpeditionEnter;
            RunPhaseController.Instance.OnRunBackEnter += HandleRunBackEnter;
            RunPhaseController.Instance.OnRunBackExit += HandleRunBackExit;
            RunPhaseController.Instance.OnPrepEnter += HandleInactivePhaseEnter;
            RunPhaseController.Instance.OnFinalStandEnter += HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit += HandleFinalStandExit;
            RunPhaseController.Instance.OnLandingEnter += HandleInactivePhaseEnter;
        }

        private void UnsubscribeFromPhaseEvents()
        {
            if (RunPhaseController.Instance == null) return;

            RunPhaseController.Instance.OnPhaseChanged -= HandlePhaseChanged;
            RunPhaseController.Instance.OnExpeditionEnter -= HandleExpeditionEnter;
            RunPhaseController.Instance.OnRunBackEnter -= HandleRunBackEnter;
            RunPhaseController.Instance.OnRunBackExit -= HandleRunBackExit;
            RunPhaseController.Instance.OnPrepEnter -= HandleInactivePhaseEnter;
            RunPhaseController.Instance.OnFinalStandEnter -= HandleFinalStandEnter;
            RunPhaseController.Instance.OnFinalStandExit -= HandleFinalStandExit;
            RunPhaseController.Instance.OnLandingEnter -= HandleInactivePhaseEnter;
        }

        private void HandlePhaseChanged(RunPhase oldPhase, RunPhase newPhase)
        {
            currentPhase = newPhase;
        }

        private void HandleExpeditionEnter()
        {
            isRunBackMode = false;
            StartSpawning();
            Debug.Log("[EncounterSpawner] Expedition started - Spawning ACTIVE (normal mode)");
        }

        private void HandleRunBackEnter()
        {
            isRunBackMode = true;
            StartSpawning();
            Debug.Log("[EncounterSpawner] RunBack started - Spawning ACTIVE (ESCALATION MODE!)");
        }

        private void HandleRunBackExit()
        {
            isRunBackMode = false;
            Debug.Log("[EncounterSpawner] RunBack exited - Escalation modifiers removed");
        }

        private void HandleFinalStandEnter()
        {
            isRunBackMode = false;
            isFinalStandMode = true;
            StartSpawning();
            Debug.Log("[EncounterSpawner] FinalStand started - Spawning ACTIVE (PRESSURE MODE - slower, closer spawns)");
        }

        private void HandleFinalStandExit()
        {
            isFinalStandMode = false;
            StopSpawning();
            Debug.Log("[EncounterSpawner] FinalStand exited - Spawning STOPPED");
        }

        private void HandleInactivePhaseEnter()
        {
            isRunBackMode = false;
            isFinalStandMode = false;
            StopSpawning();
            Debug.Log("[EncounterSpawner] Inactive phase entered - Spawning STOPPED");
        }

        private void StartSpawning()
        {
            isSpawning = true;
            spawnTimer = 0f;
        }

        private void StopSpawning()
        {
            isSpawning = false;
        }

        private void UpdateSpawnInterval()
        {
            if (PressureSystem.Instance == null)
            {
                currentSpawnInterval = baseSpawnInterval;
                return;
            }

            float threatLevel = PressureSystem.Instance.ThreatLevel;

            // Formula: spawnInterval = baseInterval / (1 + ThreatLevel / 100)
            // Dividing ThreatLevel by 100 to make the scaling more gradual
            // Example: ThreatLevel 0 → interval = base
            //          ThreatLevel 100 → interval = base / 2
            //          ThreatLevel 300 → interval = base / 4
            currentSpawnInterval = baseSpawnInterval / (1f + (threatLevel / 100f));

            // Apply RunBack multiplier (faster spawning during chase mode)
            if (isRunBackMode)
            {
                currentSpawnInterval /= runBackSpawnRateMultiplier;
            }
            // Apply FinalStand multiplier (slower spawning for pressure/defense)
            else if (isFinalStandMode)
            {
                currentSpawnInterval /= finalStandSpawnRateMultiplier;
            }

            // Clamp to prevent extremely fast spawning
            currentSpawnInterval = Mathf.Max(currentSpawnInterval, 0.2f);
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EncounterSpawner] Cannot spawn - no prefab assigned!");
                return;
            }

            // Get random position within spawn radius
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Spawn enemy
            GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            totalSpawned++;

            // Set target if enemy has ChaserEnemy component
            ChaserEnemy enemy = enemyObject.GetComponent<ChaserEnemy>();
            if (enemy != null)
            {
                if (enemyTarget != null)
                {
                    enemy.target = enemyTarget;
                }

                // Apply RunBack speed multiplier (faster enemies during chase)
                if (isRunBackMode)
                {
                    enemy.ApplySpeedMultiplier(runBackEnemySpeedMultiplier);
                }
            }

            if (logSpawns)
            {
                float threatLevel = PressureSystem.Instance != null ? PressureSystem.Instance.ThreatLevel : 0f;
                string modeText = isRunBackMode ? "RUNBACK" : (isFinalStandMode ? "FINALSTAND" : "Normal");
                Debug.Log($"[EncounterSpawner] Spawned enemy #{totalSpawned} ({modeText}) at {spawnPosition} | ThreatLevel: {threatLevel:F1} | Interval: {currentSpawnInterval:F2}s");
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // Calculate effective spawn radius (reduced during RunBack or FinalStand)
            float effectiveRadius = spawnRadius;
            if (isRunBackMode)
            {
                effectiveRadius *= runBackSpawnRadiusMultiplier;
            }
            else if (isFinalStandMode)
            {
                effectiveRadius *= finalStandSpawnRadiusMultiplier;
            }

            // Random point on circle around spawner
            Vector2 randomCircle = Random.insideUnitCircle.normalized * effectiveRadius;
            Vector3 spawnOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);

            return transform.position + spawnOffset;
        }

        /// <summary>
        /// Manually trigger a spawn (for testing).
        /// </summary>
        public void ForceSpawn()
        {
            SpawnEnemy();
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Spawning: {isSpawning} | Interval: {currentSpawnInterval:F2}s | Total: {totalSpawned}";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay) return;

            // Debug overlay
            GUI.Box(new Rect(630, 200, 320, 160), "Enemy Spawner (Debug)");

            int yOffset = 225;
            GUI.Label(new Rect(640, yOffset, 300, 20), $"Phase: {currentPhase}");
            yOffset += 20;
            GUI.Label(new Rect(640, yOffset, 300, 20), $"Spawning Active: {(isSpawning ? "YES" : "NO")}");
            yOffset += 20;

            // Highlight RunBack or FinalStand mode
            if (isRunBackMode)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(640, yOffset, 300, 20), "⚠ RUNBACK ESCALATION MODE!");
                GUI.color = Color.white;
            }
            else if (isFinalStandMode)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(640, yOffset, 300, 20), "⚡ FINALSTAND PRESSURE MODE!");
                GUI.color = Color.white;
            }
            yOffset += 20;

            GUI.Label(new Rect(640, yOffset, 300, 20), $"Spawn Interval: {currentSpawnInterval:F2}s");
            yOffset += 20;

            if (PressureSystem.Instance != null)
            {
                float threatLevel = PressureSystem.Instance.ThreatLevel;
                GUI.Label(new Rect(640, yOffset, 300, 20), $"ThreatLevel: {threatLevel:F1}");
            }
            yOffset += 20;

            GUI.Label(new Rect(640, yOffset, 300, 20), $"Next spawn: {(currentSpawnInterval - spawnTimer):F1}s");
            yOffset += 20;
            GUI.Label(new Rect(640, yOffset, 300, 20), $"Total spawned: {totalSpawned}");
            yOffset += 25;

            // Test button
            if (GUI.Button(new Rect(640, yOffset, 140, 20), "Force Spawn"))
            {
                ForceSpawn();
            }
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            // Calculate effective spawn radius
            float effectiveRadius = spawnRadius;
            if (isRunBackMode)
            {
                effectiveRadius *= runBackSpawnRadiusMultiplier;
            }
            else if (isFinalStandMode)
            {
                effectiveRadius *= finalStandSpawnRadiusMultiplier;
            }

            // Draw spawn radius (red during RunBack, yellow during FinalStand, white otherwise)
            if (isRunBackMode)
                Gizmos.color = Color.red;
            else if (isFinalStandMode)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.white;

            DrawCircle(transform.position, effectiveRadius, 32);

            // Draw normal radius as reference during RunBack or FinalStand
            if (isRunBackMode || isFinalStandMode)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.3f); // Faded white
                DrawCircle(transform.position, spawnRadius, 32);
            }

            // Draw line to target
            if (enemyTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, enemyTarget.position);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0,
                    Mathf.Sin(angle) * radius
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
