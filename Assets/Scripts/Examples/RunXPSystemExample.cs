using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Example script showing XP system integration and testing utilities.
    /// Provides keyboard shortcuts to test XP and leveling.
    /// </summary>
    public class RunXPSystemExample : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private int testXPAmount = 5;

        private void Update()
        {
            if (RunXPSystem.Instance == null) return;

            // Keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.X))
            {
                TestAddXP();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                TestForceLevel();
            }
        }

        /// <summary>
        /// Add a small amount of XP for testing (Press X).
        /// </summary>
        private void TestAddXP()
        {
            RunXPSystem.Instance.AddXP(testXPAmount);
            Debug.Log($"[RunXPSystemExample] Added {testXPAmount} XP (Press X)");
        }

        /// <summary>
        /// Force a level-up by adding enough XP (Press C).
        /// </summary>
        private void TestForceLevel()
        {
            if (RunXPSystem.Instance.IsMaxLevel())
            {
                Debug.Log("[RunXPSystemExample] Already at max level!");
                return;
            }

            int needed = RunXPSystem.Instance.GetXPForNextLevel() - RunXPSystem.Instance.CurrentXP;
            RunXPSystem.Instance.AddXP(needed);
            Debug.Log($"[RunXPSystemExample] Force level-up! Added {needed} XP (Press C)");
        }

        private void OnGUI()
        {
            if (RunXPSystem.Instance == null) return;

            // Show test controls
            GUI.Box(new Rect(320, 640, 300, 80), "XP System Testing");

            int yOffset = 665;
            GUI.Label(new Rect(330, yOffset, 280, 20), "X: Add XP");
            yOffset += 20;
            GUI.Label(new Rect(330, yOffset, 280, 20), "C: Force Level-Up");
            yOffset += 20;

            string status = RunXPSystem.Instance.IsMaxLevel() ? "MAX LEVEL" : $"{RunXPSystem.Instance.CurrentXP}/{RunXPSystem.Instance.GetXPForNextLevel()} XP";
            GUI.Label(new Rect(330, yOffset, 280, 20), $"Status: {status}");
        }
    }
}
