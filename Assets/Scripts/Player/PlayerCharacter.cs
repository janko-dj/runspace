using UnityEngine;

namespace GameCore
{
    public class PlayerCharacter : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private CharacterMover mover;
        [SerializeField] private AutoAttackWeapon weapon;
        [SerializeField] private CharacterHealth health;
        [SerializeField] private PlayerAbilityController abilityController;
        [SerializeField] private PlayerLookController lookController;

        public CharacterMover Mover => mover;
        public AutoAttackWeapon Weapon => weapon;
        public CharacterHealth Health => health;
        public PlayerAbilityController AbilityController => abilityController;
        public PlayerLookController LookController => lookController;

        private void Awake()
        {
            if (mover == null)
                mover = GetComponent<CharacterMover>();
            if (weapon == null)
                weapon = GetComponent<AutoAttackWeapon>();
            if (health == null)
                health = GetComponent<CharacterHealth>();
            if (abilityController == null)
                abilityController = GetComponent<PlayerAbilityController>();
            if (lookController == null)
                lookController = GetComponent<PlayerLookController>();
        }
    }
}
