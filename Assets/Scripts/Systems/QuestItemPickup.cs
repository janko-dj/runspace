using UnityEngine;

namespace GameCore
{
    [RequireComponent(typeof(Collider))]
    public class QuestItemPickup : MonoBehaviour
    {
        [SerializeField] private QuestItem questItem;
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

            if (questItem == null || PlayerProgress.Instance == null)
            {
                return;
            }

            PlayerProgress.Instance.AddQuestItem(questItem);
            Debug.Log($"[QuestItem] Picked up {questItem.displayName}");
            Destroy(gameObject);
        }

        public void SetQuestItem(QuestItem item)
        {
            questItem = item;
        }
    }
}
