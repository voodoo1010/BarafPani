# BarafPani — Project Structure

## Architecture Overview

This is a casual multiplayer game using **Netcode for GameObjects (NGO)** with a **host model** (one player hosts, others join). Movement and player state are **client-authoritative** — the owning client controls their own character directly, no server validation.

The project uses two reactive data systems with a hard boundary between them:

| System | Scope | Authority | Use For |
|--------|-------|-----------|---------|
| **NGO NetworkVariables** | Networked state synced across all clients | Owner or Server writes | Anything another player needs to see or react to |
| **Unity Atoms** | Local-only reactive state on each client | Local client | UI binding, camera, audio, local VFX, settings, menus |

**The rule is simple: if the value crosses the network, it's a NetworkVariable. If it stays on one machine, it's an Atom.**

Violating this boundary — using Atoms for networked state or NetworkVariables for local-only state — creates bugs and confusion. No exceptions.

---

## Networking Model

### Host Architecture

- One player acts as **host** (server + client combined)
- Other players connect as **clients**
- The host's game instance runs both server logic and their own client
- Unity Relay handles NAT traversal so players don't need to port-forward

### Authority Model

| What | Who Controls | Sync Method |
|------|-------------|-------------|
| Player movement | Owning client | `NetworkTransform` (owner-authoritative) |
| Player state (health, score, cosmetics) | Owning client | `NetworkVariable` with `Owner` write permission |
| Match state (timer, rounds, win condition) | Host only | `NetworkVariable` with `Server` write permission |
| Environment objects (doors, pickups, spawns) | Host | Host spawns/despawns, `NetworkVariable` for state |
| UI, camera, audio, settings | Each client locally | Not networked — use Atoms or plain C# |

### NetworkVariable Write Permissions

```csharp
// Player owns their own state — casual, trusting clients
NetworkVariable<int> _health = new(writePerm: NetworkVariableWritePermission.Owner);

// Only host controls match flow
NetworkVariable<float> _matchTimer = new(writePerm: NetworkVariableWritePermission.Server);
```

Default to `Owner` for player-specific state. Use `Server` for shared game state only the host should control.

---

## Atoms vs NetworkVariables — The Boundary

### Use NetworkVariables When:

