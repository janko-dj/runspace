using UnityEngine;

namespace GameCore
{
    public class MissionSession : MonoBehaviour
    {
        public enum MissionState
        {
            NotStarted,
            InProgress,
            Success,
            Fail
        }

        public static MissionSession Instance { get; private set; }

        public MissionConfig CurrentMission { get; private set; }
        public MissionState State { get; private set; } = MissionState.NotStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartMission(MissionConfig config)
        {
            CurrentMission = config;
            State = MissionState.InProgress;
        }

        public void EndMissionSuccess()
        {
            State = MissionState.Success;
        }

        public void EndMissionFail()
        {
            State = MissionState.Fail;
        }

        public void ClearSession()
        {
            CurrentMission = null;
            State = MissionState.NotStarted;
        }
    }
}
