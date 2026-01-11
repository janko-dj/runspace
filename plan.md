# Project Plan / State

This file is a quick handoff for future AI or devs. It summarizes the current
state, key systems, and the intended flow so work can resume without re‑digging.

## Core Flow (Current)
- Scenes:
  - PreGame (menu)
  - Game (mission)
- Mission flow:
  - Landing -> Expedition (leave portal safe zone)
  - Expedition -> RunBack (all required repair parts collected)
  - RunBack -> FinalStand (re-enter portal with parts)
  - FinalStand -> EndSuccess/EndFail (repairs complete or fail)
  - End screen shown in mission scene; Enter/Click returns to menu

## Key Systems and Files

### Mission / Progression
- Mission config data:
  - `Assets/Scripts/Missions/MissionConfig.cs`
  - Fields include: threat, quest reward/requirement, counts for required/spawned parts,
    layout, quest item/bonus counts, spawn seed.
- Mission session:
  - `Assets/Scripts/Missions/MissionSession.cs` (menu <-> game handoff)
- Player progress (runtime only):
  - `Assets/Scripts/Progression/PlayerProgress.cs`
- Quest item data:
  - `Assets/Scripts/Progression/QuestItem.cs`
- Quest item pickup:
  - `Assets/Scripts/Systems/QuestItemPickup.cs`

### Mission Layout / Procedural Placement
- Layout data:
  - `Assets/Scripts/Missions/MissionLayout.cs`
  - Circle/Ellipse shape, radii, preset portal spawn points (2–4 positions)
- Spawn utility:
  - `Assets/Scripts/Missions/SpawnZoneUtility.cs`
- Layout controller:
  - `Assets/Scripts/Missions/MissionLayoutController.cs`
  - Picks portal/player spawn from preset list
  - Places repair parts (MID/FAR), quest item (FAR), bonus pickups (any zone)
  - Updates portal safe zone size/position
  - Gizmos for map boundary + zone rings + spawned positions

### Repair Parts / Inventory
- Repair part pickup:
  - `Assets/Scripts/Systems/RepairPartPickup.cs`
- Mission flow controller (requirements / phase transitions):
  - `Assets/Scripts/Systems/MissionFlowController.cs`

### End Screen
- `Assets/Scripts/Missions/EndScreenController.cs`
  - Shows mission name, time survived, enemy kills, quest item status
  - Input: Enter or click returns to menu

### Threat / Kills
- `Assets/Scripts/Systems/PressureSystem.cs`
  - Tracks `EnemyKillCount` for end screen stats

### Minimap
- Minimap camera + icon system:
  - `Assets/Scripts/Minimap/MinimapCameraController.cs`
  - `Assets/Scripts/Minimap/MinimapIcon.cs`
- Scene setup tool creates Minimap layer, RenderTexture, UI RawImage, floor/bounds

### Scene Setup Tool (Single Source of Truth)
- `Assets/Scripts/Editor/GameSceneSetupTool.cs`
  - Creates/updates both PreGame and Game scenes
  - Adds MissionSelectionMenu, MissionSession, PlayerProgress in PreGame
  - Adds MissionBootstrapper, MissionFlow, PortalSafeZone, EndScreen, Layout controller
  - Adds/updates player, camera, minimap, managers
  - Disables debug testing inputs and phase hotkeys

## Assets Created by Tool
- `Assets/Missions/Mission1.asset`
- `Assets/Missions/Mission2.asset`
- `Assets/Missions/Layout_Mission1.asset`
- `Assets/Missions/Layout_Mission2.asset`
- `Assets/Progression/QuestItem_Mission1.asset`
- `Assets/Minimap/MinimapTexture.renderTexture`

## Known Behavior / Expectations
- Portal/player spawn from preset layout points (2–4).
- Repair parts spawn procedurally per run based on MissionConfig counts.
- Quest item is a world pickup in FAR zone, not an end reward.
- End success unlocks Mission 2 via PlayerProgress (runtime only).
- Debug phase hotkeys are force-disabled by `RunPhaseController`.

## Next Likely Tasks
- Replace OnGUI menu with basic Unity UI.
- Add proper quest item visuals and differentiate bonus pickups.
- Tuning spawn distances, zone radii, and layouts per world theme.
- Add persistence to PlayerProgress (save/load).
- Add spawn point gizmo helpers or editor tooling for layout spawn points.
