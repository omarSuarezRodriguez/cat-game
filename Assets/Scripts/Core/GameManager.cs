using UnityEngine;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.UI;
using WhiskerHaven.Utils;

namespace WhiskerHaven.Core
{
    /// <summary>
    /// Root game manager. Orchestrates init order, binds all systems.
    /// Attach to a persistent "GameManager" GameObject in Bootstrap scene.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

        [Header("Scene References — assigned in Inspector")]
        [SerializeField] private UIManager uiManager;

        public GameConfig Config => gameConfig;
        public SaveData   Save   => SaveSystem.Instance.Current;
        public bool       IsInitialised { get; private set; }

        // ── Boot ─────────────────────────────────────────────────────────────
        private void Start()
        {
            if (gameConfig == null)
            {
                gameConfig = GameConfig.Load();
                if (gameConfig == null)
                {
                    Debug.LogError("[GameManager] GameConfig not found in Resources/Config/. Create it via Assets menu.");
                    return;
                }
            }

            InitSystems();
        }

        private void InitSystems()
        {
            // 1. Save
            var save = SaveSystem.Instance.Load();
            SaveSystem.Instance.SetAutoSaveInterval(gameConfig.autoSaveIntervalSeconds);

            // 2. Resources
            ResourceManager.Instance.Init(save);

            // 3. Data systems
            CatManager.Instance.Init(gameConfig, save);
            HabitatManager.Instance.Init(gameConfig, save);
            VolunteerSystem.Instance.Init(gameConfig, save);

            // 4. Gameplay systems
            PurrPowerSystem.Instance.Init(gameConfig, save);
            MissionSystem.Instance.Init(gameConfig, save);
            AchievementSystem.Instance.Init(gameConfig, save);

            // 5. Idle engine
            IdleManager.Instance.Init(gameConfig, save);

            // 6. Audio
            AudioManager.Instance?.PlayMusic("main_theme");

            // 7. Offline progress
            double secondsOffline = SaveSystem.Instance.GetSecondsOffline();
            IdleManager.Instance.ProcessOfflineProgress(secondsOffline);

            // 8. Hall level check
            CheckHallLevel(save);

            // 9. Achievement initial check
            AchievementSystem.Instance.Check();

            IsInitialised = true;
            Debug.Log("[GameManager] All systems initialised.");

            // 10. UI
            if (uiManager != null)
                uiManager.Init(secondsOffline > 60 || SaveSystem.Instance.HasSave);
            else
            {
                var found = FindFirstObjectByType<UIManager>();
                found?.Init(secondsOffline > 60 || SaveSystem.Instance.HasSave);
            }
        }

        private void CheckHallLevel(SaveData save)
        {
            if (gameConfig.hallLevelThresholds == null) return;
            for (int i = gameConfig.hallLevelThresholds.Length - 1; i >= 0; i--)
            {
                if (save.lifetimeSnuggles >= gameConfig.hallLevelThresholds[i] &&
                    save.hallLevel < i + 1)
                {
                    int prevLevel = save.hallLevel;
                    save.hallLevel = i + 1;
                    Debug.Log($"[GameManager] Hall levelled up to {save.hallLevel}");
                    EventBus.Publish(new OnSceneAreaChanged { AreaId = "main_hall", Level = save.hallLevel });
                    break;
                }
            }
        }

        // Called by IdleManager to update hall level on snuggles gain
        public void TryHallLevelUp()
        {
            CheckHallLevel(Save);
        }
    }
}
