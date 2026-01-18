using System.Collections;
using UnityEngine;

namespace GameCore
{
    public class GunnerBurstAbility : PlayerAbility
    {
        private const float BurstDuration = 1.5f;
        private const float SpeedMultiplier = 2f;

        public GunnerBurstAbility() : base("gunner_burst", "Burst Shot", 3f)
        {
        }

        public override void Execute(PlayerCharacter owner)
        {
            if (owner == null || owner.Weapon == null)
            {
                return;
            }

            owner.StartCoroutine(ApplyBurst(owner.Weapon));
            Debug.Log("[Ability] Burst Shot activated");
        }

        private IEnumerator ApplyBurst(AutoAttackWeapon weapon)
        {
            weapon.MultiplyAttackSpeed(SpeedMultiplier);
            yield return new WaitForSeconds(BurstDuration);
            weapon.MultiplyAttackSpeed(1f / SpeedMultiplier);
            Debug.Log("[Ability] Burst Shot ended");
        }
    }
}
