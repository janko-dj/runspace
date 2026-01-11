using UnityEngine;

namespace GameCore
{
    [RequireComponent(typeof(Collider))]
    public class BonusPickup : MonoBehaviour
    {
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

            Debug.Log("[BonusPickup] Collected bonus pickup.");
            Destroy(gameObject);
        }
    }
}
