using UnityEngine;
namespace GameCore
{
    public class MissionBootstrapper : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "PreGame";

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void Start()
        {
            ApplyMissionConfig();
            ResetMissionSystems();
            ConfigureMissionFlow();
            ApplyMissionLayout();

            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.TransitionToPhase(RunPhase.Landing);
            }

            TrySubscribe();
        }

        private void OnDisable()
        {
            if (RunPhaseController.Instance != null)
            {
                RunPhaseController.Instance.OnEndSuccessEnter -= HandleMissionSuccess;
                RunPhaseController.Instance.OnEndFailEnter -= HandleMissionFail;
            }
        }

        private void ApplyMissionConfig()
        {
            if (MissionSession.Instance == null || MissionSession.Instance.CurrentMission == null)
            {
                return;
            }

            MissionConfig mission = MissionSession.Instance.CurrentMission;

            if (PressureSystem.Instance != null)
            {
                PressureSystem.Instance.SetStartingThreat(mission.startingThreat);
                PressureSystem.Instance.SetThreatGrowthMultiplier(mission.threatGrowthMultiplier);
            }
        }

        private void ResetMissionSystems()
        {
            if (SharedInventorySystem.Instance != null)
            {
                SharedInventorySystem.Instance.ClearCargo();
            }

            if (DeploymentPointsSystem.Instance != null)
            {
                DeploymentPointsSystem.Instance.ResetDefensePoints();
            }
        }

        private void ConfigureMissionFlow()
        {
            MissionConfig mission = MissionSession.Instance != null ? MissionSession.Instance.CurrentMission : null;
            MissionFlowController flowController = Object.FindFirstObjectByType<MissionFlowController>();
            if (mission == null || flowController == null)
            {
                return;
            }

            flowController.ConfigureRequirements(mission.requiredPowerCores, mission.requiredFuelGels);
        }

        private void ApplyMissionLayout()
        {
            MissionConfig mission = MissionSession.Instance != null ? MissionSession.Instance.CurrentMission : null;
            MissionLayoutController layoutController = Object.FindFirstObjectByType<MissionLayoutController>();
            if (mission == null || layoutController == null)
            {
                return;
            }

            layoutController.ApplyLayout(mission);
        }

        private void HandleMissionSuccess()
        {
            if (MissionSession.Instance != null)
            {
                MissionSession.Instance.EndMissionSuccess();
            }
        }

        private void HandleMissionFail()
        {
            if (MissionSession.Instance != null)
            {
                MissionSession.Instance.EndMissionFail();
            }
        }

        private void TrySubscribe()
        {
            if (RunPhaseController.Instance == null)
            {
                return;
            }

            RunPhaseController.Instance.OnEndSuccessEnter -= HandleMissionSuccess;
            RunPhaseController.Instance.OnEndFailEnter -= HandleMissionFail;
            RunPhaseController.Instance.OnEndSuccessEnter += HandleMissionSuccess;
            RunPhaseController.Instance.OnEndFailEnter += HandleMissionFail;
        }
    }
}
