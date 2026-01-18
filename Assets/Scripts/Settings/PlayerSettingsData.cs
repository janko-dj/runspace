using UnityEngine;

namespace GameCore
{
    [CreateAssetMenu(menuName = "Game/Player Settings", fileName = "PlayerSettingsData")]
    public class PlayerSettingsData : ScriptableObject
    {
        public string defaultNickname = "Pilot";
        public float defaultMouseSensitivity = 1f;
        public bool defaultInvertYaw = false;
        public bool defaultInvertPitch = false;
        public bool defaultAimAssist = false;
        public bool defaultCameraOrbitEnabled = true;
    }
}
