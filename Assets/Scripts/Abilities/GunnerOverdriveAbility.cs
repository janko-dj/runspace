using System.Collections;
using UnityEngine;

namespace GameCore
{
    public class GunnerOverdriveAbility : PlayerAbility
    {
        private const float Duration = 6f;
        private const float DamageBonus = 12f;
        private const float RangeBonus = 1.5f;
        private const float SpeedMultiplier = 1.35f;

        public GunnerOverdriveAbility() : base("gunner_overdrive", "Overdrive", 22f)
        {
        }

        public override void Execute(PlayerCharacter owner)
        {
            if (owner == null || owner.Weapon == null)
            {
                return;
            }

            owner.StartCoroutine(ApplyOverdrive(owner.Weapon));
            Debug.Log("[Ability] Overdrive activated");
        }

        private IEnumerator ApplyOverdrive(AutoAttackWeapon weapon)
        {
            weapon.AddDamageModifier(DamageBonus);
            weapon.AddRangeModifier(RangeBonus);
            weapon.MultiplyAttackSpeed(SpeedMultiplier);
            yield return new WaitForSeconds(Duration);
            weapon.AddDamageModifier(-DamageBonus);
            weapon.AddRangeModifier(-RangeBonus);
            weapon.MultiplyAttackSpeed(1f / SpeedMultiplier);
            Debug.Log("[Ability] Overdrive ended");
        }
    }
}
