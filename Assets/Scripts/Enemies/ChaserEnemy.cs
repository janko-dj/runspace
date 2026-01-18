using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Basic enemy that moves toward a target.
    /// Represents the "Swarmer" archetype from design spec:
    /// - Low HP, fast, group pressure
    /// - Forces movement, punishes standing still
    /// </summary>
    public class ChaserEnemy : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [Tooltip("Maximum health of this enemy")]
        [SerializeField] private float maxHealth = 20f;

        [Tooltip("Movement speed in units per second")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Target")]
        [Tooltip("Transform to move toward (assign in Inspector)")]
        public Transform target;

        [Header("Combat")]
        [Tooltip("Damage dealt to player on contact")]
        [SerializeField] private float contactDamage = 10f;

        [Tooltip("Cooldown between damage ticks (prevents rapid hits)")]
        [SerializeField] private float damageCooldown = 1f;

        [Tooltip("Player tag for collision detection")]
        [SerializeField] private string playerTag = "Player";

        private float currentHealth;
        private float lastDamageTime = -999f;

        private void Start()
        {
            currentHealth = maxHealth;

            // Validation
            if (target == null)
            {
                Debug.LogWarning($"[ChaserEnemy] {gameObject.name} has no target assigned!", this);
            }
        }

        private void Update()
        {
            if (target == null) return;

            // Simple movement toward target
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 newPosition = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            transform.position = newPosition;

            // Optional: Rotate to face target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Apply a move speed multiplier (called on spawn for RunBack escalation).
        /// </summary>
        public void ApplySpeedMultiplier(float multiplier)
        {
            moveSpeed *= multiplier;
        }

        /// <summary>
        /// Apply damage to this enemy.
        /// </summary>
        public void TakeDamage(float amount)
        {
            currentHealth -= amount;

            Debug.Log($"[ChaserEnemy] {gameObject.name} took {amount} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Handle enemy death.
        /// </summary>
        public void Die()
        {
            Debug.Log($"[ChaserEnemy] {gameObject.name} died!");

            // Register kill with PressureSystem
            if (PressureSystem.Instance != null)
            {
                PressureSystem.Instance.RegisterEnemyKill();
            }

            // Grant XP to player
            if (RunXPSystem.Instance != null)
            {
                RunXPSystem.Instance.AddXP(5); // 5 XP per swarmer kill
            }

            // Destroy this enemy
            Destroy(gameObject);
        }

        /// <summary>
        /// Handle collision with player to deal damage.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
                return;

            // Check cooldown
            if (Time.time - lastDamageTime < damageCooldown)
                return;

            // Deal damage to player
            CharacterHealth playerHealth = other.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                lastDamageTime = Time.time;

                Debug.Log($"[ChaserEnemy] {gameObject.name} dealt {contactDamage} damage to {other.gameObject.name}");
            }
        }

        // Debug visualization
        private void OnDrawGizmos()
        {
            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }
    }
}
