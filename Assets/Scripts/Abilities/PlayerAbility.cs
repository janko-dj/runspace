using UnityEngine;

namespace GameCore
{
    public abstract class PlayerAbility
    {
        public string abilityId { get; }
        public string displayName { get; }
        public float cooldown { get; }
        public Sprite icon { get; }

        protected PlayerAbility(string abilityId, string displayName, float cooldown, Sprite icon = null)
        {
            this.abilityId = abilityId;
            this.displayName = displayName;
            this.cooldown = cooldown;
            this.icon = icon;
        }

        public virtual bool CanExecute()
        {
            return true;
        }

        public abstract void Execute(PlayerCharacter owner);
    }
}
