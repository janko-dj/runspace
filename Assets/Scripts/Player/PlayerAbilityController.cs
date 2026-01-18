using UnityEngine;

namespace GameCore
{
    public class PlayerAbilityController : MonoBehaviour
    {
        [Header("Ability Slots")]
        [SerializeField] private PlayerAbilityInstance abilitySlot1;
        [SerializeField] private PlayerAbilityInstance abilitySlot2;
        [SerializeField] private string abilitySlot1Id = "short_ability";
        [SerializeField] private string abilitySlot2Id = "long_ability";

        private PlayerCharacter owner;

        public PlayerAbilityInstance AbilitySlot1 => abilitySlot1;
        public PlayerAbilityInstance AbilitySlot2 => abilitySlot2;
        public string AbilitySlot1Id => abilitySlot1Id;
        public string AbilitySlot2Id => abilitySlot2Id;

        private void Awake()
        {
            owner = GetComponent<PlayerCharacter>();

            if (abilitySlot1 == null)
            {
                PlayerAbility ability = AbilityLibrary.Create(abilitySlot1Id) ?? new ShortCooldownAbility();
                abilitySlot1 = new PlayerAbilityInstance(ability);
            }

            if (abilitySlot2 == null)
            {
                PlayerAbility ability = AbilityLibrary.Create(abilitySlot2Id) ?? new LongCooldownAbility();
                abilitySlot2 = new PlayerAbilityInstance(ability);
            }
        }

        private void Update()
        {
            TickCooldowns(Time.deltaTime);
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                TryExecuteSlot(abilitySlot1, 1);
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                TryExecuteSlot(abilitySlot2, 2);
            }
        }

        private void TickCooldowns(float deltaTime)
        {
            abilitySlot1?.TickCooldown(deltaTime);
            abilitySlot2?.TickCooldown(deltaTime);
        }

        private void TryExecuteSlot(PlayerAbilityInstance slot, int slotIndex)
        {
            if (slot == null || slot.Ability == null)
            {
                Debug.LogWarning($"[Ability] Slot {slotIndex} is empty");
                return;
            }

            if (!slot.TryExecute(owner))
            {
                Debug.Log($"[Ability] {slot.Ability.displayName} on cooldown ({slot.RemainingCooldown:F1}s)");
                return;
            }

            Debug.Log($"[Ability] Used {slot.Ability.displayName} (Cooldown: {slot.Ability.cooldown:F1}s)");
        }
    }
}
