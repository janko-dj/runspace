using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

namespace GameCore.Editor
{
    /// <summary>
    /// Editor tool to automatically setup the game scene.
    /// Creates all necessary GameObjects, components, and positions everything correctly.
    /// </summary>
    public class GameSceneSetupTool : EditorWindow
    {
        private bool createManagers = true;
        private bool createPortalArea = true;
        private bool createRepairStations = true;
        private bool createTurretSockets = true;
        private bool createEnvironment = true;
        private bool createPlayer = true;
        private bool positionCamera = true;
        private bool createMissionSelectionMenu = false;
        private bool createMissionBootstrapper = true;
        private bool createMenuCamera = false;
        private bool createMissionSession = false;
        private bool createPlayerProgress = false;
        private bool createDashSystem = true;
        private bool createMissionFlow = true;
        private bool createPickups = true;
        private bool createMinimap = true;
        private bool createEndScreen = true;

        [MenuItem("Game/Scene Setup Tool")]
        public static void ShowWindow()
        {
            GetWindow<GameSceneSetupTool>("Scene Setup Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Game Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("Select what to create:", EditorStyles.label);
            createManagers = EditorGUILayout.Toggle("Managers (Phase, Threat, XP, etc)", createManagers);
            createPortalArea = EditorGUILayout.Toggle("Portal Area (Gate + Zone)", createPortalArea);
            createRepairStations = EditorGUILayout.Toggle("Repair Stations (3 stations)", createRepairStations);
            createTurretSockets = EditorGUILayout.Toggle("Turret Sockets (4 sockets)", createTurretSockets);
            createEnvironment = EditorGUILayout.Toggle("Environment (Ground plane)", createEnvironment);
            createPlayer = EditorGUILayout.Toggle("Player (if none exists)", createPlayer);
            positionCamera = EditorGUILayout.Toggle("Position Main Camera", positionCamera);
            createMissionSelectionMenu = EditorGUILayout.Toggle("Mission Selection Menu (Pre-Game)", createMissionSelectionMenu);
            createMissionBootstrapper = EditorGUILayout.Toggle("Mission Bootstrapper (Mission Scene)", createMissionBootstrapper);
            createMenuCamera = EditorGUILayout.Toggle("Menu Camera (Pre-Game)", createMenuCamera);
            createMissionSession = EditorGUILayout.Toggle("Mission Session (Menu)", createMissionSession);
            createPlayerProgress = EditorGUILayout.Toggle("Player Progress (Menu)", createPlayerProgress);
            createDashSystem = EditorGUILayout.Toggle("Dash System (Player)", createDashSystem);
            createMissionFlow = EditorGUILayout.Toggle("Mission Flow Controller", createMissionFlow);
            createPickups = EditorGUILayout.Toggle("Repair Part Pickups", createPickups);
            createMinimap = EditorGUILayout.Toggle("Minimap", createMinimap);
            createEndScreen = EditorGUILayout.Toggle("End Screen (Mission Scene)", createEndScreen);

            GUILayout.Space(20);

            if (GUILayout.Button("Setup Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clear All (Delete Everything)"))
            {
                if (EditorUtility.DisplayDialog("Clear Scene",
                    "This will delete ALL GameObjects in the scene. Are you sure?",
                    "Yes, Clear", "Cancel"))
                {
                    ClearScene();
                }
            }

            GUILayout.Space(20);
            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Managers Only"))
            {
                CreateManagers();
            }

            if (GUILayout.Button("Create Portal Area Only"))
            {
                CreatePortalArea();
                CreateRepairStations();
            }

            if (GUILayout.Button("Setup Pre-Game Scene (Portal)"))
            {
                EnsurePreGameSceneOpen();
                SetupPreGameScene();
                EnsureSceneInBuildSettings("Assets/Scenes/PreGame.unity");
            }

            if (GUILayout.Button("Setup Mission Scene (Gameplay)"))
            {
                SetupMissionScene();
            }
        }

        private void SetupScene()
        {
            Debug.Log("=== SCENE SETUP STARTED ===");

            // Ensure required tags exist
            EnsureTagExists("Enemy");

            if (createManagers)
                CreateManagers();

            // Create player FIRST so EncounterSpawner can find it as target
            if (createPlayer)
                CreatePlayer();

            if (createEnvironment)
                CreateEnvironment();

            if (createPortalArea)
                CreatePortalArea();

            if (createRepairStations)
                CreateRepairStations();

            if (createTurretSockets)
                CreateTurretSockets();

            if (positionCamera)
                PositionCamera();

            if (createMissionSelectionMenu || ShouldAutoCreateMissionMenu())
                CreateMissionSelectionMenu(GetOrCreateMission1Config("Main"), GetOrCreateMission2Config("Main"));

            if (createMissionBootstrapper)
                CreateMissionBootstrapper("PreGame");

            if (createMenuCamera || ShouldAutoCreateMissionMenu())
                CreateMenuCamera();

            if (createMissionSession || ShouldAutoCreateMissionMenu())
                CreateMissionSession();

            if (createPlayerProgress || ShouldAutoCreateMissionMenu())
                CreatePlayerProgress();

            if (createMissionFlow)
                CreateMissionFlowController();

            if (createPickups)
                CreateRepairPartPickups();

            if (createMissionBootstrapper)
                CreateMissionLayoutController();

            if (createMinimap)
                CreateMinimap();

            if (createEndScreen)
                CreateEndScreen();

            DisableDebugTestingObjects();

            Debug.Log("=== SCENE SETUP COMPLETE ===");
            Debug.Log("Press PLAY, then press '4' to enter Prep phase and test repair stations!");

            // List what was created
            string summary = "Objects Created:\n";
            if (createManagers) summary += "✓ Managers (RunPhase, Threat, XP, DP, etc)\n";
            if (createEnvironment) summary += "✓ Environment (Ground, Light, Spawner)\n✓ Enemy Prefab (ChaserEnemy)\n";
            if (createPortalArea) summary += "✓ Portal Area (Gate + Zone)\n";
            if (createRepairStations) summary += "✓ Repair Stations (3 cylinders)\n";
            if (createTurretSockets) summary += "✓ Turret Sockets (4 sockets)\n✓ Turret Prefab (BasicTurret)\n";
            if (createPlayer) summary += "✓ Player (capsule)\n";
            if (positionCamera) summary += "✓ Camera Follow (third-person)\n";
            if (createMissionSelectionMenu) summary += "✓ Mission Selection Menu\n✓ Mission Config (Mission 1)\n";
            if (createMissionBootstrapper) summary += "✓ Mission Bootstrapper\n";
            if (createMenuCamera) summary += "✓ Menu Camera\n";
            if (createMissionSession) summary += "✓ Mission Session\n";
            if (createPlayerProgress) summary += "✓ Player Progress\n";
            if (createDashSystem) summary += "✓ Dash System\n";
            if (createMissionFlow) summary += "✓ Mission Flow Controller\n";
            if (createPickups) summary += "✓ Repair Part Pickups (Spawner)\n";
            if (createMinimap) summary += "✓ Minimap (camera + UI + icons)\n";
            if (createEndScreen) summary += "✓ End Screen\n";
            if (createMissionBootstrapper) summary += "✓ Mission Layout Controller\n";

            Debug.Log(summary);

            EditorUtility.DisplayDialog("Scene Setup Complete",
                "Scene has been set up successfully!\n\n" +
                summary + "\n" +
                "Controls:\n" +
                "• WASD - Move player\n" +
                "• E - Repair (hold for 3 seconds)\n" +
                "• T - Place turret (at socket)\n" +
                "• 4 - Enter Prep phase (generates problems)\n" +
                "• 5 - Enter FinalStand (repairs allowed, enemies spawn)\n\n" +
                "To test turrets:\n" +
                "1. Press Play\n" +
                "2. Use debug overlay to convert salvage to DP\n" +
                "3. Press '4' to enter Prep\n" +
                "4. Move to green socket cubes, press 'T' to place turrets\n" +
                "5. Press '5' to enter FinalStand - turrets auto-shoot enemies!\n\n" +
                "Camera follows player automatically!",
                "OK");
        }

        private void SetupPreGameScene()
        {
            createManagers = false;
            createPortalArea = false;
            createRepairStations = false;
            createTurretSockets = false;
            createEnvironment = false;
            createPlayer = false;
            positionCamera = false;
            createMissionSelectionMenu = true;
            createMissionBootstrapper = false;
            createMenuCamera = true;
            createMissionSession = true;
            createPlayerProgress = true;
            createDashSystem = false;
            createMissionFlow = false;
            createPickups = false;
            createMinimap = false;
            createEndScreen = false;

            SetupScene();
        }

        private void SetupMissionScene()
        {
            createManagers = true;
            createPortalArea = true;
            createRepairStations = true;
            createTurretSockets = true;
            createEnvironment = true;
            createPlayer = true;
            positionCamera = true;
            createMissionSelectionMenu = false;
            createMissionBootstrapper = true;
            createMenuCamera = false;
            createMissionSession = false;
            createPlayerProgress = false;
            createDashSystem = true;
            createMissionFlow = true;
            createPickups = true;
            createMinimap = true;
            createEndScreen = true;

            RemoveMissionMenuObjects();
            RemoveLegacyMissionLoader();
            SetupScene();
            UpdateMissionSceneName(GetActiveSceneName());
            EnsureSceneInBuildSettings("Assets/Scenes/PreGame.unity");
            EnsureSceneInBuildSettings(EditorSceneManager.GetActiveScene().path);
        }

        private void CreateManagers()
        {
            Debug.Log("[Setup] Creating Manager objects...");

            // Create parent for organization
            GameObject managersParent = CreateOrFind("=== MANAGERS ===");

            // RunPhaseController
            GameObject phaseManager = CreateManager("RunPhaseController", managersParent, typeof(RunPhaseController));
            if (phaseManager != null)
            {
                RunPhaseController controller = phaseManager.GetComponent<RunPhaseController>();
                if (controller != null)
                {
                    SerializedObject so = new SerializedObject(controller);
                    so.FindProperty("enableKeyboardTriggers").boolValue = false;
                    so.FindProperty("autoTransition").boolValue = false;
                    so.ApplyModifiedProperties();
                }
            }

            // PressureSystem
            CreateManager("PressureSystem", managersParent, typeof(PressureSystem));

            // RunXPSystem
            CreateManager("RunXPSystem", managersParent, typeof(RunXPSystem));

            // TeamLevelUpSystem
            CreateManager("TeamLevelUpSystem", managersParent, typeof(TeamLevelUpSystem));

            // SystemIssueSystem
            CreateManager("SystemIssueSystem", managersParent, typeof(SystemIssueSystem));

            // SharedInventorySystem
            CreateManager("SharedInventorySystem", managersParent, typeof(SharedInventorySystem));

            // DeploymentPointsSystem
            CreateManager("DeploymentPointsSystem", managersParent, typeof(DeploymentPointsSystem));

            // RunCompletionSystem
            GameObject victoryManager = CreateManager("VictoryManager", managersParent, typeof(RunCompletionSystem));

            Debug.Log("[Setup] Managers created!");
        }

        private void CreateEnvironment()
        {
            Debug.Log("[Setup] Creating Environment...");

            GameObject envParent = CreateOrFind("=== ENVIRONMENT ===");

            // Ground Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0, 0, 0);
            ground.transform.localScale = new Vector3(12, 1, 12);
            ground.transform.parent = envParent.transform;

            // Note: Material left as default white - manually set color in Inspector if needed

            // Create Enemy Prefab if it doesn't exist
            GameObject enemyPrefab = CreateEnemyPrefab();

            // Enemy Spawner
            GameObject spawner = new GameObject("EncounterSpawner");
            spawner.transform.position = new Vector3(0, 0, 20);
            spawner.transform.parent = envParent.transform;
            EncounterSpawner enemySpawner = spawner.AddComponent<EncounterSpawner>();

            // Assign enemy prefab to spawner
            if (enemyPrefab != null)
            {
                SerializedObject so = new SerializedObject(enemySpawner);
                so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;

                // Find player to use as target
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    so.FindProperty("enemyTarget").objectReferenceValue = player.transform;
                }

                so.ApplyModifiedProperties();
                Debug.Log("[Setup] EncounterSpawner configured with prefab and target");
            }

            // Create Directional Light (critical for visibility!)
            Light existingLight = Object.FindFirstObjectByType<Light>();
            if (existingLight == null)
            {
                GameObject lightObj = new GameObject("Directional Light");
                lightObj.transform.parent = envParent.transform;
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.transform.rotation = Quaternion.Euler(50, -30, 0);
                light.intensity = 1f;
                light.color = Color.white;
                Debug.Log("[Setup] Created Directional Light");
            }
            else
            {
                Debug.Log("[Setup] Directional Light already exists");
            }

            Debug.Log("[Setup] Environment created!");

            CreateArenaBounds(envParent);
        }

