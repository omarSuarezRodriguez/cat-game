# 🐱 Whisker Haven — Developer Setup Guide

> *Restore a sanctuary. Collect cozy cats. Let life idle.*

---

## Quick Start (5 minutes to first run)

### Prerequisites
- **Unity 6** (6000.x LTS) — [Download Hub](https://unity.com/download)
- TextMeshPro package (auto-imported)
- Universal Render Pipeline 2D

### Step 1: Open the Project
1. Open Unity Hub → **Open** → select `cat-game/` folder
2. Unity will import all assets and compile scripts (~2–3 min first time)

### Step 2: Install TMP Essentials
When prompted: **Window → TextMeshPro → Import TMP Essential Resources**

### Step 3: Create All Data Assets
Menu: **WhiskerHaven → Create All Data Assets**

This auto-generates:
- 15 cat ScriptableObjects (`Assets/ScriptableObjects/Cats/`)
- 6 habitat ScriptableObjects (`Assets/ScriptableObjects/Habitats/`)
- 10 daily missions (`Assets/ScriptableObjects/Missions/`)
- 20 achievements (`Assets/ScriptableObjects/Achievements/`)
- 3 volunteers (`Assets/ScriptableObjects/Volunteers/`)
- `GameConfig` at `Assets/Resources/Config/GameConfig.asset`

### Step 4: Build the Bootstrap Scene
Create a new scene `Assets/Scenes/Bootstrap.unity` and:

1. Create an empty GameObject → name it **GameManager**
   - Add component: `GameManager`
   - Assign `GameConfig` from `Resources/Config/`

2. Create **AudioManager** GameObject
   - Add component: `AudioManager`

3. Create **SaveSystem** GameObject
   - Add component: `SaveSystem`

4. Create **UICanvas** (Canvas, Screen Space - Camera, Sort Order 0)
   - Add UIManager component
   - Wire up all panel prefabs (see Prefabs section below)

5. Create additional singleton GameObjects:
   - ResourceManager
   - CatManager
   - HabitatManager
   - VolunteerSystem
   - PurrPowerSystem
   - MissionSystem
   - AchievementSystem
   - IdleManager

> **Tip:** All systems extend `Singleton<T>` and auto-create themselves if missing, but explicit scene placement gives you Inspector-level configuration.

---

## Architecture Overview

```
WhiskerHaven/
├── Core/               Event-driven backbone
│   ├── EventBus        Decoupled pub/sub (no direct refs between systems)
│   ├── GameManager     Boot orchestrator, init order controller
│   ├── SaveSystem      JSON save to persistentDataPath, auto-save
│   ├── ResourceManager Currency: Snuggles, GoldenPaw, Blueprints
│   └── ObjectPool<T>   Generic pool for VFX
│
├── Data/               ScriptableObject definitions (pure data)
│   ├── CatData         Cat identity, rarity, production, personality
│   ├── HabitatData     10 upgrade levels, slot counts, bonuses
│   ├── MissionData     Daily + story missions with weighted RNG
│   ├── AchievementData 20 achievements + Steam ID fields
│   ├── VolunteerData   3 specialists with specialty bonuses
│   └── GameConfig      Master config — all arrays + tuning values
│
├── Gameplay/           Game systems (mutate SaveData)
│   ├── IdleManager     Tick engine, offline progress, production calc
│   ├── PurrPowerSystem Happiness → global multiplier (1×–5×)
│   ├── CatManager      Rescue, assign, happiness ticks
│   ├── HabitatManager  Unlock, upgrade, production multipliers
│   ├── VolunteerSystem Assign, bonuses, blueprint generation
│   ├── MissionSystem   Daily reset, weighted pool, progress tracking
│   └── AchievementSystem Condition eval, Steam hook-ready
│
├── UI/                 View layer (read-only access to game state)
│   ├── UIManager       Panel stack, navigation, global events
│   ├── MainHUDView     Resource bar, Purr Power, Hall progress, tabs
│   ├── CatCollectionView Grid + detail panel, filter, rescue flow
│   ├── HabitatView     Habitat list, upgrade flow, cat slot management
│   ├── MissionView     Daily missions, countdown timer, claim flow
│   ├── AchievementView Category filter, progress, point totals
│   ├── WelcomeBackView Offline earnings modal
│   ├── TutorialView    6-step Act 0 tutorial with skip
│   ├── SettingsView    Audio sliders, save/delete, version info
│   ├── FloatingNumber  Pooled +X Snuggles popups
│   └── NotificationBanner Queued toast notifications
│
├── Audio/
│   └── AudioManager    SFX pool (6 sources), crossfade music
│
└── Utils/
    ├── Singleton<T>    Persistent DontDestroyOnLoad singleton
    ├── NumberFormatter K/M/B/T suffix + time formatting
    └── Extensions      Transform, collection, color helpers
```

---

## Save System

- **Location:** `Application.persistentDataPath/whisker_save_v1.json`
- **Format:** JSON (Unity JsonUtility — human-readable, fast)
- **Auto-save:** Every 60 seconds (configurable in GameConfig)
- **On quit:** Saves immediately
- **Atomic writes:** Writes to `.tmp` then renames (prevents corruption)
- **Offline progress:** Calculated on load from timestamp delta

Windows save path example:
```
C:\Users\<User>\AppData\LocalLow\DefaultCompany\WhiskerHaven\whisker_save_v1.json
```

---

## Economy Balancing

| Currency    | Symbol | Source                         | Sink                          |
|-------------|--------|--------------------------------|-------------------------------|
| Snuggles    | 🐾     | Cat production (idle)          | Rescue cats, upgrade habitats |
| Golden Paw  | ✨     | Mission rewards, rare achievements | Unlock rare cats, volunteers |
| Blueprints  | 📐     | Volunteer gen, missions         | Habitat upgrades (high levels) |

### Production Formula
```
Snuggles/s = Σ cats [ BaseProd(level) × HappinessMult × HabitatMult × VolunteerBonus ]
           + HabitatPassiveBonus
           × PurrPowerMultiplier(1×–5×)
```

### Purr Power
```
NormalizedHappiness = Σ (catHappiness/maxHappiness × purrContrib) / Σ purrContrib
PurrPower = Lerp(1.0, 5.0, NormalizedHappiness)
```

### Offline Progress
```
Offline earnings = Snuggles/s × min(timeAway, 8h) × 0.5 (50% efficiency)
```

---

## Content — Act 1

### Cats (15)
| Name         | Rarity   | Personality | Cost (Snuggles) | Prod/s |
|--------------|----------|-------------|-----------------|--------|
| Mittens      | Common   | Lazy        | 50              | 1.0    |
| Biscuit      | Common   | Playful     | 60              | 1.2    |
| Luna         | Common   | Curious     | 55              | 1.1    |
| Shadow       | Common   | Grumpy      | 45              | 0.9    |
| Cream Puff   | Common   | Sweet       | 65              | 1.3    |
| Patches      | Common   | Wild        | 50              | 1.0    |
| Mochi        | Common   | Lazy        | 55              | 1.0    |
| Ginger       | Common   | Playful     | 60              | 1.2    |
| Professor    | Uncommon | Curious     | 200             | 1.8    |
| Duchess      | Uncommon | Royal       | 250             | 2.0    |
| Pixel        | Uncommon | Curious     | 180             | 1.7    |
| Stormy       | Uncommon | Wild        | 220             | 1.9    |
| Butterscotch | Uncommon | Sweet       | 230             | 2.0    |
| Noodle       | Uncommon | Lazy        | 190             | 1.6    |
| **Astrid**   | **Rare** | Royal       | 1,000 + 5 GP    | **4.0**|

### Habitats (6)
| Habitat       | Unlock Req | Cost         | Preferred     |
|---------------|-----------|--------------|---------------|
| Sunroom       | Hall Lv.1 | Free         | Lazy cats     |
| Garden        | Hall Lv.1 | 300 🐾 5 📐  | Curious cats  |
| Library       | Hall Lv.2 | 800 🐾 15 📐 | Royal cats    |
| Kitchen       | Hall Lv.2 | 800 🐾 15 📐 | Sweet cats    |
| Adventure Room| Hall Lv.3 | 2000 🐾 30 📐| Wild cats     |
| Royal Suite   | Hall Lv.4 | 8000 🐾 80 📐| Royal cats    |

Each habitat has **10 upgrade levels** with increasing cat slots (2→8) and production multipliers (1.0×→2.8×).

### Volunteers (3)
| Name    | Specialty  | Unlock         | Prod Bonus | Happy Bonus | BP/h |
|---------|-----------|----------------|-----------|-------------|------|
| Felix   | Medic      | Hall Lv.1, 500🐾| +25%      | +30%        | 0.5  |
| Mei     | Builder    | Hall Lv.2, 2✨  | +40%      | +10%        | 2.0  |
| Jasmine | Researcher | Hall Lv.3, 1500🐾+1✨| +20% | +50%       | 1.0  |

---

## Events (EventBus)

All systems communicate through typed structs:

```csharp
EventBus.Subscribe<OnCatRescued>(evt => Debug.Log(evt.CatId));
EventBus.Publish(new OnResourceChanged { Type = ResourceType.Snuggles, Delta = 100 });
```

Available events:
- `OnResourceChanged` — any currency changes
- `OnCatRescued` — new cat joined
- `OnCatHappinessChanged` — happiness tick update
- `OnHabitatUpgraded` — habitat level changed
- `OnMissionCompleted` / `OnMissionProgress`
- `OnAchievementUnlocked`
- `OnPurrPowerChanged`
- `OnOfflineProgressReady`
- `OnTutorialStep`
- `OnSceneAreaChanged` — hall level up
- `OnVolunteerAssigned`
- `OnDailyReset`
- `OnGameSaved` / `OnGameLoaded`

---

## Prefab Checklist

Create these prefabs in `Assets/Prefabs/`:

### UI Prefabs
- [ ] `CatCard` — uses `CatCardUI.cs`
- [ ] `HabitatCard` — uses `HabitatCardUI.cs`
- [ ] `HabitatCatSlot` — uses `HabitatCatSlotUI.cs`
- [ ] `MissionCard` — uses `MissionCardUI.cs`
- [ ] `AchievementCard` — uses `AchievementCardUI.cs`
- [ ] `FloatingNumber` — uses `FloatingNumber.cs` + CanvasGroup
- [ ] `NotificationBanner` — uses `NotificationBanner.cs`

### VFX Prefabs
- [ ] `CatRescueParticles` — Particle System, confetti style
- [ ] `HabitatUpgradeParticles` — sparkle burst
- [ ] `PurrPowerParticles` — ambient glow particles

---

## SFX Keys (wire to AudioManager)

| Key               | Trigger                    |
|-------------------|---------------------------|
| `ui_click`        | Any button press           |
| `ui_tab_switch`   | Navigation tab             |
| `ui_back`         | Back button                |
| `ui_error`        | Failed action              |
| `cat_rescue`      | Cat adopted                |
| `habitat_unlock`  | New habitat opened         |
| `habitat_upgrade` | Habitat levelled up        |
| `mission_complete`| Mission reward available   |
| `achievement_unlock` | Achievement popped      |
| `coin_collect`    | Offline earnings collected |
| `tutorial_complete` | Tutorial finished        |
| `main_theme`      | Music track key            |

---

## Steam Integration (Post-Launch)

Achievement IDs follow pattern `ACH_<ID_UPPER>`, e.g. `ACH_FIRST_CAT`.
Wire to Steamworks.NET or Facepunch.Steamworks:

```csharp
// On achievement unlock event:
SteamUserStats.SetAchievement(achievementData.steamAchievementId);
SteamUserStats.StoreStats();
```

---

## Build Settings

**Player Settings:**
- Company: *Your Studio*
- Product Name: Whisker Haven
- Version: 0.1.0
- Default Screen: 1920×1080, resizable
- Standalone: Windows + Mac + Linux

**Quality Settings:** Use URP 2D Renderer profile
**Color Space:** Linear
**Scripting Backend:** IL2CPP (release) / Mono (development)

---

*Built with Unity 6 · C# .NET Standard 2.1 · URP 2D · TextMeshPro*
