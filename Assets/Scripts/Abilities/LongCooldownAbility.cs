using UnityEngine;

namespace GameCore
{
    public class LongCooldownAbility : PlayerAbility
    {
        public LongCooldownAbility() : base("long_ability", "Long Ability", 25f)
        {
        }

        public override void Execute(PlayerCharacter owner)
        {
            Debug.Log("Long ability used");
        }
    }
}