        private void CreateArenaBounds(GameObject parent)
        {
            GameObject boundsParent = CreateOrFind("ArenaBounds");
            boundsParent.transform.parent = parent.transform;

            float halfSize = 60f;
            float wallHeight = 5f;
            float wallThickness = 1f;

            CreateWall("Wall_North", new Vector3(0, wallHeight / 2f, halfSize), new Vector3(halfSize * 2f, wallHeight, wallThickness), boundsParent);
            CreateWall("Wall_South", new Vector3(0, wallHeight / 2f, -halfSize), new Vector3(halfSize * 2f, wallHeight, wallThickness), boundsParent);
            CreateWall("Wall_East", new Vector3(halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, halfSize * 2f), boundsParent);
            CreateWall("Wall_West", new Vector3(-halfSize, wallHeight / 2f, 0), new Vector3(wallThickness, wallHeight, halfSize * 2f), boundsParent);
        }

        private void CreateWall(string name, Vector3 position, Vector3 scale, GameObject parent)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.transform.parent = parent.transform;
        }

        private void CreatePortalArea()
        {
            Debug.Log("[Setup] Creating Portal Area...");

            GameObject shipParent = CreateOrFind("=== PORTAL ===");

            // Portal Gate (visual) - Remove collider so players can walk through
            GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hull.name = "PortalGate";
            hull.transform.position = new Vector3(0, 0.5f, 0);
            hull.transform.localScale = new Vector3(6, 2, 6);
            hull.transform.parent = shipParent.transform;

            // Remove collider so player can walk into portal zone
            Collider hullCollider = hull.GetComponent<Collider>();
            if (hullCollider != null)
            {
                DestroyImmediate(hullCollider);
                Debug.Log("[Setup] Removed portal gate collider - players can walk inside");
            }

            // Note: Material left as default white - manually set color in Inspector if needed

            // Portal Zone (trigger for victory)
            GameObject zone = new GameObject("PortalZone");
            zone.transform.position = new Vector3(0, 0, 0);
            zone.transform.parent = shipParent.transform;

            // Add BoxCollider first (before BaseZone component)
            BoxCollider zoneCollider = zone.AddComponent<BoxCollider>();
            zoneCollider.size = new Vector3(12, 5, 12);
            zoneCollider.isTrigger = true;

            // Then add BaseZone component
            BaseZone shipZone = zone.AddComponent<BaseZone>();

            // Link RunCompletionSystem to BaseZone
            RunCompletionSystem victoryCondition = Object.FindFirstObjectByType<RunCompletionSystem>();
            if (victoryCondition != null)
            {
                SerializedObject so = new SerializedObject(victoryCondition);
                so.FindProperty("shipZone").objectReferenceValue = shipZone;
                so.ApplyModifiedProperties();
                Debug.Log("[Setup] RunCompletionSystem linked to BaseZone");
            }

            Debug.Log("[Setup] Portal Area created!");

            CreatePortalSafeZone(shipParent);
        }

