using UnityEngine;

namespace GameCore
{
    public interface ICombatPresentation
    {
        void PlayAttackStart(CombatAttackDefinition attack);
        void PlayWindupStart(CombatAttackDefinition attack);
        void PlayActiveStart(CombatAttackDefinition attack);
        void PlayRecoveryStart(CombatAttackDefinition attack);
        void SetHitWindow(CombatAttackDefinition attack, bool enabled);
        void PlayFireMoment(CombatAttackDefinition attack, Vector3 origin, Vector3 direction, RaycastHit? hitInfo);
        void PlayHitImpact(Vector3 position, CombatAttackDefinition attack);
        void PlayAttackEnd(CombatAttackDefinition attack);
    }
}
