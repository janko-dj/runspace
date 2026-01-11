using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class MissionFlowController : MonoBehaviour
    {
        [System.Serializable]
        public class RequiredPart
        {
            public InventoryItemType itemType = InventoryItemType.PowerCore;
            public int requiredCount = 1;
        }

        [SerializeField] private List<RequiredPart> requiredParts = new List<RequiredPart>();

        private bool hasLeftPortal = false;
        private bool allPartsCollected = false;
        private bool isPlayerInPortal = false;

        private void Start()
        {
            if (SharedInventorySystem.Instance != null)
            {
                SharedInventorySystem.Instance.OnItemAdded += HandleItemAdded;
            }
        }

        private void OnDisable()
        {
            if (SharedInventorySystem.Instance != null)
            {
                SharedInventorySystem.Instance.OnItemAdded -= HandleItemAdded;
            }
        }

        public void HandlePortalExit()
        {
            if (RunPhaseController.Instance == null)
            {
                return;
            }

            isPlayerInPortal = false;

            if (RunPhaseController.Instance.CurrentPhase == RunPhase.Landing && !hasLeftPortal)
            {
                hasLeftPortal = true;
                RunPhaseController.Instance.TransitionToPhase(RunPhase.Expedition);
                Debug.Log("[Flow] Left portal zone -> Expedition");
            }
            else if (RunPhaseController.Instance.CurrentPhase == RunPhase.Landing)
            {
                RunPhaseController.Instance.TransitionToPhase(RunPhase.Expedition);
                Debug.Log("[Flow] Left portal zone -> Expedition (repeat)");
            }
        }

        public void HandlePortalEnter()
        {
            if (RunPhaseController.Instance == null)
            {
                return;
            }

            isPlayerInPortal = true;

            if (allPartsCollected && RunPhaseController.Instance.CurrentPhase == RunPhase.RunBack)
            {
                RunPhaseController.Instance.TransitionToPhase(RunPhase.FinalStand);
                Debug.Log("[Flow] Returned to portal with parts -> FinalStand");
            }
        }

        private void HandleItemAdded(InventoryItemType itemType)
        {
            if (allPartsCollected)
            {
                return;
            }

            if (AreAllPartsCollected())
            {
                allPartsCollected = true;
                Debug.Log("[Flow] All repair parts collected -> RunBack");

                if (RunPhaseController.Instance != null && RunPhaseController.Instance.CurrentPhase == RunPhase.Expedition)
                {
                    RunPhaseController.Instance.TransitionToPhase(RunPhase.RunBack);
                }

                if (RunPhaseController.Instance != null
                    && RunPhaseController.Instance.CurrentPhase == RunPhase.RunBack
                    && isPlayerInPortal)
                {
                    RunPhaseController.Instance.TransitionToPhase(RunPhase.FinalStand);
                    Debug.Log("[Flow] Parts collected while in portal -> FinalStand");
                }
            }
        }

        public void ConfigureRequirements(int powerCoreCount, int fuelGelCount)
        {
            requiredParts.Clear();

            if (powerCoreCount > 0)
            {
                requiredParts.Add(new RequiredPart
                {
                    itemType = InventoryItemType.PowerCore,
                    requiredCount = powerCoreCount
                });
            }

            if (fuelGelCount > 0)
            {
                requiredParts.Add(new RequiredPart
                {
                    itemType = InventoryItemType.FuelGel,
                    requiredCount = fuelGelCount
                });
            }

            allPartsCollected = false;
            isPlayerInPortal = false;
        }

        private bool AreAllPartsCollected()
        {
            if (SharedInventorySystem.Instance == null)
            {
                return false;
            }

            foreach (RequiredPart part in requiredParts)
            {
                int count = SharedInventorySystem.Instance.CountItem(part.itemType);
                if (count < part.requiredCount)
                {
                    return false;
                }
            }

            return requiredParts.Count > 0;
        }
    }
}
