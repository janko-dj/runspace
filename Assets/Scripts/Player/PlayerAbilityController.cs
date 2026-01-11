using UnityEngine;

namespace GameCore
{
    public class PlayerAbilityController : MonoBehaviour
    {
        [Header("Ability Slots")]
        [SerializeField] private PlayerAbilityInstance abilitySlot1;
        [SerializeField] private PlayerAbilityInstance abilitySlot2;

        private PlayerCharacter owner;

        private void Awake()
        {
            owner = GetComponent<PlayerCharacter>();

            if (abilitySlot1 == null)
            {
                abilitySlot1 = new PlayerAbilityInstance(new ShortCooldownAbility());
            }

            if (abilitySlot2 == null)
            {
                abilitySlot2 = new PlayerAbilityInstance(new LongCooldownAbility());
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
