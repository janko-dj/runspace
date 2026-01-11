using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameCore
{
    public class EndScreenController : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "PreGame";
        [SerializeField] private Font uiFont;

        private Canvas canvas;
        private Text headlineText;
        private Text detailsText;
        private Text promptText;
        private bool isVisible;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            CreateUi();
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnEndSuccessEnter -= HandleSuccess;
                RunPhaseController.Instance.OnEndFailEnter -= HandleFail;
            }
        }

        private void Update()
        {
            if (!isVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene(menuSceneName);
            }
        }

        private void HandleSuccess()
        {
            ShowEndScreen(true);
        }

        private void HandleFail()
        {
            ShowEndScreen(false);
        }

        private void ShowEndScreen(bool success)
        {
            CreateUi();
            isVisible = true;
            if (canvas != null)
            {
                canvas.enabled = true;
            }

            MissionConfig mission = MissionSession.Instance != null ? MissionSession.Instance.CurrentMission : null;
            string missionName = mission != null ? mission.displayName : "Mission";
            string headline = success ? $"Mission Complete: {missionName}" : "Mission Failed";

            float timeSurvived = Time.timeSinceLevelLoad;
            int kills = PressureSystem.Instance != null ? PressureSystem.Instance.EnemyKillCount : 0;

            headlineText.text = headline;
            detailsText.text = $"Time Survived: {timeSurvived:F1}s\nEnemies Killed: {kills}";

            if (success && mission != null && mission.questItemReward != null)
            {
                QuestItem reward = mission.questItemReward;
                bool hasItem = PlayerProgress.Instance != null && PlayerProgress.Instance.HasQuestItem(reward.questItemId);
                string status = hasItem ? "OBTAINED" : "MISSING";
                detailsText.text += $"\nQuest Item: {reward.displayName} ({status})";
            }

            promptText.text = "Press Enter or Click to return to menu";
        }

        private void TrySubscribe()
        {
            if (RunPhaseController.Instance == null)
            {
                return;
            }

            RunPhaseController.Instance.OnEndSuccessEnter -= HandleSuccess;
            RunPhaseController.Instance.OnEndFailEnter -= HandleFail;
            RunPhaseController.Instance.OnEndSuccessEnter += HandleSuccess;
            RunPhaseController.Instance.OnEndFailEnter += HandleFail;
        }

        private void CreateUi()
        {
            if (canvas != null)
            {
                canvas.enabled = false;
                return;
            }

            GameObject canvasObject = new GameObject("EndScreenCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.enabled = false;

            Font fontToUse = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            headlineText = CreateText("EndScreenHeadline", canvas.transform, fontToUse, 28, new Vector2(0.5f, 0.7f));
            detailsText = CreateText("EndScreenDetails", canvas.transform, fontToUse, 18, new Vector2(0.5f, 0.55f));
            promptText = CreateText("EndScreenPrompt", canvas.transform, fontToUse, 16, new Vector2(0.5f, 0.4f));
        }

        private Text CreateText(string name, Transform parent, Font font, int fontSize, Vector2 anchor)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800f, 80f);
            rect.anchoredPosition = Vector2.zero;

            return text;
        }
    }
}
