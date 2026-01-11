using UnityEngine;

namespace GameCore
{
    public class ShortCooldownAbility : PlayerAbility
    {
        public ShortCooldownAbility() : base("short_ability", "Short Ability", 3f)
        {
        }

        public override void Execute(PlayerCharacter owner)
        {
            Debug.Log("Short ability used");
        }
    }
}
