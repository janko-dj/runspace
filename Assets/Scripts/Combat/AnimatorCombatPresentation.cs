using UnityEngine;

namespace GameCore
{
    public class AnimatorCombatPresentation : MonoBehaviour, ICombatPresentation
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string attackTrigger = "Attack";
        [SerializeField] private string attackTypeParam = "AttackType";
        [SerializeField] private string hitWindowParam = "HitWindow";
        [SerializeField] private int rangedTypeValue = 0;
        [SerializeField] private int meleeTypeValue = 1;

        private int attackTriggerHash;
        private int attackTypeHash;
        private int hitWindowHash;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            attackTriggerHash = Animator.StringToHash(attackTrigger);
            attackTypeHash = Animator.StringToHash(attackTypeParam);
            hitWindowHash = Animator.StringToHash(hitWindowParam);
        }

        public void PlayAttackStart(CombatAttackDefinition attack)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetInteger(attackTypeHash, attack.attackType == CombatAttackType.Melee ? meleeTypeValue : rangedTypeValue);
            animator.SetTrigger(attackTriggerHash);
        }

        public void PlayWindupStart(CombatAttackDefinition attack)
        {
        }

        public void PlayActiveStart(CombatAttackDefinition attack)
        {
        }

        public void PlayRecoveryStart(CombatAttackDefinition attack)
        {
        }

        public void SetHitWindow(CombatAttackDefinition attack, bool enabled)
        {
            if (animator == null)
            {
                return;
            }

            animator.SetBool(hitWindowHash, enabled);
        }

        public void PlayFireMoment(CombatAttackDefinition attack, Vector3 origin, Vector3 direction, RaycastHit? hitInfo)
        {
        }

        public void PlayHitImpact(Vector3 position, CombatAttackDefinition attack)
        {
        }

        public void PlayAttackEnd(CombatAttackDefinition attack)
        {
        }
    }
}
