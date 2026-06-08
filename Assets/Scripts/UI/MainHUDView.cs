using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Persistent HUD showing resources, Purr Power bar, and navigation tabs.
    /// </summary>
    public class MainHUDView : BaseView
    {
        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI snugglesText;
        [SerializeField] private TextMeshProUGUI goldenPawText;
        [SerializeField] private TextMeshProUGUI blueprintsText;
        [SerializeField] private TextMeshProUGUI snugglesPerSecText;
        [SerializeField] private Image           snugglesIcon;
        [SerializeField] private Image           goldenPawIcon;
        [SerializeField] private Image           blueprintsIcon;

        [Header("Purr Power")]
        [SerializeField] private Slider          purrPowerSlider;
        [SerializeField] private TextMeshProUGUI purrPowerText;
        [SerializeField] private Image           purrPowerFill;
        [SerializeField] private Gradient        purrPowerGradient;

        [Header("Hall Level")]
        [SerializeField] private TextMeshProUGUI hallLevelText;
        [SerializeField] private Slider          hallProgressSlider;
        [SerializeField] private TextMeshProUGUI hallProgressText;

        [Header("Navigation Tabs")]
        [SerializeField] private Button catsTabBtn;
        [SerializeField] private Button habitatsTabBtn;
        [SerializeField] private Button missionsTabBtn;
        [SerializeField] private Button achievementsTabBtn;
        [SerializeField] private Button settingsTabBtn;

        [Header("Cat Count Badge")]
        [SerializeField] private TextMeshProUGUI catCountText;

        private float _uiUpdateTimer;
        private const float UIUpdateInterval = 0.25f; // 4 updates/sec for perf

        public void Init()
        {
            // Wire navigation buttons
            catsTabBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.ShowCats());
            habitatsTabBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.ShowHabitats());
            missionsTabBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.ShowMissions());
            achievementsTabBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.ShowAchievements());
            settingsTabBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.ShowSettings());

            EventBus.Subscribe<OnPurrPowerChanged>(OnPurrPowerChanged);
            EventBus.Subscribe<OnSceneAreaChanged>(OnHallLevelChanged);

            RefreshAll();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnPurrPowerChanged>(OnPurrPowerChanged);
            EventBus.Unsubscribe<OnSceneAreaChanged>(OnHallLevelChanged);
        }

        // ── Update ────────────────────────────────────────────────────────────
        private void Update()
        {
            _uiUpdateTimer += Time.deltaTime;
            if (_uiUpdateTimer >= UIUpdateInterval)
            {
                _uiUpdateTimer = 0f;
                RefreshResources();
                RefreshSnugglesPerSec();
            }
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            RefreshResources();
            RefreshSnugglesPerSec();
            RefreshPurrPower(PurrPowerSystem.Instance?.CurrentMultiplier ?? 1f);
            RefreshHallLevel();
            RefreshCatCount();
        }

        private void RefreshResources()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;
            if (snugglesText)   snugglesText.text   = NumberFormatter.Format(rm.Snuggles);
            if (goldenPawText)  goldenPawText.text  = NumberFormatter.Format(rm.GoldenPaw);
            if (blueprintsText) blueprintsText.text = NumberFormatter.Format(rm.Blueprints);
        }

        private void RefreshSnugglesPerSec()
        {
            var idle = IdleManager.Instance;
            if (idle == null || snugglesPerSecText == null) return;
            snugglesPerSecText.text = NumberFormatter.Format(idle.SnugglesPerSecond) + "/s";
        }

        private void RefreshPurrPower(float multiplier)
        {
            if (purrPowerSlider != null)
            {
                float t = (multiplier - 1f) / 4f; // map 1–5 → 0–1
                purrPowerSlider.value = t;
                if (purrPowerFill != null)
                    purrPowerFill.color = purrPowerGradient.Evaluate(t);
            }
            if (purrPowerText != null)
                purrPowerText.text = $"×{multiplier:F2}";
        }

        private void RefreshHallLevel()
        {
            var save   = GameManager.Instance?.Save;
            var config = GameManager.Instance?.Config;
            if (save == null || config == null) return;

            if (hallLevelText != null)
                hallLevelText.text = $"Haven Lv.{save.hallLevel}";

            if (hallProgressSlider != null && config.hallLevelThresholds != null)
            {
                int next = save.hallLevel;
                if (next < config.hallLevelThresholds.Length)
                {
                    double cur  = save.hallLevel > 0 ? config.hallLevelThresholds[save.hallLevel - 1] : 0;
                    double req  = config.hallLevelThresholds[next];
                    float  pct  = (float)((save.lifetimeSnuggles - cur) / (req - cur));
                    hallProgressSlider.value = Mathf.Clamp01(pct);
                    if (hallProgressText != null)
                        hallProgressText.text = $"{NumberFormatter.Format(save.lifetimeSnuggles - cur)} / {NumberFormatter.Format(req - cur)}";
                }
                else
                {
                    hallProgressSlider.value = 1f;
                    if (hallProgressText != null) hallProgressText.text = "MAX";
                }
            }
        }

        private void RefreshCatCount()
        {
            var cm = CatManager.Instance;
            if (catCountText != null && cm != null)
                catCountText.text = $"{cm.OwnedCount}/{cm.TotalCats}";
        }

        // ── Events ────────────────────────────────────────────────────────────
        public void UpdateResourceDisplay(ResourceType type, double newAmount)
        {
            switch (type)
            {
                case ResourceType.Snuggles:   if (snugglesText)   snugglesText.text   = NumberFormatter.Format(newAmount); break;
                case ResourceType.GoldenPaw:  if (goldenPawText)  goldenPawText.text  = NumberFormatter.Format(newAmount); break;
                case ResourceType.Blueprints: if (blueprintsText) blueprintsText.text = NumberFormatter.Format(newAmount); break;
            }
        }

        private void OnPurrPowerChanged(OnPurrPowerChanged evt) => RefreshPurrPower(evt.Multiplier);
        private void OnHallLevelChanged(OnSceneAreaChanged evt) { RefreshHallLevel(); RefreshCatCount(); }
    }
}
