using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class PlayerProgress : MonoBehaviour
    {
        public static PlayerProgress Instance { get; private set; }

        private readonly HashSet<string> collectedQuestItemIds = new HashSet<string>();

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

        public bool HasQuestItem(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return collectedQuestItemIds.Contains(id);
        }

        public void AddQuestItem(QuestItem item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.questItemId))
            {
                return;
            }

            if (collectedQuestItemIds.Add(item.questItemId))
            {
                Debug.Log($"[Progress] Quest item obtained: {item.displayName}");
            }
        }
    }
}
