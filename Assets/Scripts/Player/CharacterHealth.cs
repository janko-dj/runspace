using System;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Player health component.
    /// Tracks health and fires events when damage is taken.
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health")]
        [SerializeField] private float maxHealth = 100f;

        [Tooltip("Current health")]
        [SerializeField] private float currentHealth;

        [Tooltip("Invulnerability duration after taking damage (prevents rapid consecutive hits)")]
        [SerializeField] private float invulnerabilityDuration = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool logDamageEvents = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private bool isInvulnerable = false;
        [SerializeField] private float invulnerabilityTimer = 0f;

        // Event fired when this player takes damage
        public event Action<float> OnDamaged; // damage amount

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => (currentHealth / maxHealth) * 100f;
        public bool IsAlive => currentHealth > 0f;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            // Update invulnerability timer
            if (isInvulnerable)
            {
                invulnerabilityTimer -= Time.deltaTime;
                if (invulnerabilityTimer <= 0f)
                {
                    isInvulnerable = false;
                }
            }
        }

        /// <summary>
        /// Apply damage to this player.
        /// </summary>
        public void TakeDamage(float amount)
        {
            // Skip if invulnerable
            if (isInvulnerable)
                return;

            // Skip if already dead
            if (!IsAlive)
                return;

            currentHealth -= amount;
            currentHealth = Mathf.Max(0f, currentHealth);

            if (logDamageEvents)
            {
                Debug.Log($"[CharacterHealth] {gameObject.name} took {amount} damage. Health: {currentHealth:F1}/{maxHealth}");
            }

            // Fire damage event (for repair interruption)
            OnDamaged?.Invoke(amount);

            // Start invulnerability period
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;

            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heal this player.
        /// </summary>
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            if (logDamageEvents)
            {
                Debug.Log($"[CharacterHealth] {gameObject.name} healed {amount}. Health: {currentHealth:F1}/{maxHealth}");
            }
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void Die()
        {
            Debug.Log($"[CharacterHealth] {gameObject.name} died!");

            // TODO: Trigger game over / respawn logic
            // For now, just log
        }

        /// <summary>
        /// Reset health to full (for new run).
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            invulnerabilityTimer = 0f;
        }
    }
}
