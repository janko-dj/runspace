using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Helper script for testing enemy behavior in the editor.
    /// Provides keyboard shortcuts to damage/kill enemies.
    /// </summary>
    public class EnemyTestHelper : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Damage amount when pressing test key")]
        [SerializeField] private float testDamageAmount = 10f;

        [Header("Info")]
        [SerializeField] private int currentEnemyCount = 0;

        private void Update()
        {
            // Update enemy count
            currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            // Keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.D))
            {
                DamageRandomEnemy();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                KillRandomEnemy();
            }
            else if (Input.GetKeyDown(KeyCode.G))
            {
                KillAllEnemies();
            }
        }

        /// <summary>
        /// Damage a random enemy for testing.
        /// </summary>
        private void DamageRandomEnemy()
        {
            ChaserEnemy[] enemies = Object.FindObjectsByType<ChaserEnemy>(FindObjectsSortMode.None);

            if (enemies.Length == 0)
            {
                Debug.Log("[EnemyTestHelper] No enemies to damage!");
                return;
            }

            ChaserEnemy randomEnemy = enemies[Random.Range(0, enemies.Length)];
            randomEnemy.TakeDamage(testDamageAmount);

            Debug.Log($"[EnemyTestHelper] Damaged {randomEnemy.gameObject.name} for {testDamageAmount} (Press D)");
        }

        /// <summary>
        /// Instantly kill a random enemy.
        /// </summary>
        private void KillRandomEnemy()
        {
            ChaserEnemy[] enemies = Object.FindObjectsByType<ChaserEnemy>(FindObjectsSortMode.None);

            if (enemies.Length == 0)
            {
                Debug.Log("[EnemyTestHelper] No enemies to kill!");
                return;
            }

            ChaserEnemy randomEnemy = enemies[Random.Range(0, enemies.Length)];
            randomEnemy.Die();

            Debug.Log($"[EnemyTestHelper] Killed {randomEnemy.gameObject.name} (Press F)");
        }

        /// <summary>
        /// Kill all enemies in the scene.
        /// </summary>
        private void KillAllEnemies()
        {
            ChaserEnemy[] enemies = Object.FindObjectsByType<ChaserEnemy>(FindObjectsSortMode.None);

            if (enemies.Length == 0)
            {
                Debug.Log("[EnemyTestHelper] No enemies to kill!");
                return;
            }

            foreach (ChaserEnemy enemy in enemies)
            {
                enemy.Die();
            }

            Debug.Log($"[EnemyTestHelper] Killed all {enemies.Length} enemies (Press G)");
        }

        private void OnGUI()
        {
            // Simple debug info
            GUI.Box(new Rect(630, 350, 320, 100), "Enemy Test Helper");

            int yOffset = 375;
            GUI.Label(new Rect(640, yOffset, 300, 20), $"Active Enemies: {currentEnemyCount}");
            yOffset += 20;
            GUI.Label(new Rect(640, yOffset, 300, 20), "D: Damage Random Enemy");
            yOffset += 20;
            GUI.Label(new Rect(640, yOffset, 300, 20), "F: Kill Random Enemy");
            yOffset += 20;
            GUI.Label(new Rect(640, yOffset, 300, 20), "G: Kill All Enemies");
        }
    }
}
