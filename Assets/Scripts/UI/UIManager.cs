using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Core;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public enum UIPanel { HUD, CatCollection, HabitatView, MissionView, Settings, WelcomeBack, Tutorial, AchievementView }

    /// <summary>
    /// Root UI controller. Manages panel stack, transitions, and global UI events.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private MainHUDView           hudPanel;
        [SerializeField] private CatCollectionView     catCollectionPanel;
        [SerializeField] private HabitatView           habitatPanel;
        [SerializeField] private MissionView           missionPanel;
        [SerializeField] private AchievementView       achievementPanel;
        [SerializeField] private WelcomeBackView       welcomeBackPanel;
        [SerializeField] private TutorialView          tutorialPanel;
        [SerializeField] private SettingsView          settingsPanel;

        [Header("Global VFX")]
        [SerializeField] private FloatingNumberPool    floatingNumbers;
        [SerializeField] private NotificationBanner    notificationBanner;

        private Stack<UIPanel> _panelStack = new();
        private UIPanel _activePanel = UIPanel.HUD;

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(bool showWelcomeBack)
        {
            hudPanel?.Init();
            catCollectionPanel?.Init();
            habitatPanel?.Init();
            missionPanel?.Init();
            achievementPanel?.Init();
            settingsPanel?.Init();
            floatingNumbers?.Init();

            // Subscribe to events
            EventBus.Subscribe<OnAchievementUnlocked>(OnAchievement);
            EventBus.Subscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Subscribe<OnResourceChanged>(OnResourceChanged);
            EventBus.Subscribe<OnCatRescued>(OnCatRescued);
            EventBus.Subscribe<OnOfflineProgressReady>(OnOfflineProgress);

            // Show entry point
            bool tutorialNeeded = !GameManager.Instance.Save.tutorialComplete;

            SetAllPanelsInactive();
            hudPanel?.gameObject.SetActive(true);

            if (tutorialNeeded && !GameManager.Instance.Config.skipTutorialInEditor)
            {
                ShowTutorial();
            }
            else if (showWelcomeBack)
            {
                // WelcomeBack shown on top of HUD after offline progress calc
                welcomeBackPanel?.gameObject.SetActive(true);
                welcomeBackPanel?.Show();
            }

            _activePanel = UIPanel.HUD;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnAchievementUnlocked>(OnAchievement);
            EventBus.Unsubscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Unsubscribe<OnResourceChanged>(OnResourceChanged);
            EventBus.Unsubscribe<OnCatRescued>(OnCatRescued);
            EventBus.Unsubscribe<OnOfflineProgressReady>(OnOfflineProgress);
        }

        // ── Panel Navigation ─────────────────────────────────────────────────
        public void ShowPanel(UIPanel panel)
        {
            if (_activePanel == panel) return;

            GetPanelView(_activePanel)?.Hide();
            _panelStack.Push(_activePanel);
            _activePanel = panel;
            GetPanelView(panel)?.Show();
            AudioManager.Instance?.PlaySFX("ui_tab_switch");
        }

        public void GoBack()
        {
            if (_panelStack.Count == 0) return;
            GetPanelView(_activePanel)?.Hide();
            _activePanel = _panelStack.Pop();
            GetPanelView(_activePanel)?.Show();
            AudioManager.Instance?.PlaySFX("ui_back");
        }

        public void ShowHUD()      => ShowPanel(UIPanel.HUD);
        public void ShowCats()     => ShowPanel(UIPanel.CatCollection);
        public void ShowHabitats() => ShowPanel(UIPanel.HabitatView);
        public void ShowMissions() => ShowPanel(UIPanel.MissionView);
        public void ShowAchievements() => ShowPanel(UIPanel.AchievementView);
        public void ShowSettings() => ShowPanel(UIPanel.Settings);
        public void ShowTutorial()
        {
            tutorialPanel?.gameObject.SetActive(true);
            tutorialPanel?.StartTutorial();
        }

        private BaseView GetPanelView(UIPanel panel) => panel switch
        {
            UIPanel.HUD             => hudPanel,
            UIPanel.CatCollection   => catCollectionPanel,
            UIPanel.HabitatView     => habitatPanel,
            UIPanel.MissionView     => missionPanel,
            UIPanel.AchievementView => achievementPanel,
            UIPanel.Settings        => settingsPanel,
            _                       => null
        };

        private void SetAllPanelsInactive()
        {
            catCollectionPanel?.gameObject.SetActive(false);
            habitatPanel?.gameObject.SetActive(false);
            missionPanel?.gameObject.SetActive(false);
            achievementPanel?.gameObject.SetActive(false);
            welcomeBackPanel?.gameObject.SetActive(false);
            tutorialPanel?.gameObject.SetActive(false);
            settingsPanel?.gameObject.SetActive(false);
        }

        // ── Floating Numbers ──────────────────────────────────────────────────
        public void SpawnFloatingNumber(Vector3 worldPos, double amount, bool isPositive = true)
        {
            floatingNumbers?.Spawn(worldPos, amount, isPositive);
        }

        // ── Event Handlers ────────────────────────────────────────────────────
        private void OnAchievement(OnAchievementUnlocked evt)
        {
            var data = AchievementSystem.Instance?.GetAll()
                .Find(a => a.achievementId == evt.AchievementId);
            notificationBanner?.Show($"Achievement: {data?.achievementName ?? evt.AchievementId}", NotificationType.Achievement);
        }

        private void OnMissionComplete(OnMissionCompleted evt)
        {
            var data = MissionSystem.Instance?.GetData(evt.MissionId);
            notificationBanner?.Show($"Mission complete: {data?.missionName ?? evt.MissionId}", NotificationType.Mission);
        }

        private void OnResourceChanged(OnResourceChanged evt)
        {
            hudPanel?.UpdateResourceDisplay(evt.Type, evt.NewAmount);
        }

        private void OnCatRescued(OnCatRescued evt)
        {
            var data = CatManager.Instance?.GetData(evt.CatId);
            notificationBanner?.Show($"{data?.catName ?? evt.CatId} joined the haven!", NotificationType.CatRescue);
        }

        private void OnOfflineProgress(OnOfflineProgressReady evt)
        {
            welcomeBackPanel?.SetData(evt.SnugglesEarned, evt.TimeAwaySeconds);
        }
    }
}
