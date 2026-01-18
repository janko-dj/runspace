using System.Collections;
using UnityEngine;

namespace GameCore
{
    public class ProceduralCombatPresentation : MonoBehaviour, ICombatPresentation
    {
        [Header("Attachments")]
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Transform firePoint;

        [Header("Recoil (Ranged)")]
        [SerializeField] private float recoilDistance = 0.12f;
        [SerializeField] private float recoilAngle = 6f;
        [SerializeField] private float recoilReturnSpeed = 18f;

        [Header("Melee Swing")]
        [SerializeField] private float swingAngle = 90f;
        [SerializeField] private float swingSpeed = 1f;
        [SerializeField] private TrailRenderer swingTrail;

        [Header("VFX (Optional Prefabs)")]
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject hitImpactPrefab;

        private Coroutine recoilRoutine;
        private Quaternion weaponOriginalRotation;
        private Vector3 weaponOriginalPosition;
        private Coroutine swingRoutine;

        private void Awake()
        {
            if (weaponRoot == null)
            {
                weaponRoot = FindChildTransform("WeaponRoot");
            }

            if (firePoint == null)
            {
                firePoint = FindChildTransform("FirePoint");
            }

            if (weaponRoot != null)
            {
                weaponOriginalRotation = weaponRoot.localRotation;
                weaponOriginalPosition = weaponRoot.localPosition;
            }

            if (swingTrail == null && weaponRoot != null)
            {
                swingTrail = weaponRoot.GetComponent<TrailRenderer>();
            }

            if (swingTrail != null)
            {
                swingTrail.emitting = false;
            }
        }

        public void PlayAttackStart(CombatAttackDefinition attack)
        {
        }

        public void PlayWindupStart(CombatAttackDefinition attack)
        {
        }

        public void PlayActiveStart(CombatAttackDefinition attack)
        {
            if (attack.attackType == CombatAttackType.Melee)
            {
                StartSwing(attack);
            }
        }

        public void PlayRecoveryStart(CombatAttackDefinition attack)
        {
        }

        public void SetHitWindow(CombatAttackDefinition attack, bool enabled)
        {
            if (attack.attackType != CombatAttackType.Melee)
            {
                return;
            }

            if (swingTrail != null)
            {
                swingTrail.emitting = enabled;
            }
        }

        public void PlayFireMoment(CombatAttackDefinition attack, Vector3 origin, Vector3 direction, RaycastHit? hitInfo)
        {
            if (attack.attackType != CombatAttackType.Ranged)
            {
                return;
            }

            TriggerRecoil();
            SpawnMuzzleFlash();
            SpawnTracer(origin, hitInfo.HasValue ? hitInfo.Value.point : origin + direction * attack.range);
        }

        public void PlayHitImpact(Vector3 position, CombatAttackDefinition attack)
        {
            SpawnHitImpact(position);
        }

        public void PlayAttackEnd(CombatAttackDefinition attack)
        {
        }

        private void StartSwing(CombatAttackDefinition attack)
        {
            if (weaponRoot == null)
            {
                return;
            }

            if (swingRoutine != null)
            {
                StopCoroutine(swingRoutine);
            }

            swingRoutine = StartCoroutine(SwingRoutine(attack.activeDuration));
        }

        private IEnumerator SwingRoutine(float duration)
        {
            if (weaponRoot == null)
            {
                yield break;
            }

            Quaternion startRotation = weaponOriginalRotation;
            Quaternion endRotation = weaponOriginalRotation * Quaternion.Euler(0f, swingAngle, 0f);

            float timer = 0f;
            float effectiveDuration = Mathf.Max(0.01f, duration / Mathf.Max(0.01f, swingSpeed));

            while (timer < effectiveDuration)
            {
                float t = timer / effectiveDuration;
                weaponRoot.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                timer += Time.deltaTime;
                yield return null;
            }

            weaponRoot.localRotation = weaponOriginalRotation;
        }

        private void TriggerRecoil()
        {
            if (weaponRoot == null)
            {
                return;
            }

            if (recoilRoutine != null)
            {
                StopCoroutine(recoilRoutine);
            }

            recoilRoutine = StartCoroutine(RecoilRoutine());
        }

        private IEnumerator RecoilRoutine()
        {
            if (weaponRoot == null)
            {
                yield break;
            }

            weaponRoot.localPosition = weaponOriginalPosition + new Vector3(0f, 0f, -recoilDistance);
            weaponRoot.localRotation = weaponOriginalRotation * Quaternion.Euler(-recoilAngle, 0f, 0f);

            while (Vector3.Distance(weaponRoot.localPosition, weaponOriginalPosition) > 0.001f)
            {
                weaponRoot.localPosition = Vector3.Lerp(weaponRoot.localPosition, weaponOriginalPosition, Time.deltaTime * recoilReturnSpeed);
                weaponRoot.localRotation = Quaternion.Slerp(weaponRoot.localRotation, weaponOriginalRotation, Time.deltaTime * recoilReturnSpeed);
                yield return null;
            }

            weaponRoot.localPosition = weaponOriginalPosition;
            weaponRoot.localRotation = weaponOriginalRotation;
        }

        private void SpawnMuzzleFlash()
        {
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position + transform.forward * 0.5f;
            if (muzzleFlashPrefab != null)
            {
                SpawnTempPrefab(muzzleFlashPrefab, spawnPosition, Quaternion.LookRotation(transform.forward), 0.25f);
                return;
            }

            SpawnTempPrimitive(spawnPosition, new Color(1f, 0.8f, 0.2f), 0.12f);
        }

        private void SpawnHitImpact(Vector3 position)
        {
            if (hitImpactPrefab != null)
            {
                SpawnTempPrefab(hitImpactPrefab, position, Quaternion.identity, 0.35f);
                return;
            }

            SpawnTempPrimitive(position, new Color(1f, 0.4f, 0.2f), 0.1f);
        }

        private void SpawnTracer(Vector3 start, Vector3 end)
        {
            GameObject tracer = new GameObject("Tracer");
            LineRenderer line = tracer.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.03f;
            line.endWidth = 0.01f;
            line.material = new Material(Shader.Find("Unlit/Color")) { color = new Color(1f, 0.9f, 0.3f) };
            line.useWorldSpace = true;
            Destroy(tracer, 0.12f);
        }

        private void SpawnTempPrefab(GameObject prefab, Vector3 position, Quaternion rotation, float lifetime)
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            Destroy(instance, lifetime);
        }

        private void SpawnTempPrimitive(Vector3 position, Color color, float size)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * size;
            Destroy(sphere.GetComponent<Collider>());

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = color;
            }

            Destroy(sphere, 0.2f);
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
    }
}
