using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Basic auto-targeting turret deployable.
    /// Finds nearest enemy, shoots at fixed interval, deals damage.
    /// Can be destroyed by enemies.
    /// </summary>
    public class DefenseEmplacement : MonoBehaviour
    {
        [Header("Combat Stats")]
        [Tooltip("Maximum health of this turret")]
        [SerializeField] private float maxHealth = 50f;

        [Tooltip("Damage dealt per shot")]
        [SerializeField] private float damage = 15f;

        [Tooltip("Detection range for finding enemies")]
        [SerializeField] private float detectionRange = 10f;

        [Tooltip("Time between shots in seconds")]
        [SerializeField] private float fireRate = 1f;

        [Header("Targeting")]
        [Tooltip("Layer mask for enemy detection")]
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Tag for enemy detection (fallback if layer not set)")]
        [SerializeField] private string enemyTag = "Enemy";

        [Header("Debug Settings")]
        [SerializeField] private bool showDebugOverlay = true;
        [SerializeField] private bool logCombatEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private float currentHealth;
        [SerializeField] private Transform currentTarget;
        [SerializeField] private float fireTimer = 0f;

        private bool isDestroyed = false;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f && !isDestroyed;

        private void Start()
        {
            currentHealth = maxHealth;

            if (logCombatEvents)
            {
                Debug.Log($"[DefenseEmplacement] {gameObject.name} deployed at {transform.position}");
            }
        }

        private void Update()
        {
            if (isDestroyed)
                return;

            // Find nearest enemy
            currentTarget = FindNearestEnemy();

            // If we have a target in range, shoot
            if (currentTarget != null)
            {
                // Rotate toward target
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                direction.y = 0f; // Keep rotation on horizontal plane
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }

                // Update fire timer
                fireTimer += Time.deltaTime;

                if (fireTimer >= fireRate)
                {
                    Shoot(currentTarget);
                    fireTimer = 0f;
                }
            }
            else
            {
                // Reset fire timer when no target
                fireTimer = 0f;
            }
        }

        /// <summary>
        /// Find the nearest enemy within detection range.
        /// </summary>
        private Transform FindNearestEnemy()
        {
            // Use OverlapSphere to find all enemies in range
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange);

            Transform nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider enemyCollider in enemies)
            {
                // Check if this is an enemy (by tag)
                if (!enemyCollider.CompareTag(enemyTag))
                    continue;

                // Check distance
                float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemyCollider.transform;
                }
            }

            return nearestEnemy;
        }

        /// <summary>
        /// Shoot at target (instant hit, no projectile).
        /// </summary>
        private void Shoot(Transform target)
        {
            if (target == null)
                return;

            // Try to damage the enemy
            ChaserEnemy enemy = target.GetComponent<ChaserEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (logCombatEvents)
                {
                    Debug.Log($"[DefenseEmplacement] {gameObject.name} shot {target.name} for {damage} damage");
                }
            }
        }

        /// <summary>
        /// Take damage from enemies.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (isDestroyed)
                return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(0f, currentHealth);

            if (logCombatEvents)
            {
                Debug.Log($"[DefenseEmplacement] {gameObject.name} took {amount} damage. Health: {currentHealth:F1}/{maxHealth}");
            }

            if (currentHealth <= 0f)
            {
                DestroyTurret();
            }
        }

        /// <summary>
        /// Destroy this turret.
        /// </summary>
        private void DestroyTurret()
        {
            if (isDestroyed)
                return;

            isDestroyed = true;

            if (logCombatEvents)
            {
                Debug.Log($"[DefenseEmplacement] {gameObject.name} destroyed!");
            }

            // Destroy the GameObject
            Destroy(gameObject);
        }

        /// <summary>
        /// Get debug info string.
        /// </summary>
        public string GetDebugInfo()
        {
            if (currentTarget != null)
                return $"HP: {currentHealth:F0}/{maxHealth} | Targeting: {currentTarget.name}";
            else
                return $"HP: {currentHealth:F0}/{maxHealth} | Idle";
        }

        private void OnGUI()
        {
            if (!showDebugOverlay || isDestroyed)
                return;

            // Calculate screen position above the turret
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);

            if (screenPos.z <= 0)
                return; // Behind camera

            int boxWidth = 250;
            int boxHeight = 60;
            int xPos = (int)screenPos.x - boxWidth / 2;
            int yPos = Screen.height - (int)screenPos.y - boxHeight;

            // Background box
            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Turret");

            int yOffset = yPos + 25;

            // Health bar
            float healthPercent = currentHealth / maxHealth;
            GUI.color = healthPercent > 0.5f ? Color.green : (healthPercent > 0.25f ? Color.yellow : Color.red);
            GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"HP: {currentHealth:F0}/{maxHealth} ({healthPercent * 100:F0}%)");
            GUI.color = Color.white;
            yOffset += 20;

            // Target info
            if (currentTarget != null)
            {
                GUI.color = Color.yellow;
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), $"⚔ Targeting: {currentTarget.name}");
                GUI.color = Color.white;
            }
            else
            {
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Idle (no targets)");
            }
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw line to current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.position);
            }
        }
    }
}
