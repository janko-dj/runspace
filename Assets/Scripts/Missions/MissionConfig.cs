using UnityEngine;

namespace GameCore
{
    [CreateAssetMenu(menuName = "Game/Mission Config", fileName = "MissionConfig")]
    public class MissionConfig : ScriptableObject
    {
        public string missionId;
        public string displayName;
        [Header("Progression")]
        public QuestItem questItemReward;
        public QuestItem requiredQuestItem;
        [Header("Repair Parts")]
        public int requiredPowerCores = 2;
        public int requiredFuelGels = 2;
        public int spawnPowerCores = 2;
        public int spawnFuelGels = 2;
        public float pickupMinDistance = 20f;
        public float pickupMaxDistance = 60f;
        [Header("Layout")]
        public MissionLayout layout;
        public int questItemSpawnCount = 1;
        public int bonusPickupCount = 2;
        public float bonusWeightSafe = 1f;
        public float bonusWeightMid = 1f;
        public float bonusWeightFar = 1f;
        public bool useSpawnSeed = false;
        public int spawnSeed = 12345;
        public int startingThreat;
        public float threatGrowthMultiplier;
        public string sceneName;
    }
}
