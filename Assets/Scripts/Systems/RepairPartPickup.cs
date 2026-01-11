using UnityEngine;

namespace GameCore
{
    [RequireComponent(typeof(Collider))]
    public class RepairPartPickup : MonoBehaviour
    {
        [SerializeField] private InventoryItemType itemType = InventoryItemType.PowerCore;
        [SerializeField] private string playerTag = "Player";

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (SharedInventorySystem.Instance == null)
            {
                return;
            }

            if (SharedInventorySystem.Instance.TryAddItem(itemType))
            {
                Debug.Log($"[Pickup] Collected {itemType}");
                Destroy(gameObject);
            }
        }

        public void SetItemType(InventoryItemType newType)
        {
            itemType = newType;
        }
    }
}
