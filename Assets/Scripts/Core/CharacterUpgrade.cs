using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// Base class for per-player upgrades.
    /// Each upgrade affects only the specific player who chose it.
    /// </summary>
    public abstract class CharacterUpgrade
    {
        public string upgradeName;
        public string description;

        public CharacterUpgrade(string name, string desc)
        {
            upgradeName = name;
            description = desc;
        }

        /// <summary>
        /// Apply this upgrade to a specific player's weapon and controller.
        /// </summary>
        public abstract void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller);

        public override string ToString()
        {
            return $"{upgradeName}: {description}";
        }
    }

    // ========== WEAPON UPGRADES ==========

    /// <summary>
    /// Increases player's weapon attack range.
    /// </summary>
    public class UpgradeAttackRange : CharacterUpgrade
    {
        private float rangeIncrease;

        public UpgradeAttackRange(float increase) : base("Extended Range", $"+{increase} attack range")
        {
            rangeIncrease = increase;
        }

        public override void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller)
        {
            if (weapon != null)
            {
                weapon.AddRangeModifier(rangeIncrease);
                Debug.Log($"[Upgrade] Attack range increased by {rangeIncrease}");
            }
        }
    }

    /// <summary>
    /// Increases player's weapon attack speed.
    /// </summary>
    public class UpgradeAttackSpeed : CharacterUpgrade
    {
        private float speedMultiplier;

        public UpgradeAttackSpeed(float multiplier) : base("Faster Attacks", $"+{(multiplier - 1f) * 100:F0}% attack speed")
        {
            speedMultiplier = multiplier;
        }

        public override void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller)
        {
            if (weapon != null)
            {
                weapon.MultiplyAttackSpeed(speedMultiplier);
                Debug.Log($"[Upgrade] Attack speed multiplied by {speedMultiplier}");
            }
        }
    }

    /// <summary>
    /// Increases player's weapon damage.
    /// </summary>
    public class UpgradeDamage : CharacterUpgrade
    {
        private float damageIncrease;

        public UpgradeDamage(float increase) : base("More Damage", $"+{increase} damage per attack")
        {
            damageIncrease = increase;
        }

        public override void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller)
        {
            if (weapon != null)
            {
                weapon.AddDamageModifier(damageIncrease);
                Debug.Log($"[Upgrade] Damage increased by {damageIncrease}");
            }
        }
    }

    // ========== MOVEMENT UPGRADES ==========

    /// <summary>
    /// Increases player movement speed.
    /// </summary>
    public class UpgradeMoveSpeed : CharacterUpgrade
    {
        private float speedIncrease;

        public UpgradeMoveSpeed(float increase) : base("Faster Movement", $"+{increase} movement speed")
        {
            speedIncrease = increase;
        }

        public override void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller)
        {
            if (controller != null)
            {
                controller.AddMoveSpeedModifier(speedIncrease);
                Debug.Log($"[Upgrade] Movement speed increased by {speedIncrease}");
            }
        }
    }

    // ========== UTILITY UPGRADES ==========

    /// <summary>
    /// Example of a multi-stat upgrade.
    /// </summary>
    public class UpgradeBalanced : CharacterUpgrade
    {
        public UpgradeBalanced() : base("Balanced Growth", "+0.3 range, +0.5 speed, +2 damage")
        {
        }

        public override void ApplyToPlayer(AutoAttackWeapon weapon, CharacterMover controller)
        {
            if (weapon != null)
            {
                weapon.AddRangeModifier(0.3f);
                weapon.AddDamageModifier(2f);
            }

            if (controller != null)
            {
                controller.AddMoveSpeedModifier(0.5f);
            }

            Debug.Log("[Upgrade] Balanced stats improved");
        }
    }
}
