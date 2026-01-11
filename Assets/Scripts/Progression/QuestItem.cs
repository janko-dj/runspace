using UnityEngine;

namespace GameCore
{
    [CreateAssetMenu(menuName = "Game/Quest Item", fileName = "QuestItem")]
    public class QuestItem : ScriptableObject
    {
        public string questItemId;
        public string displayName;
        public string description;
        public string unlocksMissionId;
    }
}
