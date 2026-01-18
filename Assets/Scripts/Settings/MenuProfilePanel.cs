using UnityEngine;

namespace GameCore
{
    public class MenuProfilePanel : MonoBehaviour
    {
        [SerializeField] private bool showPanel = true;
        [SerializeField] private Rect panelRect = new Rect(20, 20, 360, 280);

        private string nicknameInput;
        private float sensitivity;
        private bool invertYaw;
        private bool invertPitch;
        private bool aimAssist;
        private bool cameraOrbitEnabled;

        private void Start()
        {
            EnsureSettingsService();
            LoadFromService();
        }

        private void EnsureSettingsService()
        {
            if (PlayerSettingsService.Instance != null)
            {
                return;
            }

            GameObject settingsObject = new GameObject("PlayerSettingsService");
            settingsObject.AddComponent<PlayerSettingsService>();
        }

        private void LoadFromService()
        {
            if (PlayerSettingsService.Instance == null)
            {
                return;
            }

            nicknameInput = PlayerSettingsService.Instance.Nickname;
            sensitivity = PlayerSettingsService.Instance.MouseSensitivity;
            invertYaw = PlayerSettingsService.Instance.InvertYaw;
            invertPitch = PlayerSettingsService.Instance.InvertPitch;
            aimAssist = PlayerSettingsService.Instance.AimAssistEnabled;
            cameraOrbitEnabled = PlayerSettingsService.Instance.CameraOrbitEnabled;
        }

        private void OnGUI()
        {
            if (!showPanel)
            {
                return;
            }

            GUI.Box(panelRect, "Profile");

            float y = panelRect.y + 30f;
            float labelWidth = 120f;
            float fieldWidth = panelRect.width - labelWidth - 20f;

            GUI.Label(new Rect(panelRect.x + 10f, y, labelWidth, 20f), "Nickname");
            nicknameInput = GUI.TextField(new Rect(panelRect.x + labelWidth, y, fieldWidth, 20f), nicknameInput);
            y += 30f;

            GUI.Label(new Rect(panelRect.x + 10f, y, labelWidth, 20f), "Sensitivity");
            sensitivity = GUI.HorizontalSlider(new Rect(panelRect.x + labelWidth, y + 5f, fieldWidth, 20f), sensitivity, 0.1f, 5f);
            GUI.Label(new Rect(panelRect.x + labelWidth, y + 20f, fieldWidth, 20f), sensitivity.ToString("F2"));
            y += 40f;

            invertYaw = GUI.Toggle(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 20f), invertYaw, "Invert Yaw");
            y += 22f;

            invertPitch = GUI.Toggle(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 20f), invertPitch, "Invert Pitch (future)");
            y += 22f;

            aimAssist = GUI.Toggle(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 20f), aimAssist, "Aim Assist (off by default)");
            y += 22f;

            cameraOrbitEnabled = GUI.Toggle(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 20f), cameraOrbitEnabled, "Camera Orbit Enabled");
            y += 30f;

            if (GUI.Button(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 24f), "Save Profile"))
            {
                ApplySettings();
            }
            y += 32f;

            GUI.Label(new Rect(panelRect.x + 10f, y, panelRect.width - 20f, 20f), "Keys: WASD Move, Q/E Abilities, Space Dash");
        }

        private void ApplySettings()
        {
            if (PlayerSettingsService.Instance == null)
            {
                return;
            }

            PlayerSettingsService.Instance.SetNickname(nicknameInput);
            PlayerSettingsService.Instance.SetMouseSensitivity(sensitivity);
            PlayerSettingsService.Instance.SetInvertYaw(invertYaw);
            PlayerSettingsService.Instance.SetInvertPitch(invertPitch);
            PlayerSettingsService.Instance.SetAimAssist(aimAssist);
            PlayerSettingsService.Instance.SetCameraOrbitEnabled(cameraOrbitEnabled);
        }
    }
}
