using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCore
{
    public class MissionSelectionMenu : MonoBehaviour
    {
        [Header("Mission 1 (Hardcoded)")]
        [SerializeField] private MissionConfig mission1;
        [Header("Mission 2 (Hardcoded)")]
        [SerializeField] private MissionConfig mission2;

        private bool isLoading = false;

        private void Start()
        {
            if (Object.FindFirstObjectByType<RunPhaseController>() != null)
            {
                Destroy(gameObject);
                return;
            }

            if (MissionSession.Instance != null)
            {
                MissionSession.Instance.ClearSession();
            }

            EnsurePlayerProgress();
            EnsureMenuCamera();

            if (mission1 == null)
            {
                Debug.LogError("[MissionSelectionMenu] Mission 1 not assigned.");
            }

            LogMissionLockState(mission1);
            LogMissionLockState(mission2);
        }

        private void Update()
        {
            if (mission1 == null || isLoading)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (IsMissionUnlocked(mission1))
                {
                    SelectMission(mission1);
                }
            }
        }

        private void SelectMission(MissionConfig mission)
        {
            if (MissionSession.Instance == null)
            {
                Debug.LogError("[MissionSelectionMenu] MissionSession not found.");
                return;
            }

            if (!IsMissionUnlocked(mission))
            {
                Debug.Log($"[MissionSelectionMenu] Mission locked: {mission.displayName}");
                return;
            }

            MissionSession.Instance.StartMission(mission);
            isLoading = true;

            if (string.IsNullOrWhiteSpace(mission.sceneName))
            {
                Debug.LogError("[MissionSelectionMenu] Mission scene name is empty.");
                isLoading = false;
                return;
            }

            Debug.Log($"[MissionSelectionMenu] Selected mission: {mission.displayName} ({mission.missionId})");
            StartCoroutine(LoadSelectedMission(mission.sceneName));
        }

        private System.Collections.IEnumerator LoadSelectedMission(string sceneName)
        {
            yield return null;
            SceneManager.LoadScene(sceneName);
        }

        private void EnsureMenuCamera()
        {
            Camera existingCamera = Object.FindFirstObjectByType<Camera>();
            if (existingCamera != null)
            {
                return;
            }

            GameObject cameraObject = new GameObject("MenuCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 5f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(15f, 0f, 0f);
        }

        private void OnGUI()
        {
            int boxWidth = 420;
            int boxHeight = 170;
            int xPos = Screen.width / 2 - boxWidth / 2;
            int yPos = Screen.height / 2 - boxHeight / 2;

            GUI.Box(new Rect(xPos, yPos, boxWidth, boxHeight), "Mission Select");

            int yOffset = yPos + 30;
            DrawMissionRow(mission1, xPos + 10, yOffset, boxWidth - 20);
            yOffset += 35;

            DrawMissionRow(mission2, xPos + 10, yOffset, boxWidth - 20);
            yOffset += 35;

            if (isLoading)
            {
                GUI.Label(new Rect(xPos + 10, yOffset, boxWidth - 20, 20), "Loading mission...");
            }
        }

        private void DrawMissionRow(MissionConfig mission, int x, int y, int width)
        {
            if (mission == null)
            {
                GUI.Label(new Rect(x, y, width, 20), "Mission (missing)");
                return;
            }

            bool unlocked = IsMissionUnlocked(mission);
            string label = unlocked ? mission.displayName : $"{mission.displayName} - LOCKED";

            GUI.enabled = unlocked && !isLoading;
            if (GUI.Button(new Rect(x, y, width, 24), label))
            {
                SelectMission(mission);
            }
            GUI.enabled = true;
        }

        private bool IsMissionUnlocked(MissionConfig mission)
        {
            if (mission == null || mission.requiredQuestItem == null)
            {
                return true;
            }

            if (PlayerProgress.Instance == null)
            {
                return false;
            }

            return PlayerProgress.Instance.HasQuestItem(mission.requiredQuestItem.questItemId);
        }

        private void LogMissionLockState(MissionConfig mission)
        {
            if (mission == null)
            {
                return;
            }

            bool unlocked = IsMissionUnlocked(mission);
            string state = unlocked ? "unlocked" : "locked";
            Debug.Log($"[MissionSelectionMenu] Mission {state}: {mission.displayName}");
        }

        private void EnsurePlayerProgress()
        {
            if (PlayerProgress.Instance != null)
            {
                return;
            }

            GameObject progressObject = new GameObject("PlayerProgress");
            progressObject.AddComponent<PlayerProgress>();
        }
    }
}
