using UnityEngine;

namespace GameCore
{
    [RequireComponent(typeof(Collider))]
    public class PortalSafeZone : MonoBehaviour
    {
        [SerializeField] private MissionFlowController flowController;
        [SerializeField] private string playerTag = "Player";

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            if (flowController == null)
            {
                flowController = Object.FindFirstObjectByType<MissionFlowController>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (flowController != null)
            {
                flowController.HandlePortalExit();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (flowController != null)
            {
                flowController.HandlePortalEnter();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (flowController != null)
            {
                flowController.HandlePortalEnter();
            }
        }
    }
}
