using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Simple auto-attack weapon system.
    /// Automatically attacks the nearest enemy within range at fixed intervals.
    /// Vampire Survivors style - no aiming required.
    /// </summary>
    public class AutoAttackWeapon : MonoBehaviour
    {
        [Header("Attack Settings")]
        [Tooltip("Range within which enemies can be attacked")]
        [SerializeField] private float attackRange = 3f;

        [Tooltip("Time between attacks in seconds")]
        [SerializeField] private float attackInterval = 0.5f;

        [Tooltip("Damage dealt per attack")]
        [SerializeField] private float attackDamage = 10f;

        [Header("Per-Player Modifiers")]
        [Tooltip("Additive damage modifier from upgrades")]
        [SerializeField] private float damageModifier = 0f;

        [Tooltip("Additive range modifier from upgrades")]
        [SerializeField] private float rangeModifier = 0f;

        [Tooltip("Multiplicative attack speed modifier from upgrades (1.0 = normal, 1.2 = 20% faster)")]
        [SerializeField] private float attackSpeedModifier = 1f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool logAttacks = false;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private int enemiesInRange = 0;
        [SerializeField] private float timeSinceLastAttack = 0f;

        private ChaserEnemy currentTarget;

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            // Attack at fixed intervals (modified by per-player attack speed modifier)
            float effectiveInterval = attackInterval / attackSpeedModifier;
            if (timeSinceLastAttack >= effectiveInterval)
            {
                TryAttack();
                timeSinceLastAttack = 0f;
            }
        }

        private void TryAttack()
        {
            // Find nearest enemy within range
            ChaserEnemy nearestEnemy = FindNearestEnemy();

            if (nearestEnemy != null)
            {
                Attack(nearestEnemy);
            }
        }

        private ChaserEnemy FindNearestEnemy()
        {
            ChaserEnemy[] allEnemies = Object.FindObjectsByType<ChaserEnemy>(FindObjectsSortMode.None);
            ChaserEnemy nearest = null;
            float nearestDistance = float.MaxValue;

            enemiesInRange = 0;

            // Calculate effective range with per-player modifier
            float effectiveRange = attackRange + rangeModifier;

            foreach (ChaserEnemy enemy in allEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(transform.position, enemy.transform.position);

                // Count enemies in range
                if (distance <= effectiveRange)
                {
                    enemiesInRange++;

                    // Track nearest
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = enemy;
                    }
                }
            }

            currentTarget = nearest;
            return nearest;
        }

        private void Attack(ChaserEnemy enemy)
        {
            if (enemy == null) return;

            // Calculate effective damage with per-player modifier
            float effectiveDamage = attackDamage + damageModifier;

            // Deal damage directly (no projectile)
            enemy.TakeDamage(effectiveDamage);

            if (logAttacks)
            {
                Debug.Log($"[AutoAttackWeapon] Attacked {enemy.gameObject.name} for {effectiveDamage} damage");
            }

            // Visual feedback could go here (particle effects, sound, etc.)
            // For now, just draw a debug line
            Debug.DrawLine(transform.position, enemy.transform.position, Color.yellow, 0.1f);
        }

        /// <summary>
        /// Get the current attack range.
        /// </summary>
        public float GetAttackRange()
        {
            return attackRange;
        }

        /// <summary>
        /// Get the current attack range including modifiers.
        /// </summary>
        public float GetEffectiveAttackRange()
        {
            return attackRange + rangeModifier;
        }

        /// <summary>
        /// Get the current attack damage.
        /// </summary>
        public float GetAttackDamage()
        {
            return attackDamage;
        }

        /// <summary>
        /// Get the current attack damage including modifiers.
        /// </summary>
        public float GetEffectiveAttackDamage()
        {
            return attackDamage + damageModifier;
        }

        /// <summary>
        /// Get the current attack interval.
        /// </summary>
        public float GetAttackInterval()
        {
            return attackInterval;
        }

        /// <summary>
        /// Modify attack range (for upgrades, buffs, etc.)
        /// </summary>
        public void SetAttackRange(float newRange)
        {
            attackRange = Mathf.Max(0, newRange);
        }

        /// <summary>
        /// Modify attack damage (for upgrades, buffs, etc.)
        /// </summary>
        public void SetAttackDamage(float newDamage)
        {
            attackDamage = Mathf.Max(0, newDamage);
        }

        /// <summary>
        /// Modify attack interval (for upgrades, buffs, etc.)
        /// </summary>
        public void SetAttackInterval(float newInterval)
        {
            attackInterval = Mathf.Max(0.1f, newInterval);
        }

        /// <summary>
        /// Add to damage modifier (for upgrades).
        /// </summary>
        public void AddDamageModifier(float amount)
        {
            damageModifier += amount;
        }

        /// <summary>
        /// Add to range modifier (for upgrades).
        /// </summary>
        public void AddRangeModifier(float amount)
        {
            rangeModifier += amount;
        }

        /// <summary>
        /// Multiply attack speed modifier (for upgrades).
        /// </summary>
        public void MultiplyAttackSpeed(float multiplier)
        {
            attackSpeedModifier *= multiplier;
        }

        /// <summary>
        /// Reset all modifiers (for new run).
        /// </summary>
        public void ResetModifiers()
        {
            damageModifier = 0f;
            rangeModifier = 0f;
            attackSpeedModifier = 1f;
        }

        // Gizmo for attack range visualization
        private void OnDrawGizmos()
        {
            // Draw attack range circle (with per-player modifier applied)
            float effectiveRange = attackRange + rangeModifier;
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
            DrawWireCircle(transform.position, effectiveRange, 32);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw more detailed gizmos when selected (with per-player modifier applied)
            float effectiveRange = attackRange + rangeModifier;
            Gizmos.color = Color.yellow;
            DrawWireCircle(transform.position, effectiveRange, 64);

            // Draw line to current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }

        private void DrawWireCircle(Vector3 center, float radius, int segments)
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

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUI.Box(new Rect(10, 640, 300, 100), "Player Weapon");
            GUI.Label(new Rect(20, 665, 280, 20), $"Attack Range: {attackRange:F1}");
            GUI.Label(new Rect(20, 685, 280, 20), $"Attack Damage: {attackDamage:F1}");
            GUI.Label(new Rect(20, 705, 280, 20), $"Enemies in Range: {enemiesInRange}");
            GUI.Label(new Rect(20, 725, 280, 20), $"Next Attack: {(attackInterval - timeSinceLastAttack):F2}s");
        }
    }
}
