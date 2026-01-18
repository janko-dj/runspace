using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class MissionLayoutController : MonoBehaviour
    {
        [SerializeField] private MissionLayout layout;
        [SerializeField] private float spawnHeight = 0.5f;
        [SerializeField] private float minSeparation = 4f;
        [SerializeField] private string portalName = "PortalGate";

        private readonly List<Vector3> occupiedPositions = new List<Vector3>();
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();
        private Vector3 portalPosition = Vector3.zero;
        private Vector3 mapCenter = Vector3.zero;

        public void ApplyLayout(MissionConfig mission)
        {
            if (mission != null && mission.layout != null)
            {
                layout = mission.layout;
            }

            if (layout == null)
            {
                Debug.LogWarning("[MissionLayout] No layout assigned.");
                return;
            }

            ClearExistingPickupObjects();
            ClearSpawnedObjects();
            occupiedPositions.Clear();

            System.Random rng = CreateRandom(mission);
            mapCenter = GetMapCenter();
            portalPosition = SelectPortalSpawnPoint(rng);
            occupiedPositions.Add(portalPosition);

            PositionPortal(portalPosition);
            Vector3 playerPosition = PositionPlayerNearPortal(portalPosition, rng);
            occupiedPositions.Add(playerPosition);

            PlaceRepairParts(mission, portalPosition, rng);
            PlaceQuestItem(mission, portalPosition, rng);
            PlaceBonusPickups(mission, portalPosition, rng);

            UpdatePortalSafeZone();
        }

        private void PositionPortal(Vector3 position)
        {
            GameObject portal = GameObject.Find(portalName);
            if (portal == null)
            {
                portal = GameObject.Find("=== PORTAL ===");
            }

            if (portal != null)
            {
                Vector3 portalPos = position;
                portalPos.y = portal.transform.position.y;
                portal.transform.position = portalPos;
                Debug.Log("[MissionLayout] Portal positioned in SAFE zone");
            }
        }

        private Vector3 PositionPlayerNearPortal(Vector3 center, System.Random rng)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || layout == null)
            {
                return center;
            }

            Vector3 position = center;
            if (layout.playerSpawnRadius > 0.1f)
            {
                float maxRadius = Mathf.Max(0.1f, layout.safeZoneRadius - layout.mapMargin);
                float radius = Mathf.Min(layout.playerSpawnRadius, maxRadius);
                Vector2 offset = RandomInsideCircle(rng, radius);
                position += new Vector3(offset.x, 0f, offset.y);
            }
            position.y = player.transform.position.y;
            player.transform.position = position;
            return position;
        }

        private void PlaceRepairParts(MissionConfig mission, Vector3 center, System.Random rng)
        {
            if (mission == null)
            {
                return;
            }

            SpawnRepairParts(InventoryItemType.PowerCore, mission.spawnPowerCores, center, rng);
            SpawnRepairParts(InventoryItemType.FuelGel, mission.spawnFuelGels, center, rng);
        }

        private void SpawnRepairParts(InventoryItemType type, int count, Vector3 center, System.Random rng)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnInZone(type, SpawnZoneUtility.ZoneType.Mid, SpawnZoneUtility.ZoneType.Far, center, rng);
            }
        }

        private void PlaceQuestItem(MissionConfig mission, Vector3 center, System.Random rng)
        {
            if (mission == null || mission.questItemReward == null || mission.questItemSpawnCount <= 0)
            {
                return;
            }

            for (int i = 0; i < mission.questItemSpawnCount; i++)
            {
                if (TrySpawnObject(SpawnZoneUtility.ZoneType.Far, center, rng, out Vector3 position))
                {
                    GameObject questObject = CreatePrimitive("QuestItem", PrimitiveType.Cube, position);
                    QuestItemPickup pickup = questObject.AddComponent<QuestItemPickup>();
                    pickup.SetQuestItem(mission.questItemReward);
                    ApplyMinimapIcon(questObject, Color.magenta, 1.6f);
                    spawnedObjects.Add(questObject);
                    Debug.Log($"[MissionLayout] Placed Quest Item at {position}");
                }
            }
        }

        private void PlaceBonusPickups(MissionConfig mission, Vector3 center, System.Random rng)
        {
            if (mission == null || mission.bonusPickupCount <= 0)
            {
                return;
            }

            for (int i = 0; i < mission.bonusPickupCount; i++)
            {
                SpawnZoneUtility.ZoneType zone = PickWeightedZone(mission, rng);
                if (TrySpawnObject(zone, center, rng, out Vector3 position))
                {
                    GameObject bonusObject = CreatePrimitive("BonusPickup", PrimitiveType.Capsule, position);
                    bonusObject.AddComponent<BonusPickup>();
                    ApplyMinimapIcon(bonusObject, Color.cyan, 1.2f);
                    spawnedObjects.Add(bonusObject);
                    Debug.Log($"[MissionLayout] Placed Bonus Pickup at {position}");
                }
            }
        }

        private void SpawnInZone(InventoryItemType type, SpawnZoneUtility.ZoneType zoneA, SpawnZoneUtility.ZoneType zoneB, Vector3 center, System.Random rng)
        {
            if (!TrySpawnObject(zoneA, center, rng, out Vector3 position) && !TrySpawnObject(zoneB, center, rng, out position))
            {
                return;
            }

            GameObject pickup = CreatePrimitive($"RepairPickup_{type}", PrimitiveType.Sphere, position);
            RepairPartPickup pickupComponent = pickup.AddComponent<RepairPartPickup>();
            pickupComponent.SetItemType(type);
            Color color = type == InventoryItemType.FuelGel ? Color.red : Color.yellow;
            ApplyMinimapIcon(pickup, color, 1.4f);
            spawnedObjects.Add(pickup);
            Debug.Log($"[MissionLayout] Placed {type} at {position}");
        }

        private bool TrySpawnObject(SpawnZoneUtility.ZoneType zone, Vector3 center, System.Random rng, out Vector3 position)
        {
            return SpawnZoneUtility.TryGetSpawnPosition(layout, zone, center, mapCenter, minSeparation, occupiedPositions, rng, out position);
        }

        private GameObject CreatePrimitive(string baseName, PrimitiveType type, Vector3 position)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = $"{baseName}_{spawnedObjects.Count + 1}";
            obj.transform.position = new Vector3(position.x, spawnHeight, position.z);
            obj.transform.localScale = Vector3.one * 1.2f;
            obj.transform.SetParent(transform, true);
            obj.AddComponent<SimpleHoverEffect>();
            obj.AddComponent<SimplePulseEffect>();
            obj.AddComponent<SimpleRotateEffect>();
            return obj;
        }

        private void ApplyMinimapIcon(GameObject target, Color color, float size)
        {
            MinimapIcon icon = target.GetComponent<MinimapIcon>();
            if (icon == null)
            {
                icon = target.AddComponent<MinimapIcon>();
            }

            icon.SetIcon(color, size);
        }

        private void ClearSpawnedObjects()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            spawnedObjects.Clear();
        }

        private void ClearExistingPickupObjects()
        {
            RepairPartPickup[] repairPickups = Object.FindObjectsByType<RepairPartPickup>(FindObjectsSortMode.None);
            foreach (RepairPartPickup pickup in repairPickups)
            {
                if (pickup != null)
                {
                    Destroy(pickup.gameObject);
                }
            }

            QuestItemPickup[] questPickups = Object.FindObjectsByType<QuestItemPickup>(FindObjectsSortMode.None);
            foreach (QuestItemPickup pickup in questPickups)
            {
                if (pickup != null)
                {
                    Destroy(pickup.gameObject);
                }
            }

            BonusPickup[] bonusPickups = Object.FindObjectsByType<BonusPickup>(FindObjectsSortMode.None);
            foreach (BonusPickup pickup in bonusPickups)
            {
                if (pickup != null)
                {
                    Destroy(pickup.gameObject);
                }
            }
        }

        private Vector3 GetMapCenter()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                return ground.transform.position;
            }

            return Vector3.zero;
        }

        private Vector3 SelectPortalSpawnPoint(System.Random rng)
        {
            if (layout != null && layout.portalSpawnPoints != null && layout.portalSpawnPoints.Count > 0)
            {
                int index = rng.Next(0, layout.portalSpawnPoints.Count);
                Vector3 selected = layout.portalSpawnPoints[index];
                Debug.Log($"[MissionLayout] Selected portal spawn #{index + 1}");
                return selected;
            }

            GameObject portal = GameObject.Find(portalName);
            if (portal != null)
            {
                return portal.transform.position;
            }

            return Vector3.zero;
        }

        private System.Random CreateRandom(MissionConfig mission)
        {
            if (mission != null && mission.useSpawnSeed)
            {
                return new System.Random(mission.spawnSeed);
            }

            return new System.Random();
        }

        private void UpdatePortalSafeZone()
        {
            PortalSafeZone safeZone = Object.FindFirstObjectByType<PortalSafeZone>();
            if (safeZone == null || layout == null)
            {
                return;
            }

            safeZone.transform.position = portalPosition;
            BoxCollider collider = safeZone.GetComponent<BoxCollider>();
            if (collider != null)
            {
                float size = layout.safeZoneRadius * 2f;
                collider.size = new Vector3(size, collider.size.y, size);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (layout == null)
            {
                return;
            }

            Vector3 center = portalPosition == Vector3.zero ? GetMapCenter() : portalPosition;
            DrawMapBoundaryGizmo(mapCenter == Vector3.zero ? GetMapCenter() : mapCenter, layout.mapRadius, Color.white);
            DrawZoneGizmo(center, layout.safeZoneRadius, new Color(0f, 1f, 0f, 0.6f));
            DrawZoneGizmo(center, layout.midZoneRadius, new Color(1f, 1f, 0f, 0.6f));
            DrawZoneGizmo(center, layout.farZoneRadius, new Color(1f, 0.5f, 0f, 0.6f));

            Gizmos.color = Color.cyan;
            foreach (Vector3 point in occupiedPositions)
            {
                Gizmos.DrawSphere(point + Vector3.up * 0.2f, 0.4f);
            }
        }

        private void DrawZoneGizmo(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            float zScale = layout.shapeType == MissionShapeType.Ellipse ? layout.ellipseZScale : 1f;
            DrawEllipse(center, radius, radius * zScale);
        }

        private void DrawMapBoundaryGizmo(Vector3 center, float radius, Color color)
        {
            Gizmos.color = color;
            float zScale = layout.shapeType == MissionShapeType.Ellipse ? layout.ellipseZScale : 1f;
            DrawEllipse(center, radius, radius * zScale);
        }

        private SpawnZoneUtility.ZoneType PickWeightedZone(MissionConfig mission, System.Random rng)
        {
            float safe = Mathf.Max(0f, mission.bonusWeightSafe);
            float mid = Mathf.Max(0f, mission.bonusWeightMid);
            float far = Mathf.Max(0f, mission.bonusWeightFar);
            float total = safe + mid + far;
            if (total <= 0f)
            {
                return SpawnZoneUtility.ZoneType.Mid;
            }

            float roll = (float)(rng.NextDouble() * total);
            if (roll < safe)
            {
                return SpawnZoneUtility.ZoneType.Safe;
            }
            if (roll < safe + mid)
            {
                return SpawnZoneUtility.ZoneType.Mid;
            }
            return SpawnZoneUtility.ZoneType.Far;
        }

        private Vector2 RandomInsideCircle(System.Random rng, float radius)
        {
            double angle = rng.NextDouble() * Mathf.PI * 2f;
            double distance = Mathf.Sqrt((float)rng.NextDouble()) * radius;
            return new Vector2((float)(Mathf.Cos((float)angle) * distance), (float)(Mathf.Sin((float)angle) * distance));
        }

        private void DrawEllipse(Vector3 center, float radiusX, float radiusZ)
        {
            const int segments = 64;
            Vector3 previous = center + new Vector3(radiusX, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radiusX, 0f, Mathf.Sin(angle) * radiusZ);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
