using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Gameplay;

namespace WhiskerHaven.UI
{
    public enum UIPanel { HUD, CatCollection, HabitatView, MissionView, AchievementView, Settings }

    /// <summary>
    /// Root UI orchestrator. Auto-discovers panel views from children.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // Auto-discovered — no Inspector wiring needed
        private MainHUDView        _hud;
        private CatCollectionView  _cats;
        private HabitatView        _habitats;
        private MissionView        _missions;
        private AchievementView    _achievements;
        private SettingsView       _settings;
        private WelcomeBackView    _welcomeBack;
        private TutorialView       _tutorial;
        private NotificationBanner _banner;
        private FloatingNumberPool _floatingPool;

        private UIPanel             _active = UIPanel.HUD;
        private Stack<UIPanel>      _stack  = new();

        private void Awake()
        {
            // Discover panels (searched in entire canvas children, including inactive)
            _hud          = GetComponentInChildren<MainHUDView>(true);
            _cats         = GetComponentInChildren<CatCollectionView>(true);
            _habitats     = GetComponentInChildren<HabitatView>(true);
            _missions     = GetComponentInChildren<MissionView>(true);
            _achievements = GetComponentInChildren<AchievementView>(true);
            _settings     = GetComponentInChildren<SettingsView>(true);
            _welcomeBack  = GetComponentInChildren<WelcomeBackView>(true);
            _tutorial     = GetComponentInChildren<TutorialView>(true);
            _banner       = GetComponentInChildren<NotificationBanner>(true);
            _floatingPool = GetComponentInChildren<FloatingNumberPool>(true);
        }

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(bool showWelcomeBack)
        {
            EventBus.Subscribe<OnAchievementUnlocked>(OnAchievement);
            EventBus.Subscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Subscribe<OnResourceChanged>(OnResourceChanged);
            EventBus.Subscribe<OnCatRescued>(OnCatRescued);
            EventBus.Subscribe<OnOfflineProgressReady>(OnOfflineProgress);

            // Deactivate non-HUD panels
            _cats?.gameObject.SetActive(false);
            _habitats?.gameObject.SetActive(false);
            _missions?.gameObject.SetActive(false);
            _achievements?.gameObject.SetActive(false);
            _settings?.gameObject.SetActive(false);

            _hud?.gameObject.SetActive(true);
            _hud?.Init(this);

            _floatingPool?.Init();

            bool needTutorial = !GameManager.Instance.Save.tutorialComplete;

            if (needTutorial && !GameManager.Instance.Config.skipTutorialInEditor)
            {
                _tutorial?.gameObject.SetActive(true);
                _tutorial?.StartTutorial();
            }
            else if (showWelcomeBack)
            {
                _welcomeBack?.gameObject.SetActive(true);
                _welcomeBack?.Show();
            }
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnAchievementUnlocked>(OnAchievement);
            EventBus.Unsubscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Unsubscribe<OnResourceChanged>(OnResourceChanged);
            EventBus.Unsubscribe<OnCatRescued>(OnCatRescued);
            EventBus.Unsubscribe<OnOfflineProgressReady>(OnOfflineProgress);
        }

        // ── Navigation ────────────────────────────────────────────────────────
        public void ShowPanel(UIPanel panel)
        {
            if (_active == panel) { GetView(panel)?.Show(); return; }

            GetView(_active)?.Hide();
            _stack.Push(_active);
            _active = panel;

            var view = GetView(panel);
            if (view != null)
            {
                view.gameObject.SetActive(true);
                view.Show();
            }
            AudioManager.Instance?.PlaySFX("ui_tab_switch");
        }

        public void GoBack()
        {
            if (_stack.Count == 0) return;
            GetView(_active)?.Hide();
            _active = _stack.Pop();
            GetView(_active)?.Show();
            AudioManager.Instance?.PlaySFX("ui_back");
        }

        public void ShowHUD()          => ShowPanel(UIPanel.HUD);
        public void ShowCats()         => ShowPanel(UIPanel.CatCollection);
        public void ShowHabitats()     => ShowPanel(UIPanel.HabitatView);
        public void ShowMissions()     => ShowPanel(UIPanel.MissionView);
        public void ShowAchievements() => ShowPanel(UIPanel.AchievementView);
        public void ShowSettings()     => ShowPanel(UIPanel.Settings);

        private BaseView GetView(UIPanel p) => p switch
        {
            UIPanel.HUD             => _hud,
            UIPanel.CatCollection   => _cats,
            UIPanel.HabitatView     => _habitats,
            UIPanel.MissionView     => _missions,
            UIPanel.AchievementView => _achievements,
            UIPanel.Settings        => _settings,
            _ => null
        };

        // ── Floating Numbers ─────────────────────────────────────────────────
        public void SpawnFloatingNumber(Vector3 worldPos, double amount, bool positive = true)
            => _floatingPool?.Spawn(worldPos, amount, positive);

        // ── Event Handlers ────────────────────────────────────────────────────
        private void OnAchievement(OnAchievementUnlocked evt)
        {
            var d = AchievementSystem.Instance?.GetAll()?.Find(a => a.achievementId == evt.AchievementId);
            _banner?.Show($"🏆 {d?.achievementName ?? evt.AchievementId}", NotificationType.Achievement);
        }

        private void OnMissionComplete(OnMissionCompleted evt)
        {
            var d = MissionSystem.Instance?.GetData(evt.MissionId);
            _banner?.Show($"✅ Mission complete: {d?.missionName ?? evt.MissionId}", NotificationType.Mission);
        }

        private void OnResourceChanged(OnResourceChanged evt)
            => _hud?.UpdateResource(evt.Type, evt.NewAmount);

        private void OnCatRescued(OnCatRescued evt)
        {
            var d = CatManager.Instance?.GetData(evt.CatId);
            _banner?.Show($"🐱 {d?.catName ?? evt.CatId} joined the haven!", NotificationType.CatRescue);
        }

        private void OnOfflineProgress(OnOfflineProgressReady evt)
            => _welcomeBack?.SetData(evt.SnugglesEarned, evt.TimeAwaySeconds);
    }
}