- Another player needs to see the value (health bar above someone else's head)
- The host needs to read it for game logic (player position for spawn decisions)
- It affects gameplay for anyone other than the local player
- It must survive a client disconnect/reconnect

### Use Atoms When:

- Driving local UI elements (HUD binding, menu state, settings values)
- Camera parameters (shake intensity, zoom level, follow target)
- Audio state (music volume, mixer snapshots, spatial audio config)
- Local VFX triggers (screen flash, post-processing overrides)
- Input state that hasn't been sent to the network yet
- Anything in the pause menu, settings screen, or local-only systems

### The Bridge Pattern

When networked state needs to drive local reactive systems (e.g., a NetworkVariable health value updating a locally-bound HUD via Atoms):

```csharp
// On the NetworkBehaviour
_health.OnValueChanged += (oldVal, newVal) =>
{
    // Write to local Atom so UI can observe it without touching networking
    _localHealthAtom.Value = newVal;
};
```

This is a one-way bridge: **Network -> Atom**. Never go Atom -> Network. The NetworkVariable is the source of truth. The Atom is a local mirror for UI/audio/VFX consumption.

---

## Folder Structure

```
Assets/
├── _Game/                          Shared project-wide foundation
│   ├── Network/                    All network infrastructure
│   │   ├── Config/                 NetworkManager prefab, NetworkPrefabsList
│   │   └── Scripts/                Connection, session, spawning, relay setup
│   ├── Atoms/                      LOCAL-ONLY cross-feature reactive data
│   │   ├── Variables/              IsPaused, MasterVolume, CurrentMenuState
│   │   ├── Events/                 OnLocalPause, OnSettingsChanged, OnSceneReady
│   │   ├── Actions/                LogAction, PlaySFXAction
│   │   ├── Conditions/             IsGamePaused, IsMenuOpen
│   │   ├── Lists/                  (as needed)
│   │   └── FSM/                    UIStateMachine, AppLifecycleFSM
│   ├── Config/                     Static ScriptableObjects (balance, tuning, databases)
│   ├── Audio/
│   │   ├── Music/
│   │   ├── SFX/
│   │   └── Mixers/
│   ├── Input/                      Input Action assets, input maps
│   ├── Materials/                  Shared materials (skybox, post-process)
│   ├── Prefabs/                    Non-networked shared prefabs (local FX, loading screen)
│   ├── Scenes/
│   │   ├── _Boot/                  Bootstrap scene (NetworkManager spawns here)
│   │   ├── MainMenu/
│   │   ├── Gameplay/
│   │   └── Test/                   Per-dev scratchpad scenes
│   ├── Scripts/
│   │   ├── Atoms/                  Custom Atom type DEFINITIONS (C# classes only)
│   │   ├── Core/                   Bootstrap, app lifecycle, scene management
│   │   ├── Extensions/             C# extension methods
│   │   ├── Interfaces/             IDamageable, IInteractable, INetworkOwnable
│   │   ├── Data/                   Shared structs, enums, INetworkSerializable types
│   │   └── Utilities/              Object pooling, math helpers, coroutine runners
│   ├── Shaders/
│   ├── Textures/
│   ├── UI/
│   │   ├── Fonts/
│   │   ├── Icons/
│   │   ├── Sprites/
│   │   └── Themes/
│   └── VFX/
│
├── _Features/                      One folder per gameplay feature
│   ├── Player/
│   │   ├── Atoms/                  LOCAL-ONLY player reactive data
│   │   │   ├── Variables/          LocalPlayerHealth (mirror), IsGrounded (local only)
│   │   │   ├── Events/             OnLocalPlayerDamaged (for screen shake/audio)
│   │   │   └── ...
│   │   ├── Config/                 PlayerConfig.asset (base HP, speed — static tuning)
│   │   ├── Scripts/
│   │   │   ├── PlayerController.cs         NetworkBehaviour — owner moves themselves
│   │   │   ├── PlayerHealth.cs             NetworkVariable<int>, owner writes
│   │   │   ├── PlayerAnimator.cs           Reads synced state, drives anims
│   │   │   ├── PlayerInteraction.cs        Proximity interact
│   │   │   └── PlayerData.cs               INetworkSerializable (name, cosmetic id)
│   │   ├── Prefabs/                Player.prefab (NetworkObject)
│   │   ├── Animations/
│   │   │   ├── Controllers/
│   │   │   └── Clips/
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── VFX/
│   │
│   │   └── _Features/              Sub-features (only when genuinely independent)
│   │       └── [SubFeatureName]/
│   │
│   ├── Lobby/
│   │   ├── Atoms/
│   │   │   ├── Variables/          LobbyUIState (local menu state)
│   │   │   └── Events/             OnLobbyUIRefresh
│   │   ├── Config/                 LobbyConfig.asset (max players, countdown)
│   │   ├── Scripts/
│   │   │   ├── LobbyManager.cs             NetworkBehaviour — host creates, clients join
│   │   │   ├── LobbyPlayerState.cs         NetworkBehaviour (ready status, cosmetics)
│   │   │   └── LobbyUI.cs                  Reads synced state, updates local UI
│   │   └── Prefabs/
│   │
│   ├── MatchFlow/
│   │   ├── Atoms/
│   │   │   ├── Variables/          LocalMatchTimer (Atom mirror of networked timer)
│   │   │   └── Events/             OnMatchPhaseChangedLocal (for UI/audio)
│   │   ├── Config/                 MatchConfig.asset (round time, score limit)
│   │   ├── Scripts/
│   │   │   ├── MatchManager.cs             NetworkBehaviour — host runs game logic
│   │   │   ├── ScoreTracker.cs             NetworkVariable per player/team
│   │   │   └── MatchHUD.cs                 Reads synced state via Atom mirrors
│   │   └── Prefabs/
│   │
│   ├── Camera/
│   │   ├── Atoms/
│   │   │   ├── Variables/          CameraTarget, CameraZoom, ShakeIntensity
│   │   │   └── Events/             OnCameraShake
│   │   ├── Config/                 CameraConfig.asset
│   │   ├── Scripts/                Entirely local — no networking
│   │   └── Profiles/               Cinemachine profiles, volume overrides
│   │
│   ├── Environment/
│   │   ├── Atoms/
│   │   │   └── Events/             OnDoorOpenedLocal (for local SFX/VFX)
│   │   ├── Config/                 InteractableConfig.asset
│   │   ├── Scripts/
│   │   │   ├── Door.cs                     NetworkBehaviour (synced open/close)
│   │   │   ├── Pickup.cs                   NetworkBehaviour (despawn on collect)
│   │   │   └── InteractableTrigger.cs      Proximity detection
│   │   ├── Prefabs/                NetworkObject prefabs
│   │   ├── Materials/
│   │   ├── Models/
│   │   └── Textures/
│   │
│   ├── AI/
│   │   ├── Atoms/
│   │   │   └── Events/             OnEnemyDeathLocal (for local VFX/audio)
│   │   ├── Config/                 EnemySpawnTable.asset, DifficultyScaling.asset
│   │   ├── Scripts/
│   │   │   ├── AIController.cs             NetworkBehaviour — host runs AI brain
│   │   │   ├── AIVisuals.cs                Reads synced state, drives anims
│   │   │   └── AISensor.cs                 Detection logic (runs on host)
│   │   ├── Prefabs/                Enemy.prefab (NetworkObject)
│   │   ├── Animations/
│   │   └── Models/
│   │
│   └── UI/
│       ├── HUD/
│       │   ├── Atoms/
│       │   │   ├── Variables/      DisplayedHealth, DisplayedScore (Atom mirrors)
│       │   │   └── Events/         OnHUDFlash
│       │   ├── Config/             HUDLayout.asset
│       │   ├── Scripts/            HealthBar.cs, ScoreDisplay.cs — reads Atoms
│       │   └── Prefabs/
│       ├── PauseMenu/
│       │   ├── Atoms/
│       │   │   └── Variables/      SelectedMenuIndex
│       │   ├── Scripts/
│       │   └── Prefabs/
│       └── Settings/
│           ├── Atoms/
│           │   └── Variables/      GraphicsQuality, AudioVolume
│           ├── Scripts/
│           └── Prefabs/
│
├── _Debug/
│   ├── Scripts/                    NetworkDebugOverlay.cs, FakeLatencySimulator.cs
│   ├── Scenes/                     NetworkTestScene (2-player local test)
│   └── Prefabs/
│
├── Art/                            Staging area — moves to feature folders after integration
│   ├── Models/
│   │   ├── Characters/
│   │   ├── Props/
│   │   └── Environment/
│   ├── Textures/
│   ├── Animations/
│   └── Concepts/                   Reference images (.gitignore this)
│
├── ThirdParty/                     Read-only (Custom Inspector, FlatKit, etc.)
│
└── Settings/                       URP settings, quality, input (Unity-managed)
```

---

## Adding a New Feature

Minimum template:

```
_Features/[FeatureName]/
├── Scripts/
└── Prefabs/
```

Add these only when the feature needs them:

```
├── Atoms/          Only if the feature has local reactive state
│   ├── Variables/
│   └── Events/
├── Config/         Only if the feature has static tuning data
├── Animations/     Only if the feature has unique animations
├── Materials/      Only if the feature has unique materials
├── Models/         Only if the feature has unique models
├── VFX/            Only if the feature has unique effects
└── _Features/      Only if the feature has genuinely independent sub-systems
```

Do not pre-create empty folders.

---

## Assembly Definitions

```
_Game/Scripts/                      → Game.Core.asmdef
                                      Dependencies: Unity Atoms Core, Unity Atoms Base
_Game/Scripts/Atoms/                → Game.Atoms.asmdef
                                      Dependencies: Game.Core, Unity Atoms Core
_Game/Network/Scripts/              → Game.Network.asmdef
                                      Dependencies: Game.Core, Unity.Netcode.Runtime

_Features/Player/Scripts/           → Game.Player.asmdef
                                      Dependencies: Game.Core, Game.Atoms, Game.Network
_Features/Lobby/Scripts/            → Game.Lobby.asmdef
                                      Dependencies: Game.Core, Game.Atoms, Game.Network
_Features/MatchFlow/Scripts/        → Game.MatchFlow.asmdef
                                      Dependencies: Game.Core, Game.Atoms, Game.Network
_Features/Camera/Scripts/           → Game.Camera.asmdef
                                      Dependencies: Game.Core, Game.Atoms (no Game.Network)
_Features/Environment/Scripts/      → Game.Environment.asmdef
                                      Dependencies: Game.Core, Game.Atoms, Game.Network
_Features/AI/Scripts/               → Game.AI.asmdef
                                      Dependencies: Game.Core, Game.Atoms, Game.Network
_Features/UI/**/Scripts/            → Game.UI.asmdef
                                      Dependencies: Game.Core, Game.Atoms (no Game.Network)

_Debug/Scripts/                     → Game.Debug.asmdef
                                      Editor/Dev only, can depend on anything
```

**Features never reference other feature assemblies.** Cross-feature communication goes through:
1. NetworkVariables on shared NetworkBehaviours (for networked state)
2. Atoms in `_Game/Atoms/` (for local cross-feature signals)
3. RPCs to the host, which routes to the appropriate system

---

## Installed Packages

```
com.unity.netcode.gameobjects       2.9.2    Core networking
com.unity.services.multiplayer       2.0.0    Relay + Lobby (NAT traversal, room codes)
com.unity.inputsystem                1.18.0   New Input System
com.unity.cinemachine                3.1.5    Camera
com.unity.ai.navigation             2.0.10   NavMesh (AI pathfinding)
com.unity.probuilder                 6.0.9    Level prototyping
com.unity.render-pipelines.universal 17.3.0   URP
com.unity.timeline                   1.8.10   Cutscenes, sequencing
com.unity-atoms (full suite)                  Local reactive data layer
```

---

## Team Ownership (4 People)

| Dev | Primary Ownership | Shared Responsibility |
|-----|-------------------|-----------------------|
| **Dev 1 — Network + Core** | `_Game/Network/`, `_Game/Scripts/Core/`, `_Features/Lobby/`, `_Features/MatchFlow/` | Network prefab list, boot sequence, session lifecycle |
| **Dev 2 — Player + Gameplay** | `_Features/Player/`, gameplay mechanics features | Player prefab, core gameplay feel |
| **Dev 3 — World + AI** | `_Features/AI/`, `_Features/Environment/` | Enemy prefabs, level objects, interactables |
| **Dev 4 — UI + Polish** | `_Features/UI/`, `_Features/Camera/`, `_Game/Audio/`, `_Game/UI/` | All HUD, menus, camera, audio, VFX |

**Everyone can read from `_Game/`. Modifications to `_Game/` require a message in team chat before pushing.**

---

## NetworkObject Prefab Registry

Every spawnable prefab with a `NetworkObject` component must be registered in:

```
_Game/Network/Config/NetworkPrefabsList.asset
```

This is a single file. One person (Dev 1) owns it to prevent merge conflicts. When you create a new NetworkObject prefab in any feature folder, tell Dev 1 to register it.

The existing `Assets/DefaultNetworkPrefabs.asset` should be consolidated into this location.

---

## Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Folders | PascalCase | `MatchFlow` |
| Scripts | PascalCase matching class name | `PlayerController.cs` |
| NetworkBehaviours | PascalCase, no suffix needed | `PlayerHealth.cs` |
| Atom Variables | PascalCase descriptive | `LocalPlayerHealth.asset` |
| Atom Events | PascalCase with On prefix | `OnLocalPlayerDamaged.asset` |
| Atom Actions | PascalCase with Action suffix | `PlayHitSFXAction.asset` |
| Atom Conditions | PascalCase descriptive | `IsGamePaused.asset` |
| Config SOs | PascalCase with Config suffix | `PlayerConfig.asset` |
| Prefabs | PascalCase | `Player.prefab` |
| Materials | PascalCase | `PlayerSkin.mat` |
| Textures | PascalCase with channel suffix | `PlayerSkin_Albedo.png` |
| Animations | PascalCase with subject prefix | `Player_Idle.anim` |
| Scenes | PascalCase | `MainMenu.unity` |

---

## Rules

### 1. NetworkVariables for Networked State, Atoms for Local State — No Exceptions

If another player needs to see it or the host needs to read it for game logic, it's a `NetworkVariable` on a `NetworkBehaviour`. If it only matters on one machine (UI, camera, audio, settings), it's an Atom or plain C#.

The one-way bridge pattern (Network -> Atom) is acceptable for feeding networked values into local UI binding. Never bridge the other direction.

### 2. Atoms/ vs Config/ Is a Hard Boundary

- `Atoms/` = reactive, runtime-mutable, observable ScriptableObjects
- `Config/` = static, design-time-only ScriptableObjects (read once at init)

Never mix them.

### 3. Scripts/ Contains Code, Atoms/ Contains Instances

- `Scripts/` has `.cs` files: MonoBehaviours, NetworkBehaviours, custom Atom class definitions
- `Atoms/` has `.asset` files: ScriptableObject instances created from those classes

The class `PlayHitSFXAction : AtomAction` lives in `Scripts/`.
The SO instance `PlayHitSFX.asset` lives in `Atoms/Actions/`.

### 4. Global vs Feature Ownership

| Location | Who creates | Who reads | Who writes |
|----------|------------|-----------|------------|
| `_Game/Atoms/` | Team consensus | Any feature | Designated owner |
| `_Game/Network/` | Dev 1 | Any feature | Dev 1 |
| `_Features/[X]/Atoms/` | Feature owner | That feature (primarily) | That feature only |
| `_Features/[X]/Scripts/` | Feature owner | Via assembly refs only | Feature owner only |

A feature may read another feature's Atoms or listen to another feature's Atom Events. It must never write to them. Cross-feature mutations go through `_Game/Atoms/Events/`.

### 5. Feature Folders Are Self-Contained

Deleting a feature folder should cause missing SO references and prefab warnings — never compile errors in other features.

### 6. No Loose Files in Assets Root

Every file lives inside an appropriate folder.

### 7. ThirdParty Is Read-Only

Never modify files inside `ThirdParty/`. Write wrappers in `_Game/Scripts/` or the relevant feature folder.

### 8. _Debug Gets Stripped from Builds

All scripts in `_Debug/` use Editor-only assembly definitions or `#if UNITY_EDITOR` / `#if DEVELOPMENT_BUILD` guards.

### 9. _Game Changes Require Team Communication

`_Game/` is the shared contract. Communicate before modifying shared Atoms, interfaces, config, or network infrastructure.

### 10. Art/ Is a Staging Area

Assets in `Art/` are awaiting integration. Once placed into a feature folder, remove from `Art/`.

### 11. One Scene Per Subfolder

Each scene gets its own subfolder containing the `.unity` file and scene-specific baked data.

### 12. Host Logic Is Clearly Marked

Any code that only runs on the host should have an early guard:

```csharp
if (!IsServer) return;
```

This applies to: match flow, AI brains, environment spawning, score calculation. Everything else is owner-authoritative (`if (!IsOwner) return;`).

### 13. Sub-Features Are Optional

Only nest `_Features/` inside a feature when it has genuinely independent subsystems with their own reactive data. One level of nesting maximum. Most features don't need this.

---

## Data Flow Summary

```
[Client Input] → [Owner NetworkBehaviour] → [NetworkVariable] → [All Clients]
                                                    │
                                                    ▼
                                            [Atom Variable]  (one-way bridge, local only)
                                                    │
                                                    ▼
                                            [UI / Audio / VFX]  (local reactions)


[Host-Only Logic] → [Server NetworkVariable] → [All Clients]
                                                    │
                                                    ▼
                                            [Atom Variable]  (one-way bridge)
                                                    │
                                                    ▼
                                            [Match HUD / Timers]


[Local-Only State] → [Atom Variable] → [Atom Listeners]
                                            │
                                            ▼
                                    [Camera / Settings / Menus]
```

---

## Quick Reference — "Where Does This Go?"

| I need to... | Put it in... |
|--------------|-------------|
| Sync player position | `NetworkTransform` on Player prefab (owner-authoritative) |
| Sync player health | `NetworkVariable<int>` in `PlayerHealth.cs` |
| Show health on local HUD | Atom Variable `LocalPlayerHealth`, bridged from NetworkVariable |
| Play local hit sound | Atom Event `OnLocalPlayerDamaged` → Atom Action `PlayHitSFXAction` |
| Control match timer | `NetworkVariable<float>` in `MatchManager.cs` (server write only) |
| Pause the game locally | Atom Variable `IsPaused` in `_Game/Atoms/Variables/` |
| Store weapon stats | Config SO `WeaponDatabase.asset` in feature's `Config/` |
| Define a new enemy type | Config SO in `_Features/AI/Config/`, prefab in `_Features/AI/Prefabs/` |
| Add a UI screen | Scripts + Prefabs in `_Features/UI/[ScreenName]/`, Atoms if reactive |
| Prototype a level | ProBuilder in a test scene under `_Game/Scenes/Test/` |
| Import new art | Drop in `Art/`, move to feature folder when integrating |
