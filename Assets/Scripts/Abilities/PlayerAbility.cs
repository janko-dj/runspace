namespace GameCore
{
    public abstract class PlayerAbility
    {
        public string abilityId { get; }
        public string displayName { get; }
        public float cooldown { get; }

        protected PlayerAbility(string abilityId, string displayName, float cooldown)
        {
            this.abilityId = abilityId;
            this.displayName = displayName;
            this.cooldown = cooldown;
        }

        public virtual bool CanExecute()
        {
            return true;
        }

        public abstract void Execute(PlayerCharacter owner);
    }
}
