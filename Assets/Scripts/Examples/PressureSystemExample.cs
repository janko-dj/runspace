using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Example script showing how other systems can interact with PressureSystem.
    /// This demonstrates how enemy spawners, loot systems, etc. would use the threat values.
    /// </summary>
    public class PressureSystemExample : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private bool logThreatUpdates = true;
        [SerializeField] private float checkInterval = 2f;

        private float checkTimer = 0f;

        private void Update()
        {
            if (PressureSystem.Instance == null) return;

            // Example: Check threat level periodically and react
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                CheckThreatLevels();
            }

            // Example: Simulate enemy kills and salvage pickups with keyboard
            if (Input.GetKeyDown(KeyCode.K))
            {
                SimulateEnemyKill();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                SimulateSalvagePickup();
            }
        }

        private void CheckThreatLevels()
        {
            if (!logThreatUpdates) return;

            float currentThreat = PressureSystem.Instance.ThreatLevel;
            float currentDebt = PressureSystem.Instance.ThreatDebt;
            string category = PressureSystem.Instance.GetThreatLevelCategory();

            Debug.Log($"[ThreatExample] Current state: {category} | Threat: {currentThreat:F1} | Debt: {currentDebt:F1}");

            // Example: How a spawner might react to threat levels
            if (currentThreat > 200f)
            {
                Debug.Log("[ThreatExample] High threat! Spawner would increase spawn rate here.");
            }
        }

        private void SimulateEnemyKill()
        {
            // In real implementation, this would be called when an enemy dies
            PressureSystem.Instance.RegisterEnemyKill();
            Debug.Log("[ThreatExample] Simulated enemy kill (Press K)");
        }

        private void SimulateSalvagePickup()
        {
            // In real implementation, this would be called when player picks up salvage
            PressureSystem.Instance.RegisterSalvagePickup();
            Debug.Log("[ThreatExample] Simulated salvage pickup (Press L)");
        }

        // Example: How a spawner would calculate spawn intensity based on ThreatLevel
        public int CalculateSpawnIntensity()
        {
            if (PressureSystem.Instance == null) return 1;

            float threat = PressureSystem.Instance.ThreatLevel;

            // Simple example: more threat = more enemies
            if (threat < 50f) return 1;
            if (threat < 100f) return 2;
            if (threat < 200f) return 3;
            return 5;
        }

        // Example: How Final Stand would use ThreatDebt to scale waves
        public float CalculateFinalStandWaveStrength()
        {
            if (PressureSystem.Instance == null) return 1f;

            float debt = PressureSystem.Instance.ThreatDebt;

            // Example formula: base strength + debt multiplier
            float baseStrength = 1f;
            float debtMultiplier = 0.05f; // 5% increase per debt point

            return baseStrength + (debt * debtMultiplier);
        }

        private void OnGUI()
        {
            if (PressureSystem.Instance == null) return;

            // Additional debug info showing how systems would use threat values
            GUI.Box(new Rect(340, 200, 280, 100), "Threat Usage Example");
            GUI.Label(new Rect(350, 225, 260, 20), $"Spawn Intensity: {CalculateSpawnIntensity()}x");
            GUI.Label(new Rect(350, 245, 260, 20), $"Wave Strength: {CalculateFinalStandWaveStrength():F2}x");
            GUI.Label(new Rect(350, 270, 260, 20), "Press K: Enemy Kill");
            GUI.Label(new Rect(350, 285, 260, 20), "Press L: Salvage Pickup");
        }
    }
}
