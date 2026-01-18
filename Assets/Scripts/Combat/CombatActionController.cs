using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class CombatActionController : MonoBehaviour
    {
        public enum CombatPhase
        {
            Idle,
            Windup,
            Active,
            Recovery
        }

        [Header("Attacks")]
        [SerializeField] private CombatAttackDefinition primaryAttack;
        [SerializeField] private CombatAttackDefinition secondaryAttack;

        [Header("Input")]
        [SerializeField] private bool allowInput = true;
        [SerializeField] private KeyCode primaryKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode secondaryKey = KeyCode.Mouse1;

        [Header("Hit Detection")]
        [SerializeField] private float forwardOffset = 0.5f;
        [SerializeField] private bool drawDebugGizmos = true;
        [SerializeField] private float rangedHitRadius = 0.9f;

        [Header("Presentation")]
        [SerializeField] private MonoBehaviour proceduralPresentation;
        [SerializeField] private MonoBehaviour animatorPresentation;
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Transform firePoint;

        [Header("Compatibility")]
        [SerializeField] private bool disableAutoAttackWeapon = false;
        [SerializeField] private bool useWeaponStats = true;
        [SerializeField] private AutoAttackWeapon weaponSource;

        public event Action<CombatAttackDefinition> OnAttackStarted;
        public event Action<CombatAttackDefinition> OnWindupStarted;
        public event Action<CombatAttackDefinition> OnActiveStarted;
        public event Action<CombatAttackDefinition> OnRecoveryStarted;
        public event Action<CombatAttackDefinition> OnAttackEnded;
        public event Action<CombatAttackDefinition> OnFireMoment;
        public event Action<CombatAttackDefinition, bool> OnHitWindow;

        public CombatPhase CurrentPhase => currentPhase;
        public CombatAttackDefinition CurrentAttack => currentAttack;

        private CombatPhase currentPhase = CombatPhase.Idle;
        private CombatAttackDefinition currentAttack;
        private float phaseTimer;
        private float activeElapsed;
        private bool fireMomentTriggered;
        private HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
        private ICombatPresentation proceduralPresenter;
        private ICombatPresentation animatorPresenter;
        private AutoAttackWeapon autoAttackWeapon;

        private void Awake()
        {
            autoAttackWeapon = GetComponent<AutoAttackWeapon>();
            if (weaponSource == null)
            {
                weaponSource = autoAttackWeapon;
            }
            if (disableAutoAttackWeapon && autoAttackWeapon != null)
            {
                autoAttackWeapon.enabled = false;
            }

            if (weaponRoot == null)
            {
                weaponRoot = FindChildTransform("WeaponRoot");
            }

            if (firePoint == null)
            {
                firePoint = FindChildTransform("FirePoint");
            }

            proceduralPresenter = proceduralPresentation as ICombatPresentation;
            animatorPresenter = animatorPresentation as ICombatPresentation;
        }

        private void Update()
        {
            if (allowInput)
            {
                HandleInput();
            }

            TickState(Time.deltaTime);
        }

        public bool TryStartAttack(CombatAttackDefinition attack)
        {
            if (attack == null)
            {
                return false;
            }

            if (currentPhase != CombatPhase.Idle)
            {
                return false;
            }

            currentAttack = attack;
            hitTargets.Clear();
            fireMomentTriggered = false;
            activeElapsed = 0f;

            StartPhase(CombatPhase.Windup, attack.windupDuration);
            OnAttackStarted?.Invoke(attack);
            CallPresentation(p => p.PlayAttackStart(attack));
            return true;
        }

        private void HandleInput()
        {
            if (primaryAttack != null && Input.GetKeyDown(primaryKey))
            {
                TryStartAttack(primaryAttack);
            }

            if (secondaryAttack != null && Input.GetKeyDown(secondaryKey))
            {
                TryStartAttack(secondaryAttack);
            }
        }

        private void TickState(float deltaTime)
        {
            if (currentPhase == CombatPhase.Idle || currentAttack == null)
            {
                return;
            }

            phaseTimer -= deltaTime;

            switch (currentPhase)
            {
                case CombatPhase.Windup:
                    if (phaseTimer <= 0f)
                    {
                        StartPhase(CombatPhase.Active, currentAttack.activeDuration);
                    }
                    break;
                case CombatPhase.Active:
                    activeElapsed += deltaTime;
                    HandleActivePhase();
                    if (phaseTimer <= 0f)
                    {
                        EndHitWindow();
                        StartPhase(CombatPhase.Recovery, currentAttack.recoveryDuration);
                    }
                    break;
                case CombatPhase.Recovery:
                    if (phaseTimer <= 0f)
                    {
                        FinishAttack();
                    }
                    break;
            }
        }

        private void HandleActivePhase()
        {
            if (currentAttack == null)
            {
                return;
            }

            if (currentAttack.attackType == CombatAttackType.Ranged)
            {
                if (!fireMomentTriggered && activeElapsed >= currentAttack.FireMomentTime)
                {
                    TriggerFireMoment();
                }
            }
            else
            {
                PerformMeleeHits();
            }
        }

        private void TriggerFireMoment()
        {
            fireMomentTriggered = true;
            OnFireMoment?.Invoke(currentAttack);

            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
            Vector3 direction = transform.forward;

            float range = GetEffectiveRange();
            float radius = Mathf.Max(rangedHitRadius, currentAttack.hitRadius);
            bool hasHit = TryGetDamageableHit(origin, direction, range, radius, out RaycastHit hit);
            if (hasHit)
            {
                ApplyDamage(hit.collider, hit.point);
                CallPresentation(p => p.PlayHitImpact(hit.point, currentAttack));
            }

            CallPresentation(p => p.PlayFireMoment(currentAttack, origin, direction, hasHit ? (RaycastHit?)hit : null));
        }

        private void PerformMeleeHits()
        {
            Vector3 origin = transform.position + transform.forward * forwardOffset;
            Collider[] hits = Physics.OverlapSphere(origin, currentAttack.hitRadius, currentAttack.targetMask, QueryTriggerInteraction.Collide);
            foreach (Collider collider in hits)
            {
                if (collider == null)
                {
                    continue;
                }

                IDamageable damageable = collider.GetComponentInParent<IDamageable>();
                if (damageable == null || hitTargets.Contains(damageable))
                {
                    continue;
                }

                Vector3 toTarget = collider.transform.position - transform.position;
                toTarget.y = 0f;
                Vector3 forward = transform.forward;
                forward.y = 0f;

                float angle = Vector3.Angle(forward, toTarget);
                if (angle > currentAttack.arcAngle * 0.5f)
                {
                    continue;
                }

                hitTargets.Add(damageable);
                damageable.TakeDamage(GetEffectiveDamage());
                CallPresentation(p => p.PlayHitImpact(collider.ClosestPoint(transform.position), currentAttack));
            }
        }

        private void ApplyDamage(Collider collider, Vector3 hitPoint)
        {
            IDamageable damageable = collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(GetEffectiveDamage());
            }
        }

        private bool TryGetDamageableHit(Vector3 origin, Vector3 direction, float range, float radius, out RaycastHit hit)
        {
            int mask = currentAttack.targetMask.value;
            if (mask == 0)
            {
                mask = ~0;
            }

            float effectiveRadius = Mathf.Max(0.35f, radius);
            RaycastHit[] hits = Physics.SphereCastAll(origin, effectiveRadius, direction, range, mask, QueryTriggerInteraction.Collide);
            float bestDistance = float.MaxValue;
            RaycastHit bestHit = default;
            bool found = false;

            foreach (RaycastHit candidate in hits)
            {
                if (candidate.collider == null)
                {
                    continue;
                }

                IDamageable damageable = candidate.collider.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                if (candidate.distance < bestDistance)
                {
                    bestDistance = candidate.distance;
                    bestHit = candidate;
                    found = true;
                }
            }

            if (!found)
            {
                hit = default;
                return false;
            }

            hit = bestHit;
            return true;
        }

        private float GetEffectiveDamage()
        {
            if (useWeaponStats && weaponSource != null)
            {
                return weaponSource.GetEffectiveAttackDamage();
            }

            return currentAttack != null ? currentAttack.damage : 0f;
        }

        private float GetEffectiveRange()
        {
            if (useWeaponStats && weaponSource != null)
            {
                return weaponSource.GetEffectiveAttackRange();
            }

            return currentAttack != null ? currentAttack.range : 0f;
        }

        private void StartPhase(CombatPhase phase, float duration)
        {
            currentPhase = phase;
            phaseTimer = duration;

            switch (phase)
            {
                case CombatPhase.Windup:
                    OnWindupStarted?.Invoke(currentAttack);
                    CallPresentation(p => p.PlayWindupStart(currentAttack));
                    break;
                case CombatPhase.Active:
                    OnActiveStarted?.Invoke(currentAttack);
                    CallPresentation(p => p.PlayActiveStart(currentAttack));
                    if (currentAttack.attackType == CombatAttackType.Melee)
                    {
                        StartHitWindow();
                    }
                    break;
                case CombatPhase.Recovery:
                    OnRecoveryStarted?.Invoke(currentAttack);
                    CallPresentation(p => p.PlayRecoveryStart(currentAttack));
                    break;
            }
        }

        private void StartHitWindow()
        {
            OnHitWindow?.Invoke(currentAttack, true);
            CallPresentation(p => p.SetHitWindow(currentAttack, true));
        }

        private void EndHitWindow()
        {
            OnHitWindow?.Invoke(currentAttack, false);
            CallPresentation(p => p.SetHitWindow(currentAttack, false));
        }

        private void FinishAttack()
        {
            CallPresentation(p => p.PlayAttackEnd(currentAttack));
            OnAttackEnded?.Invoke(currentAttack);

            currentPhase = CombatPhase.Idle;
            currentAttack = null;
            phaseTimer = 0f;
        }

        private void CallPresentation(Action<ICombatPresentation> action)
        {
            if (proceduralPresenter != null)
            {
                action(proceduralPresenter);
            }

            if (animatorPresenter != null && animatorPresenter != proceduralPresenter)
            {
                action(animatorPresenter);
            }
        }

        private Transform FindChildTransform(string childName)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                return child;
            }

            foreach (Transform descendant in GetComponentsInChildren<Transform>(true))
            {
                if (descendant.name == childName)
                {
                    return descendant;
                }
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawDebugGizmos || currentAttack == null)
            {
                return;
            }

            if (currentAttack.attackType == CombatAttackType.Melee)
            {
                Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.5f);
                Vector3 origin = transform.position + transform.forward * forwardOffset;
                Gizmos.DrawWireSphere(origin, currentAttack.hitRadius);

                Vector3 forward = transform.forward;
                forward.y = 0f;
                Quaternion leftRot = Quaternion.AngleAxis(-currentAttack.arcAngle * 0.5f, Vector3.up);
                Quaternion rightRot = Quaternion.AngleAxis(currentAttack.arcAngle * 0.5f, Vector3.up);
                Gizmos.DrawLine(transform.position, transform.position + (leftRot * forward).normalized * currentAttack.range);
                Gizmos.DrawLine(transform.position, transform.position + (rightRot * forward).normalized * currentAttack.range);
            }
            else
            {
                Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.7f);
                Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
                Gizmos.DrawLine(origin, origin + transform.forward * currentAttack.range);
            }
        }
    }
}
