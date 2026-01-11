namespace GameCore
{
    public class PlayerAbilityInstance
    {
        private readonly PlayerAbility ability;
        private float remainingCooldown;

        public PlayerAbility Ability => ability;
        public float RemainingCooldown => remainingCooldown;

        public PlayerAbilityInstance(PlayerAbility ability)
        {
            this.ability = ability;
        }

        public bool TryExecute(PlayerCharacter owner)
        {
            if (ability == null)
            {
                return false;
            }

            if (remainingCooldown > 0f)
            {
                return false;
            }

            if (!ability.CanExecute())
            {
                return false;
            }

            ability.Execute(owner);
            remainingCooldown = ability.cooldown;
            return true;
        }

        public void TickCooldown(float deltaTime)
        {
            if (remainingCooldown <= 0f)
            {
                return;
            }

            remainingCooldown -= deltaTime;
            if (remainingCooldown < 0f)
            {
                remainingCooldown = 0f;
            }
        }
    }
}
