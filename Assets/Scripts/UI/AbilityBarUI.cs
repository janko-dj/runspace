using UnityEngine;

namespace GameCore
{
    public class AbilityBarUI : MonoBehaviour
    {
        [SerializeField] private AbilityIconWidget slot1Widget;
        [SerializeField] private AbilityIconWidget slot2Widget;
        [SerializeField] private Color slot1Color = new Color(0.2f, 0.7f, 1f);
        [SerializeField] private Color slot2Color = new Color(1f, 0.6f, 0.2f);

        private PlayerAbilityController abilityController;

        private void Awake()
        {
            if (slot1Widget != null)
            {
                slot1Widget.Configure("Q", slot1Color);
            }

            if (slot2Widget != null)
            {
                slot2Widget.Configure("E", slot2Color);
            }
        }

        private void Start()
        {
            FindAbilityController();
        }

        private void Update()
        {
            if (abilityController == null)
            {
                FindAbilityController();
                if (abilityController == null)
                {
                    return;
                }
            }

            UpdateSlot(slot1Widget, abilityController.AbilitySlot1);
            UpdateSlot(slot2Widget, abilityController.AbilitySlot2);
        }

        private void FindAbilityController()
        {
            abilityController = Object.FindFirstObjectByType<PlayerAbilityController>();
        }

        private void UpdateSlot(AbilityIconWidget widget, PlayerAbilityInstance instance)
        {
            if (widget == null)
            {
                return;
            }

            if (instance == null || instance.Ability == null)
            {
                widget.SetEmpty();
                return;
            }

            widget.SetAbilityInfo(instance.Ability.displayName, instance.Ability.icon);
            float remaining = instance.RemainingCooldown;
            float total = instance.Ability.cooldown;
            widget.SetState(remaining, total, false);
        }
    }
}