        private void CreatePortalSafeZone(GameObject parent)
        {
            GameObject safeZone = new GameObject("PortalSafeZone");
            safeZone.transform.position = new Vector3(0, 0, 0);
            safeZone.transform.parent = parent.transform;

            BoxCollider safeCollider = safeZone.AddComponent<BoxCollider>();
            safeCollider.size = new Vector3(10, 3, 10);
            safeCollider.isTrigger = true;

            PortalSafeZone safeZoneComponent = safeZone.AddComponent<PortalSafeZone>();
            MissionFlowController flowController = Object.FindFirstObjectByType<MissionFlowController>();
            if (flowController != null)
            {
                SerializedObject so = new SerializedObject(safeZoneComponent);
                so.FindProperty("flowController").objectReferenceValue = flowController;
                so.ApplyModifiedProperties();
            }
        }

        private void CreateRepairStations()
        {
            Debug.Log("[Setup] Creating Repair Stations...");

            GameObject shipParent = GameObject.Find("=== PORTAL ===");
            if (shipParent == null)
                shipParent = CreateOrFind("=== PORTAL ===");

            // Station 1 - Power (Yellow, East)
            CreateRepairStation("RepairStation_Power", new Vector3(5, 1, 0),
                SystemIssueType.PowerFailure, Color.yellow, shipParent);

            // Station 2 - Hull (Red, West)
            CreateRepairStation("RepairStation_Hull", new Vector3(-5, 1, 0),
                SystemIssueType.HullBreach, Color.red, shipParent);

            // Station 3 - Navigation (Blue, North)
            CreateRepairStation("RepairStation_Nav", new Vector3(0, 1, 5),
                SystemIssueType.NavigationError, Color.cyan, shipParent);

            Debug.Log("[Setup] Repair Stations created!");
        }

        private void CreateRepairStation(string name, Vector3 position, SystemIssueType problemType, Color color, GameObject parent)
        {
            GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            station.name = name;
            station.transform.position = position;
            station.transform.localScale = new Vector3(1, 2, 1);
            station.transform.parent = parent.transform;

            // Note: Material left as default white - manually color in Inspector using the 'color' parameter

            // Remove default collider and add trigger
            DestroyImmediate(station.GetComponent<Collider>());
            SphereCollider trigger = station.AddComponent<SphereCollider>();
            trigger.radius = 2f;
            trigger.isTrigger = true;

            // Add IssueRepairStation component
            IssueRepairStation repairStation = station.AddComponent<IssueRepairStation>();

            // Configure IssueRepairStation using SerializedObject
            SerializedObject so = new SerializedObject(repairStation);
            so.FindProperty("problemType").enumValueIndex = (int)problemType;
            so.FindProperty("repairDuration").floatValue = 3f;
            so.FindProperty("interactionKey").intValue = (int)KeyCode.E;  // Use intValue for KeyCode
            so.FindProperty("playerTag").stringValue = "Player";
            so.FindProperty("showDebugOverlay").boolValue = true;
            so.FindProperty("logRepairEvents").boolValue = true;
            so.ApplyModifiedProperties();

            Debug.Log($"[Setup] Created {name} at {position}");
        }

        private void CreateMissionSelectionMenu(MissionConfig missionConfig, MissionConfig missionConfig2)
        {
            GameObject menuObject = GameObject.Find("MissionSelectionMenu");
            if (menuObject == null)
            {
                menuObject = new GameObject("MissionSelectionMenu");
            }

            MissionSelectionMenu menu = menuObject.GetComponent<MissionSelectionMenu>();
            if (menu == null)
            {
                menu = menuObject.AddComponent<MissionSelectionMenu>();
            }

            if (missionConfig != null)
            {
                SerializedObject so = new SerializedObject(menu);
                so.FindProperty("mission1").objectReferenceValue = missionConfig;
                so.FindProperty("mission2").objectReferenceValue = missionConfig2;
                so.ApplyModifiedProperties();
            }

            Debug.Log("[Setup] MissionSelectionMenu created");
        }

