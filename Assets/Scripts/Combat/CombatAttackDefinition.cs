using UnityEngine;

namespace GameCore
{
    public enum CombatAttackType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(menuName = "GameCore/Combat/Attack Definition", fileName = "AttackDefinition")]
    public class CombatAttackDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string attackId = "attack_basic";
        public string displayName = "Basic Attack";
        public CombatAttackType attackType = CombatAttackType.Ranged;
        public Sprite icon;

        [Header("Timing (seconds)")]
        [Min(0f)] public float windupDuration = 0.1f;
        [Min(0f)] public float activeDuration = 0.2f;
        [Min(0f)] public float recoveryDuration = 0.2f;

        [Header("Fire Moment (Ranged)")]
        public bool useNormalizedFireMoment = true;
        [Range(0f, 1f)] public float fireMomentNormalized = 0.5f;
        [Min(0f)] public float fireMomentSeconds = 0.05f;

        [Header("Damage")]
        [Min(0f)] public float damage = 10f;

        [Header("Range / Shape")]
        [Min(0f)] public float range = 6f;
        [Range(0f, 180f)] public float arcAngle = 90f;
        [Min(0f)] public float hitRadius = 1.5f;

        [Header("Targeting")]
        public LayerMask targetMask = ~0;

        public float FireMomentTime
        {
            get
            {
                if (activeDuration <= 0f)
                {
                    return 0f;
                }

                return useNormalizedFireMoment
                    ? Mathf.Clamp01(fireMomentNormalized) * activeDuration
                    : Mathf.Min(fireMomentSeconds, activeDuration);
            }
        }
    }
}
