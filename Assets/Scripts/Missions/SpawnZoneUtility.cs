using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public static class SpawnZoneUtility
    {
        public enum ZoneType
        {
            Safe,
            Mid,
            Far
        }

        public static bool TryGetSpawnPosition(
            MissionLayout layout,
            ZoneType zone,
            Vector3 center,
            Vector3 mapCenter,
            float minSeparation,
            List<Vector3> occupied,
            System.Random rng,
            out Vector3 position)
        {
            position = center;
            if (layout == null)
            {
                return false;
            }

            float mapRadius = Mathf.Max(5f, layout.mapRadius);
            float safeRadius = Mathf.Clamp(layout.safeZoneRadius, 1f, mapRadius);
            float midRadius = Mathf.Clamp(layout.midZoneRadius, safeRadius + 1f, mapRadius);
            float farRadius = Mathf.Clamp(layout.farZoneRadius, midRadius + 1f, mapRadius);

            float innerRadius = 0f;
            float outerRadius = safeRadius;

            switch (zone)
            {
                case ZoneType.Safe:
                    innerRadius = 0f;
                    outerRadius = safeRadius;
                    break;
                case ZoneType.Mid:
                    innerRadius = safeRadius;
                    outerRadius = midRadius;
                    break;
                case ZoneType.Far:
                    innerRadius = midRadius;
                    outerRadius = farRadius;
                    break;
            }

            float zScale = layout.shapeType == MissionShapeType.Ellipse ? 0.75f : 1f;

            for (int i = 0; i < 40; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float radius = Mathf.Lerp(innerRadius, outerRadius, (float)rng.NextDouble());
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius * zScale;

                Vector3 candidate = new Vector3(center.x + x, center.y, center.z + z);
                if (IsInsideMap(layout, mapCenter, candidate, zScale) && IsFarEnough(candidate, occupied, minSeparation))
                {
                    position = candidate;
                    occupied.Add(candidate);
                    return true;
                }
            }

            return false;
        }

        private static bool IsInsideMap(MissionLayout layout, Vector3 mapCenter, Vector3 candidate, float zScale)
        {
            float radius = Mathf.Max(1f, layout.mapRadius);
            Vector3 offset = candidate - mapCenter;
            float x = offset.x;
            float z = offset.z;

            if (layout.shapeType == MissionShapeType.Ellipse)
            {
                float rx = radius;
                float rz = radius * zScale;
                float value = (x * x) / (rx * rx) + (z * z) / (rz * rz);
                return value <= 1f;
            }

            return (x * x + z * z) <= radius * radius;
        }

        private static bool IsFarEnough(Vector3 candidate, List<Vector3> occupied, float minSeparation)
        {
            if (minSeparation <= 0f)
            {
                return true;
            }

            foreach (Vector3 other in occupied)
            {
                if (Vector3.Distance(candidate, other) < minSeparation)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