        private void CreateMenuCamera()
        {
            Camera existingCamera = Object.FindFirstObjectByType<Camera>();
            if (existingCamera != null)
            {
                Debug.Log("[Setup] Menu camera already exists");
                return;
            }

            GameObject cameraObject = new GameObject("MenuCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 5f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            Debug.Log("[Setup] Menu camera created");
        }

        private void CreateMissionBootstrapper(string menuSceneName)
        {
            GameObject bootstrapperObject = GameObject.Find("MissionBootstrapper");
            if (bootstrapperObject == null)
            {
                bootstrapperObject = new GameObject("MissionBootstrapper");
            }

            MissionBootstrapper bootstrapper = bootstrapperObject.GetComponent<MissionBootstrapper>();
            if (bootstrapper == null)
            {
                bootstrapper = bootstrapperObject.AddComponent<MissionBootstrapper>();
            }

            SerializedObject so = new SerializedObject(bootstrapper);
            so.FindProperty("menuSceneName").stringValue = string.IsNullOrWhiteSpace(menuSceneName) ? "PreGame" : menuSceneName;
            so.ApplyModifiedProperties();

            Debug.Log("[Setup] MissionBootstrapper created");
        }

        private MissionConfig GetOrCreateMission1Config(string defaultSceneName)
        {
            const string missionsFolder = "Assets/Missions";
            const string assetPath = "Assets/Missions/Mission1.asset";

            if (!AssetDatabase.IsValidFolder(missionsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Missions");
            }

            MissionConfig config = AssetDatabase.LoadAssetAtPath<MissionConfig>(assetPath);
            if (config != null)
            {
                ApplyMissionDefaults(config, "mission_1", "Mission 1", 0, 1f, defaultSceneName, 2, 2, 2, 2, 25f, 60f, GetOrCreateQuestItem(), null, GetOrCreateMission1Layout());
                return config;
            }

            config = ScriptableObject.CreateInstance<MissionConfig>();
            config.missionId = "mission_1";
            config.displayName = "Mission 1";
            config.startingThreat = 0;
            config.threatGrowthMultiplier = 1f;
            config.sceneName = string.IsNullOrWhiteSpace(defaultSceneName)
                ? "Main"
                : defaultSceneName;
            config.questItemReward = GetOrCreateQuestItem();
            config.requiredQuestItem = null;
            config.requiredPowerCores = 2;
            config.requiredFuelGels = 2;
            config.spawnPowerCores = 2;
            config.spawnFuelGels = 2;
            config.pickupMinDistance = 25f;
            config.pickupMaxDistance = 60f;
            config.layout = GetOrCreateMission1Layout();
            config.questItemSpawnCount = 1;
            config.bonusPickupCount = 2;
            config.useSpawnSeed = false;

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Setup] Created MissionConfig at {assetPath}");
            return config;
        }

        private MissionConfig GetOrCreateMission2Config(string defaultSceneName)
        {
            const string missionsFolder = "Assets/Missions";
            const string assetPath = "Assets/Missions/Mission2.asset";

            if (!AssetDatabase.IsValidFolder(missionsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Missions");
            }

            MissionConfig config = AssetDatabase.LoadAssetAtPath<MissionConfig>(assetPath);
            if (config != null)
            {
                ApplyMissionDefaults(config, "mission_2", "Mission 2", 8, 1.2f, defaultSceneName, 3, 3, 3, 3, 30f, 70f, null, GetOrCreateQuestItem(), GetOrCreateMission2Layout());
                return config;
            }

            QuestItem questItem = GetOrCreateQuestItem();

            config = ScriptableObject.CreateInstance<MissionConfig>();
            config.missionId = "mission_2";
            config.displayName = "Mission 2";
            config.startingThreat = 8;
            config.threatGrowthMultiplier = 1.2f;
            config.sceneName = string.IsNullOrWhiteSpace(defaultSceneName)
                ? "Main"
                : defaultSceneName;
            config.questItemReward = null;
            config.requiredQuestItem = questItem;
            config.requiredPowerCores = 3;
            config.requiredFuelGels = 3;
            config.spawnPowerCores = 3;
            config.spawnFuelGels = 3;
            config.pickupMinDistance = 30f;
            config.pickupMaxDistance = 70f;
            config.layout = GetOrCreateMission2Layout();
            config.questItemSpawnCount = 1;
            config.bonusPickupCount = 3;
            config.useSpawnSeed = false;

            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Setup] Created MissionConfig at {assetPath}");
            return config;
        }

        private QuestItem GetOrCreateQuestItem()
        {
            const string folderPath = "Assets/Progression";
            const string assetPath = "Assets/Progression/QuestItem_Mission1.asset";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Progression");
            }

            QuestItem item = AssetDatabase.LoadAssetAtPath<QuestItem>(assetPath);
            if (item != null)
            {
                return item;
            }

            item = ScriptableObject.CreateInstance<QuestItem>();
            item.questItemId = "quest_item_1";
            item.displayName = "Portal Core";
            item.description = "A recovered core that stabilizes the next portal.";
            item.unlocksMissionId = "mission_2";

            AssetDatabase.CreateAsset(item, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Created QuestItem at {assetPath}");
            return item;
        }

        private MissionLayout GetOrCreateMission1Layout()
        {
            const string folderPath = "Assets/Missions";
            const string assetPath = "Assets/Missions/Layout_Mission1.asset";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Missions");
            }

            MissionLayout layout = AssetDatabase.LoadAssetAtPath<MissionLayout>(assetPath);
            if (layout != null)
            {
                EnsureLayoutSpawnPoints(layout, new System.Collections.Generic.List<Vector3>
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(12f, 0f, 0f),
                    new Vector3(-12f, 0f, 0f),
                    new Vector3(0f, 0f, 12f)
                });
                return layout;
            }

