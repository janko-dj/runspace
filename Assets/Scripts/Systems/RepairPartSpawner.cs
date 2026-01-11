using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class RepairPartSpawner : MonoBehaviour
    {
        [SerializeField] private float pickupHeight = 0.5f;
        [SerializeField] private Vector3 pickupScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private string portalName = "PortalGate";

        public void SpawnForMission(MissionConfig mission)
        {
            if (mission == null)
            {
                return;
            }

            ClearExistingPickups();

            Vector3 center = GetPortalCenter();
            float minDistance = Mathf.Max(0f, mission.pickupMinDistance);
            float maxDistance = Mathf.Max(minDistance + 1f, mission.pickupMaxDistance);

            int powerCount = Mathf.Max(0, mission.spawnPowerCores);
            int fuelCount = Mathf.Max(0, mission.spawnFuelGels);

            SpawnPickups(InventoryItemType.PowerCore, powerCount, center, minDistance, maxDistance);
            SpawnPickups(InventoryItemType.FuelGel, fuelCount, center, minDistance, maxDistance);
        }

        private void SpawnPickups(InventoryItemType itemType, int count, Vector3 center, float minDistance, float maxDistance)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 position = FindSpawnPosition(center, minDistance, maxDistance);
                GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = $"RepairPickup_{itemType}_{i + 1}";
                pickup.transform.position = position;
                pickup.transform.localScale = pickupScale;
                pickup.transform.SetParent(transform, true);

                RepairPartPickup pickupComponent = pickup.AddComponent<RepairPartPickup>();
                pickupComponent.SetItemType(itemType);

                ApplyMinimapIcon(pickup, itemType);
            }
        }

        private Vector3 FindSpawnPosition(Vector3 center, float minDistance, float maxDistance)
        {
            float halfSize = GetGroundHalfSize();
            for (int i = 0; i < 30; i++)
            {
                Vector2 ring = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
                Vector3 candidate = new Vector3(center.x + ring.x, pickupHeight, center.z + ring.y);
                candidate.x = Mathf.Clamp(candidate.x, -halfSize + 2f, halfSize - 2f);
                candidate.z = Mathf.Clamp(candidate.z, -halfSize + 2f, halfSize - 2f);
                return candidate;
            }

            return new Vector3(center.x + minDistance, pickupHeight, center.z);
        }

        private float GetGroundHalfSize()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                return 60f;
            }

            return Mathf.Max(ground.transform.localScale.x, ground.transform.localScale.z) * 5f;
        }

        private Vector3 GetPortalCenter()
        {
            GameObject portal = GameObject.Find(portalName);
            if (portal != null)
            {
                return portal.transform.position;
            }

            GameObject portalRoot = GameObject.Find("=== PORTAL ===");
            if (portalRoot != null)
            {
                return portalRoot.transform.position;
            }

            return Vector3.zero;
        }

        private void ClearExistingPickups()
        {
            RepairPartPickup[] pickups = Object.FindObjectsByType<RepairPartPickup>(FindObjectsSortMode.None);
            foreach (RepairPartPickup pickup in pickups)
            {
                if (pickup != null)
                {
                    Destroy(pickup.gameObject);
                }
            }
        }

        private void ApplyMinimapIcon(GameObject pickup, InventoryItemType itemType)
        {
            MinimapIcon icon = pickup.GetComponent<MinimapIcon>();
            if (icon == null)
            {
                icon = pickup.AddComponent<MinimapIcon>();
            }

            Color color = itemType == InventoryItemType.FuelGel ? Color.red : Color.yellow;
            icon.SetIcon(color, 1.4f);
        }
    }
}
