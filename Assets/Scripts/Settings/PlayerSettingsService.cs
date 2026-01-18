using UnityEngine;

namespace GameCore
{
    public class PlayerSettingsService : MonoBehaviour
    {
        public static PlayerSettingsService Instance { get; private set; }

        [SerializeField] private PlayerSettingsData defaults;

        private const string NicknameKey = "player.nickname";
        private const string SensitivityKey = "player.sensitivity";
        private const string InvertYawKey = "player.invertYaw";
        private const string InvertPitchKey = "player.invertPitch";
        private const string AimAssistKey = "player.aimAssist";
        private const string OrbitEnabledKey = "player.orbitEnabled";

        public string Nickname { get; private set; }
        public float MouseSensitivity { get; private set; }
        public bool InvertYaw { get; private set; }
        public bool InvertPitch { get; private set; }
        public bool AimAssistEnabled { get; private set; }
        public bool CameraOrbitEnabled { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            string defaultName = defaults != null ? defaults.defaultNickname : "Pilot";
            float defaultSensitivity = defaults != null ? defaults.defaultMouseSensitivity : 1f;
            bool defaultInvertYaw = defaults != null && defaults.defaultInvertYaw;
            bool defaultInvertPitch = defaults != null && defaults.defaultInvertPitch;
            bool defaultAimAssist = defaults != null && defaults.defaultAimAssist;
            bool defaultOrbit = defaults == null || defaults.defaultCameraOrbitEnabled;

            Nickname = PlayerPrefs.GetString(NicknameKey, defaultName);
            MouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, defaultSensitivity);
            InvertYaw = PlayerPrefs.GetInt(InvertYawKey, defaultInvertYaw ? 1 : 0) == 1;
            InvertPitch = PlayerPrefs.GetInt(InvertPitchKey, defaultInvertPitch ? 1 : 0) == 1;
            AimAssistEnabled = PlayerPrefs.GetInt(AimAssistKey, defaultAimAssist ? 1 : 0) == 1;
            CameraOrbitEnabled = PlayerPrefs.GetInt(OrbitEnabledKey, defaultOrbit ? 1 : 0) == 1;
        }

        public void Save()
        {
            PlayerPrefs.SetString(NicknameKey, Nickname);
            PlayerPrefs.SetFloat(SensitivityKey, MouseSensitivity);
            PlayerPrefs.SetInt(InvertYawKey, InvertYaw ? 1 : 0);
            PlayerPrefs.SetInt(InvertPitchKey, InvertPitch ? 1 : 0);
            PlayerPrefs.SetInt(AimAssistKey, AimAssistEnabled ? 1 : 0);
            PlayerPrefs.SetInt(OrbitEnabledKey, CameraOrbitEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetNickname(string nickname)
        {
            Nickname = string.IsNullOrWhiteSpace(nickname) ? "Pilot" : nickname.Trim();
            Save();
        }

        public void SetMouseSensitivity(float value)
        {
            MouseSensitivity = Mathf.Clamp(value, 0.1f, 5f);
            Save();
        }

        public void SetInvertYaw(bool value)
        {
            InvertYaw = value;
            Save();
        }

        public void SetInvertPitch(bool value)
        {
            InvertPitch = value;
            Save();
        }

        public void SetAimAssist(bool value)
        {
            AimAssistEnabled = value;
            Save();
        }

        public void SetCameraOrbitEnabled(bool value)
        {
            CameraOrbitEnabled = value;
            Save();
        }
    }
}