            layout = ScriptableObject.CreateInstance<MissionLayout>();
            layout.layoutId = "layout_mission_1";
            layout.shapeType = MissionShapeType.Circle;
            layout.mapRadius = 60f;
            layout.safeZoneRadius = 12f;
            layout.midZoneRadius = 30f;
            layout.farZoneRadius = 50f;
            layout.portalSpawnPoints = new System.Collections.Generic.List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(12f, 0f, 0f),
                new Vector3(-12f, 0f, 0f),
                new Vector3(0f, 0f, 12f)
            };

            AssetDatabase.CreateAsset(layout, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Created MissionLayout at {assetPath}");
            return layout;
        }

        private MissionLayout GetOrCreateMission2Layout()
        {
            const string folderPath = "Assets/Missions";
            const string assetPath = "Assets/Missions/Layout_Mission2.asset";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Missions");
            }

            MissionLayout layout = AssetDatabase.LoadAssetAtPath<MissionLayout>(assetPath);
            if (layout != null)
            {
                EnsureLayoutSpawnPoints(layout, new System.Collections.Generic.List<Vector3>
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(14f, 0f, 6f),
                    new Vector3(-14f, 0f, -6f),
                    new Vector3(0f, 0f, -14f)
                });
                return layout;
            }

            layout = ScriptableObject.CreateInstance<MissionLayout>();
            layout.layoutId = "layout_mission_2";
            layout.shapeType = MissionShapeType.Ellipse;
            layout.mapRadius = 65f;
            layout.safeZoneRadius = 14f;
            layout.midZoneRadius = 34f;
            layout.farZoneRadius = 55f;
            layout.portalSpawnPoints = new System.Collections.Generic.List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(14f, 0f, 6f),
                new Vector3(-14f, 0f, -6f),
                new Vector3(0f, 0f, -14f)
            };

            AssetDatabase.CreateAsset(layout, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Setup] Created MissionLayout at {assetPath}");
            return layout;
        }

        private void EnsureLayoutSpawnPoints(MissionLayout layout, System.Collections.Generic.List<Vector3> spawnPoints)
        {
            if (layout == null)
            {
                return;
            }

            if (layout.portalSpawnPoints == null || layout.portalSpawnPoints.Count == 0)
            {
                layout.portalSpawnPoints = spawnPoints;
                EditorUtility.SetDirty(layout);
                AssetDatabase.SaveAssets();
            }
        }

        private void ApplyMissionDefaults(
            MissionConfig config,
            string missionId,
            string displayName,
            int startingThreat,
            float threatGrowthMultiplier,
            string defaultSceneName,
            int requiredPowerCores,
            int requiredFuelGels,
            int spawnPowerCores,
            int spawnFuelGels,
            float pickupMinDistance,
            float pickupMaxDistance,
            QuestItem reward,
            QuestItem required,
            MissionLayout layout)
        {
            bool dirty = false;

            if (string.IsNullOrWhiteSpace(config.missionId))
            {
                config.missionId = missionId;
                dirty = true;
            }

            if (string.IsNullOrWhiteSpace(config.displayName))
            {
                config.displayName = displayName;
                dirty = true;
            }

            if (config.startingThreat == 0 && startingThreat != 0)
            {
                config.startingThreat = startingThreat;
                dirty = true;
            }

            if (config.threatGrowthMultiplier == 0f)
            {
                config.threatGrowthMultiplier = threatGrowthMultiplier;
                dirty = true;
            }

            if (string.IsNullOrWhiteSpace(config.sceneName))
            {
                config.sceneName = string.IsNullOrWhiteSpace(defaultSceneName) ? "Main" : defaultSceneName;
                dirty = true;
            }

            if (config.requiredPowerCores <= 0)
            {
                config.requiredPowerCores = requiredPowerCores;
                dirty = true;
            }

            if (config.requiredFuelGels <= 0)
            {
                config.requiredFuelGels = requiredFuelGels;
                dirty = true;
            }

            if (config.spawnPowerCores <= 0)
            {
                config.spawnPowerCores = spawnPowerCores;
                dirty = true;
            }

            if (config.spawnFuelGels <= 0)
            {
                config.spawnFuelGels = spawnFuelGels;
                dirty = true;
            }

            if (config.pickupMinDistance <= 0f)
            {
                config.pickupMinDistance = pickupMinDistance;
                dirty = true;
            }

            if (config.pickupMaxDistance <= 0f)
            {
                config.pickupMaxDistance = pickupMaxDistance;
                dirty = true;
            }

            if ((config.layout == null || config.layout.layoutId == "layout_default") && layout != null)
            {
                config.layout = layout;
                dirty = true;
            }

            if (config.questItemSpawnCount <= 0)
            {
                config.questItemSpawnCount = 1;
                dirty = true;
            }

            if (config.bonusPickupCount <= 0)
            {
                config.bonusPickupCount = 2;
                dirty = true;
            }

            if (reward != null && config.questItemReward == null)
            {
                config.questItemReward = reward;
                dirty = true;
            }

            if (required != null && config.requiredQuestItem == null)
            {
                config.requiredQuestItem = required;
                dirty = true;
            }

            if (dirty)
            {
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }

        private void CreateMissionSession()
        {
            GameObject sessionObject = GameObject.Find("MissionSession");
            if (sessionObject == null)
            {
                sessionObject = new GameObject("MissionSession");
                sessionObject.AddComponent<MissionSession>();
            }

            Debug.Log("[Setup] MissionSession created");
        }

        private void CreatePlayerProgress()
        {
            GameObject progressObject = GameObject.Find("PlayerProgress");
            if (progressObject == null)
            {
                progressObject = new GameObject("PlayerProgress");
                progressObject.AddComponent<PlayerProgress>();
            }

            Debug.Log("[Setup] PlayerProgress created");
        }

        private string GetActiveSceneName()
        {
            return EditorSceneManager.GetActiveScene().name;
        }

        private void UpdateMissionSceneName(string sceneName)
        {
            MissionConfig config = GetOrCreateMission1Config(sceneName);
            if (config.sceneName != sceneName)
            {
                config.sceneName = sceneName;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }

            MissionConfig config2 = GetOrCreateMission2Config(sceneName);
            if (config2.sceneName != sceneName)
            {
                config2.sceneName = sceneName;
                EditorUtility.SetDirty(config2);
                AssetDatabase.SaveAssets();
            }
        }

        private bool ShouldAutoCreateMissionMenu()
        {
            return Object.FindFirstObjectByType<RunPhaseController>() == null;
        }

        private void EnsurePreGameSceneOpen()
        {
            const string scenesFolder = "Assets/Scenes";
            const string preGameScenePath = "Assets/Scenes/PreGame.unity";

            if (!AssetDatabase.IsValidFolder(scenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!System.IO.File.Exists(preGameScenePath))
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), preGameScenePath);
            }

            EditorSceneManager.OpenScene(preGameScenePath);
            ClearSceneRootObjects();
        }

        private void EnsureSceneInBuildSettings(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                return;
            }

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool exists = scenes.Exists(scene => scene.path == scenePath);
            if (exists)
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private void ClearSceneRootObjects()
        {
            var scene = EditorSceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                DestroyImmediate(root);
            }
        }

        private void RemoveMissionMenuObjects()
        {
            GameObject menu = GameObject.Find("MissionSelectionMenu");
            if (menu != null)
            {
                DestroyImmediate(menu);
            }

            GameObject menuCamera = GameObject.Find("MenuCamera");
            if (menuCamera != null)
            {
                DestroyImmediate(menuCamera);
            }
        }

        private void CreateEndScreen()
        {
            GameObject endScreen = GameObject.Find("EndScreenController");
            if (endScreen == null)
            {
                endScreen = new GameObject("EndScreenController");
                endScreen.AddComponent<EndScreenController>();
            }
        }

        private void CreateMissionLayoutController()
        {
            GameObject controllerObject = GameObject.Find("MissionLayoutController");
            if (controllerObject == null)
            {
                controllerObject = new GameObject("MissionLayoutController");
            }

            if (controllerObject.GetComponent<MissionLayoutController>() == null)
            {
                controllerObject.AddComponent<MissionLayoutController>();
            }
        }

        private void DisableDebugTestingObjects()
        {
            RemoveComponentByTypeName("GameCore.InventoryDebugInput, Assembly-CSharp");
            RemoveComponentByTypeName("GameCore.InventorySystemExample, Assembly-CSharp");
            RemoveComponentByTypeName("GameCore.PressureSystemExample, Assembly-CSharp");
            RemoveComponentByTypeName("GameCore.RunXPSystemExample, Assembly-CSharp");
            RemoveComponentByTypeName("GameCore.EnemyTestHelper, Assembly-CSharp");
            RemoveComponentByTypeName("GameCore.PlayerTestHelper, Assembly-CSharp");
        }

        private void RemoveComponentByTypeName(string typeName)
        {
            System.Type componentType = System.Type.GetType(typeName);
            if (componentType == null)
            {
                return;
            }

            Object[] components = Object.FindObjectsByType(componentType, FindObjectsSortMode.None);
            foreach (Object component in components)
            {
                Component castComponent = component as Component;
                if (castComponent != null)
                {
                    DestroyImmediate(castComponent);
                }
            }
        }

        private void RemoveLegacyMissionLoader()
        {
            GameObject loader = GameObject.Find("MissionLoader");
            if (loader != null)
            {
                DestroyImmediate(loader);
            }
        }

        private void CreatePlayer()
        {
            // Check if player already exists
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                EnsurePlayerComponents(existingPlayer);
                Debug.Log("[Setup] Player already exists, ensured components");
                return;
            }

            Debug.Log("[Setup] Creating Player...");

            GameObject playerParent = CreateOrFind("=== PLAYER ===");

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1, -2);
            player.transform.parent = playerParent.transform;

            // Remove default collider, add CharacterController
            DestroyImmediate(player.GetComponent<Collider>());
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.radius = 0.5f;
            controller.height = 2f;
            // NOTE: CharacterController has collision built-in, don't add Rigidbody!

            EnsurePlayerComponents(player);

            // Note: Material left as default white - manually set color in Inspector if needed

            Debug.Log("[Setup] Player created at (0, 1, -10)");
        }

        private void EnsurePlayerComponents(GameObject player)
        {
            if (player == null)
            {
                return;
            }

            AddComponentIfMissing(player, "GameCore.CharacterMover, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.CharacterHealth, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.AutoAttackWeapon, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.UpgradeChoiceHandler, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.PlayerAbilityController, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.PlayerLookController, Assembly-CSharp");
            AddComponentIfMissing(player, "GameCore.PlayerCharacter, Assembly-CSharp");

            CharacterMover mover = player.GetComponent<CharacterMover>();
            if (mover != null)
            {
                SerializedObject so = new SerializedObject(mover);
                so.FindProperty("rotateWithMovement").boolValue = true;
                so.FindProperty("useCameraRelativeMovement").boolValue = true;
                so.ApplyModifiedProperties();
            }

            PlayerLookController lookController = player.GetComponent<PlayerLookController>();
            if (lookController != null)
            {
                SerializedObject lookSo = new SerializedObject(lookController);
                lookSo.FindProperty("requireRightMouse").boolValue = false;
                lookSo.FindProperty("aimCamera").objectReferenceValue = Camera.main;
                lookSo.FindProperty("ensureCursorVisible").boolValue = true;
                lookSo.ApplyModifiedProperties();
            }

            if (createDashSystem)
            {
                AddComponentIfMissing(player, "GameCore.PlayerDashController, Assembly-CSharp");
            }
        }

        private void AddComponentIfMissing(GameObject player, string typeName)
        {
            System.Type componentType = System.Type.GetType(typeName);
            if (componentType == null)
            {
                Debug.LogWarning($"[Setup] Component type not found: {typeName}");
                return;
            }

            if (player.GetComponent(componentType) == null)
            {
                player.AddComponent(componentType);
            }
        }

        private void PositionCamera()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = Object.FindFirstObjectByType<Camera>();
                if (mainCam == null)
                {
                    GameObject cameraObject = new GameObject("Main Camera");
                    mainCam = cameraObject.AddComponent<Camera>();
                    cameraObject.tag = "MainCamera";
                    Debug.Log("[Setup] Created Main Camera");
                }
                else
                {
                    mainCam.tag = "MainCamera";
                    Debug.Log("[Setup] Assigned MainCamera tag to existing camera");
                }
            }

            Debug.Log("[Setup] Positioning Main Camera...");

            // Use perspective for natural third-person view
            mainCam.orthographic = false;
            mainCam.fieldOfView = 60;
            int minimapLayer = LayerMask.NameToLayer("Minimap");
            if (minimapLayer >= 0)
            {
                mainCam.cullingMask &= ~(1 << minimapLayer);
            }

            // Add FollowCamera component if it doesn't exist
            FollowCamera cameraFollow = mainCam.GetComponent<FollowCamera>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCam.gameObject.AddComponent<FollowCamera>();
                Debug.Log("[Setup] Added FollowCamera component to camera");
            }

            CameraOrbitController orbitController = mainCam.GetComponent<CameraOrbitController>();
            if (orbitController == null)
            {
                orbitController = mainCam.gameObject.AddComponent<CameraOrbitController>();
                Debug.Log("[Setup] Added CameraOrbitController to camera");
            }
            orbitController.enabled = true;

            PlayerLookController cameraLook = mainCam.GetComponent<PlayerLookController>();
            if (cameraLook != null)
            {
                DestroyImmediate(cameraLook);
            }

            // Configure camera follow using SerializedObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SerializedObject so = new SerializedObject(cameraFollow);
                so.FindProperty("target").objectReferenceValue = player.transform;
                so.FindProperty("offset").vector3Value = new Vector3(0, 18, -6);
                so.FindProperty("smoothSpeed").floatValue = 5f;
                so.FindProperty("lookAtTarget").boolValue = true;
                so.FindProperty("autoFindPlayer").boolValue = true;
                so.ApplyModifiedProperties();

                SerializedObject orbitSo = new SerializedObject(orbitController);
                orbitSo.FindProperty("lockCursor").boolValue = true;
                orbitSo.FindProperty("requireRightMouse").boolValue = false;
                orbitSo.ApplyModifiedProperties();

                // Set initial position
                mainCam.transform.position = player.transform.position + new Vector3(0, 18, -6);
                mainCam.transform.LookAt(player.transform);

                Debug.Log("[Setup] Camera configured to follow player with smooth movement");
            }
            else
            {
                Debug.LogWarning("[Setup] No player found to follow. Camera follow enabled but needs manual target assignment.");
            }
        }

        private GameObject CreateManager(string name, GameObject parent, System.Type componentType)
        {
            GameObject existing = GameObject.Find(name);
            if (existing != null)
            {
                Debug.Log($"[Setup] {name} already exists, skipping");
                return existing;
            }

            GameObject manager = new GameObject(name);
            manager.transform.parent = parent.transform;

            if (componentType != null)
            {
                manager.AddComponent(componentType);
            }

            return manager;
        }

        private GameObject CreateOrFind(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj == null)
            {
                obj = new GameObject(name);
            }
            return obj;
        }

        private GameObject CreateEnemyPrefab()
        {
            // Check if prefab already exists in Assets/Prefabs folder
            string prefabPath = "Assets/Prefabs/ChaserEnemy.prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existingPrefab != null)
            {
                Debug.Log("[Setup] ChaserEnemy prefab already exists, using existing");
                return existingPrefab;
            }

            // Create Prefabs folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
                Debug.Log("[Setup] Created Assets/Prefabs folder");
            }

            // Create enemy GameObject
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemy.name = "ChaserEnemy";
            enemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            // Tag as Enemy for turret detection
            enemy.tag = "Enemy";

            // Add ChaserEnemy component
            ChaserEnemy swarmEnemy = enemy.AddComponent<ChaserEnemy>();

            // Remove default collider and add trigger collider
            DestroyImmediate(enemy.GetComponent<Collider>());
            SphereCollider collider = enemy.AddComponent<SphereCollider>();
            collider.radius = 0.5f;
            collider.isTrigger = true;  // CRITICAL: Must be trigger to detect player collision

            // Add Rigidbody for trigger physics (required by Unity)
            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.isKinematic = true;  // Don't apply physics forces
            rb.useGravity = false;  // No gravity

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);

            // Delete temporary object
            DestroyImmediate(enemy);

            Debug.Log($"[Setup] Created ChaserEnemy prefab at {prefabPath}");
            return prefab;
        }

        private GameObject CreateTurretPrefab()
        {
            // Check if prefab already exists in Assets/Prefabs folder
            string prefabPath = "Assets/Prefabs/BasicTurret.prefab";
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (existingPrefab != null)
            {
                Debug.Log("[Setup] BasicTurret prefab already exists, using existing");
                return existingPrefab;
            }

            // Create Prefabs folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
                Debug.Log("[Setup] Created Assets/Prefabs folder");
            }

            // Create turret GameObject (cube for base + cylinder for barrel)
            GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
            turret.name = "BasicTurret";
            turret.transform.localScale = new Vector3(1f, 1f, 1f);

            // Add DefenseEmplacement component
            DefenseEmplacement turretDeployable = turret.AddComponent<DefenseEmplacement>();

            // Configure DefenseEmplacement using SerializedObject
            SerializedObject so = new SerializedObject(turretDeployable);
            so.FindProperty("maxHealth").floatValue = 50f;
            so.FindProperty("damage").floatValue = 15f;
            so.FindProperty("detectionRange").floatValue = 10f;
            so.FindProperty("fireRate").floatValue = 1f;
            so.FindProperty("enemyTag").stringValue = "Enemy";
            so.FindProperty("showDebugOverlay").boolValue = true;
            so.FindProperty("logCombatEvents").boolValue = true;
            so.ApplyModifiedProperties();

            // Tag as turret (for enemy targeting)
            turret.tag = "Untagged"; // Turrets are neutral

            // Add collider for enemy damage
            BoxCollider turretCollider = turret.GetComponent<BoxCollider>();
            if (turretCollider == null)
            {
                turretCollider = turret.AddComponent<BoxCollider>();
            }
            turretCollider.isTrigger = false; // Solid collider so enemies can hit it

            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(turret, prefabPath);

            // Delete temporary object
            DestroyImmediate(turret);

            Debug.Log($"[Setup] Created BasicTurret prefab at {prefabPath}");
            return prefab;
        }

        private void CreateTurretSockets()
        {
            Debug.Log("[Setup] Creating Turret Sockets...");

            // Create turret prefab first
            GameObject turretPrefab = CreateTurretPrefab();

            GameObject shipParent = GameObject.Find("=== PORTAL ===");
            if (shipParent == null)
                shipParent = CreateOrFind("=== PORTAL ===");

            // Create parent for turret sockets
            GameObject socketsParent = new GameObject("TurretSockets");
            socketsParent.transform.parent = shipParent.transform;

            // Socket positions in a square around the portal (outside the portal zone)
            Vector3[] socketPositions = new Vector3[]
            {
                new Vector3(8, 0, 8),   // Northeast
                new Vector3(-8, 0, 8),  // Northwest
                new Vector3(-8, 0, -8), // Southwest
                new Vector3(8, 0, -8)   // Southeast
            };

            for (int i = 0; i < socketPositions.Length; i++)
            {
                CreateTurretSocket($"TurretSocket_{i + 1}", socketPositions[i], turretPrefab, socketsParent);
            }

            Debug.Log("[Setup] Turret Sockets created!");
        }

        private void CreateMissionFlowController()
        {
            GameObject flowObject = GameObject.Find("MissionFlowController");
            if (flowObject == null)
            {
                flowObject = new GameObject("MissionFlowController");
                flowObject.AddComponent<MissionFlowController>();
            }

            MissionFlowController flowController = flowObject.GetComponent<MissionFlowController>();
            SerializedObject so = new SerializedObject(flowController);
            SerializedProperty parts = so.FindProperty("requiredParts");
            parts.ClearArray();
            parts.arraySize = 2;

            SerializedProperty part0 = parts.GetArrayElementAtIndex(0);
            part0.FindPropertyRelative("itemType").enumValueIndex = (int)InventoryItemType.PowerCore;
            part0.FindPropertyRelative("requiredCount").intValue = 2;

            SerializedProperty part1 = parts.GetArrayElementAtIndex(1);
            part1.FindPropertyRelative("itemType").enumValueIndex = (int)InventoryItemType.FuelGel;
            part1.FindPropertyRelative("requiredCount").intValue = 2;

            so.ApplyModifiedProperties();
        }

        private void CreateRepairPartPickups()
        {
            GameObject spawnerObject = GameObject.Find("RepairPartSpawner");
            if (spawnerObject == null)
            {
                spawnerObject = new GameObject("RepairPartSpawner");
            }

            if (spawnerObject.GetComponent<RepairPartSpawner>() == null)
            {
                spawnerObject.AddComponent<RepairPartSpawner>();
            }
        }

        private void CreateTurretSocket(string name, Vector3 position, GameObject turretPrefab, GameObject parent)
        {
            // Create socket GameObject (small cube to mark position)
            GameObject socket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socket.name = name;
            socket.transform.position = position;
            socket.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
            socket.transform.parent = parent.transform;

            // Remove default collider and add trigger sphere for player detection
            DestroyImmediate(socket.GetComponent<Collider>());
            SphereCollider trigger = socket.AddComponent<SphereCollider>();
            trigger.radius = 2f;
            trigger.isTrigger = true;

            // Add DeploymentSocket component
            DeploymentSocket turretSocket = socket.AddComponent<DeploymentSocket>();

            // Configure DeploymentSocket using SerializedObject
            SerializedObject so = new SerializedObject(turretSocket);
            so.FindProperty("turretPrefab").objectReferenceValue = turretPrefab;
            so.FindProperty("turretCost").intValue = 50;
            so.FindProperty("activationKey").intValue = (int)KeyCode.T;
            so.FindProperty("playerTag").stringValue = "Player";
            so.FindProperty("showDebugOverlay").boolValue = true;
            so.FindProperty("logPlacementEvents").boolValue = true;
            so.ApplyModifiedProperties();

            Debug.Log($"[Setup] Created {name} at {position}");
        }

        private void ClearScene()
        {
            Debug.Log("[Setup] Clearing scene...");

            var scene = EditorSceneManager.GetActiveScene();
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject obj in rootObjects)
            {
                // Don't delete camera roots
                if (obj.GetComponentInChildren<Camera>() != null)
                    continue;

                DestroyImmediate(obj);
            }

            Debug.Log("[Setup] Scene cleared!");
        }

        private void EnsureTagExists(string tagName)
        {
            // Open tag manager
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // Check if tag already exists
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty tag = tagsProp.GetArrayElementAtIndex(i);
                if (tag.stringValue.Equals(tagName))
                {
                    return; // Tag already exists
                }
            }

            // Add new tag
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = tagName;
            tagManager.ApplyModifiedProperties();

            Debug.Log($"[Setup] Created tag: {tagName}");
        }

        private void EnsureLayerExists(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layer = layersProp.GetArrayElementAtIndex(i);
                if (layer.stringValue == layerName)
                {
                    return;
                }
            }

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layer = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    Debug.Log($"[Setup] Created layer: {layerName}");
                    return;
                }
            }

            Debug.LogWarning("[Setup] No free layer slot available for Minimap.");
        }

        private void CreateMinimap()
        {
            EnsureLayerExists("Minimap");

            GameObject minimapRoot = CreateOrFind("=== MINIMAP ===");

            RenderTexture minimapTexture = GetOrCreateMinimapTexture();

            Vector3 mapCenter = Vector3.zero;
            float mapSize = 120f;
            GameObject ground = GameObject.Find("Ground");
            if (ground != null)
            {
                mapCenter = ground.transform.position;
                mapSize = Mathf.Max(ground.transform.localScale.x, ground.transform.localScale.z) * 10f;
            }

            CreateMinimapFloor(minimapRoot, mapCenter, mapSize);
            CreateMinimapBounds(minimapRoot, mapCenter, mapSize);
            CreateMinimapCamera(minimapRoot, minimapTexture, mapCenter, mapSize);
            CreateMinimapDisplay(minimapRoot, minimapTexture);
            CreateMinimapIcons();
        }

        private RenderTexture GetOrCreateMinimapTexture()
        {
            const string folderPath = "Assets/Minimap";
            const string texturePath = "Assets/Minimap/MinimapTexture.renderTexture";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Minimap");
            }

            RenderTexture texture = AssetDatabase.LoadAssetAtPath<RenderTexture>(texturePath);
            if (texture != null)
            {
                return texture;
            }

            texture = new RenderTexture(512, 512, 16)
            {
                name = "MinimapTexture"
            };

            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.SaveAssets();
            return texture;
        }

        private void CreateMinimapFloor(GameObject parent, Vector3 center, float size)
        {
            GameObject floor = GameObject.Find("MinimapFloor");
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "MinimapFloor";
            }

            floor.transform.parent = parent.transform;
            floor.transform.position = new Vector3(center.x, 0.02f, center.z);
            floor.transform.rotation = Quaternion.identity;
            float scale = size / 10f;
            floor.transform.localScale = new Vector3(scale, 1f, scale);
            floor.layer = LayerMask.NameToLayer("Minimap");

            Collider col = floor.GetComponent<Collider>();
            if (col != null)
            {
                DestroyImmediate(col);
            }

            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetOrCreateMinimapMaterial("MinimapFloorMat", new Color(0.15f, 0.15f, 0.15f));
            }
        }

        private void CreateMinimapBounds(GameObject parent, Vector3 center, float size)
        {
            GameObject boundsRoot = CreateOrFind("MinimapBounds");
            boundsRoot.transform.parent = parent.transform;

            float half = size * 0.5f;
            float height = 0.2f;
            float thickness = 1f;

            CreateMinimapWall("MinimapBound_North", boundsRoot, new Vector3(center.x, 0.1f, center.z + half),
                new Vector3(size, height, thickness));
            CreateMinimapWall("MinimapBound_South", boundsRoot, new Vector3(center.x, 0.1f, center.z - half),
                new Vector3(size, height, thickness));
            CreateMinimapWall("MinimapBound_East", boundsRoot, new Vector3(center.x + half, 0.1f, center.z),
                new Vector3(thickness, height, size));
            CreateMinimapWall("MinimapBound_West", boundsRoot, new Vector3(center.x - half, 0.1f, center.z),
                new Vector3(thickness, height, size));
        }

        private void CreateMinimapWall(string name, GameObject parent, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.Find(name);
            if (wall == null)
            {
                wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = name;
            }

            wall.transform.parent = parent.transform;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.layer = LayerMask.NameToLayer("Minimap");

            Collider col = wall.GetComponent<Collider>();
            if (col != null)
            {
                DestroyImmediate(col);
            }

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetOrCreateMinimapMaterial("MinimapBorderMat", new Color(0.25f, 0.25f, 0.25f));
            }
        }

        private Material GetOrCreateMinimapMaterial(string assetName, Color color)
        {
            string folderPath = "Assets/Minimap";
            string materialPath = $"{folderPath}/{assetName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material != null)
            {
                return material;
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Minimap");
            }

            material = new Material(Shader.Find("Unlit/Color"));
            material.color = color;
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            return material;
        }

        private void CreateMinimapCamera(GameObject parent, RenderTexture texture, Vector3 center, float size)
        {
            GameObject camObject = GameObject.Find("MinimapCamera");
            if (camObject == null)
            {
                camObject = new GameObject("MinimapCamera");
            }

            camObject.transform.parent = parent.transform;

            Camera cam = camObject.GetComponent<Camera>();
            if (cam == null)
            {
                cam = camObject.AddComponent<Camera>();
            }

            MinimapCameraController controller = camObject.GetComponent<MinimapCameraController>();
            if (controller == null)
            {
                controller = camObject.AddComponent<MinimapCameraController>();
            }

            float orthographicSize = Mathf.Max(30f, size * 0.5f);

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("height").floatValue = 50f;
            so.FindProperty("orthographicSize").floatValue = orthographicSize;
            so.FindProperty("mapCenter").vector3Value = center;
            so.FindProperty("renderTexture").objectReferenceValue = texture;
            so.FindProperty("minimapLayerName").stringValue = "Minimap";
            so.ApplyModifiedProperties();

            cam.cullingMask = 1 << LayerMask.NameToLayer("Minimap");
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.targetTexture = texture;
        }

        private void CreateMinimapDisplay(GameObject parent, RenderTexture texture)
        {
            GameObject canvasObject = GameObject.Find("MinimapCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("MinimapCanvas");
            }

            canvasObject.transform.parent = parent.transform;

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            GameObject displayObject = GameObject.Find("MinimapDisplay");
            if (displayObject == null)
            {
                displayObject = new GameObject("MinimapDisplay");
                displayObject.transform.SetParent(canvasObject.transform, false);
            }

            RawImage image = displayObject.GetComponent<RawImage>();
            if (image == null)
            {
                image = displayObject.AddComponent<RawImage>();
            }

            image.texture = texture;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(200f, 200f);
            rect.anchoredPosition = new Vector2(-20f, -20f);
        }

        private void CreateMinimapIcons()
        {
            EnsureMinimapIcon(GameObject.FindGameObjectWithTag("Player"), Color.white, 1.6f);

            GameObject portal = GameObject.Find("PortalGate");
            if (portal == null)
            {
                portal = GameObject.Find("=== PORTAL ===");
            }
            EnsureMinimapIcon(portal, Color.blue, 2.2f);

            RepairPartPickup[] pickups = Object.FindObjectsByType<RepairPartPickup>(FindObjectsSortMode.None);
            foreach (RepairPartPickup pickup in pickups)
            {
                Color color = GetPickupColor(pickup);
                EnsureMinimapIcon(pickup.gameObject, color, 1.4f);
            }
        }

        private void EnsureMinimapIcon(GameObject target, Color color, float size)
        {
            if (target == null)
            {
                return;
            }

            MinimapIcon icon = target.GetComponent<MinimapIcon>();
            if (icon == null)
            {
                icon = target.AddComponent<MinimapIcon>();
            }

            SerializedObject so = new SerializedObject(icon);
            so.FindProperty("iconColor").colorValue = color;
            so.FindProperty("iconSize").floatValue = size;
            so.FindProperty("iconHeight").floatValue = 0.4f;
            so.FindProperty("minimapLayerName").stringValue = "Minimap";
            so.ApplyModifiedProperties();
        }

        private Color GetPickupColor(RepairPartPickup pickup)
        {
            SerializedObject so = new SerializedObject(pickup);
            SerializedProperty itemTypeProp = so.FindProperty("itemType");
            if (itemTypeProp == null)
            {
                return Color.yellow;
            }

            InventoryItemType itemType = (InventoryItemType)itemTypeProp.enumValueIndex;
            switch (itemType)
            {
                case InventoryItemType.PowerCore:
                    return Color.yellow;
                case InventoryItemType.FuelGel:
                    return Color.red;
                default:
                    return Color.yellow;
            }
        }
    }
}
