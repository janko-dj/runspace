using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public enum MissionShapeType
    {
        Circle,
        Ellipse
    }

    [CreateAssetMenu(menuName = "Game/Mission Layout", fileName = "MissionLayout")]
    public class MissionLayout : ScriptableObject
    {
        public string layoutId;
        public MissionShapeType shapeType = MissionShapeType.Circle;
        public float mapRadius = 60f;
        public float mapMargin = 2f;
        public float safeZoneRadius = 12f;
        public float midZoneRadius = 30f;
        public float farZoneRadius = 50f;
        public float ellipseZScale = 0.75f;
        public float playerSpawnRadius = 2f;
        public List<Vector3> portalSpawnPoints = new List<Vector3>();
    }
}
