using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Core;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Persistent HUD — builds its own UI in Awake, no Inspector wiring needed.
    /// Layout: top resource bar | center haven info | bottom nav tabs
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class MainHUDView : BaseView
    {
        // Runtime refs — populated in BuildUI()
        private TextMeshProUGUI _snugglesText, _goldenPawText, _blueprintsText, _spsText;
        private Slider          _purrSlider;
        private TextMeshProUGUI _purrText;
        private Image           _purrFill;
        private TextMeshProUGUI _hallLevelText, _hallProgressText, _catCountText;
        private Slider          _hallSlider;

        private UIManager _ui;
        private float     _tick;
        private const float TICK_INTERVAL = 0.33f;

        public void Init(UIManager ui)
        {
            _ui = ui;
            BuildUI();
            EventBus.Subscribe<OnPurrPowerChanged>(e => RefreshPurrPower(e.Multiplier));
            EventBus.Subscribe<OnSceneAreaChanged>(_ => RefreshHall());
            RefreshAll();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnPurrPowerChanged>(e => RefreshPurrPower(e.Multiplier));
            EventBus.Unsubscribe<OnSceneAreaChanged>(_ => RefreshHall());
        }

        private void Update()
        {
            _tick += Time.deltaTime;
            if (_tick >= TICK_INTERVAL) { _tick = 0; RefreshResources(); RefreshSPS(); }
        }

        // ── Build UI ─────────────────────────────────────────────────────────
        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>();

            // ── TOP BAR ──────────────────────────────────────────────────────
            var topBar = Panel(transform, "TopBar", BG_DARK);
            AnchorTop(topBar, 72);
            HLayout(topBar, 12, new RectOffset(16, 16, 8, 8));

            // Snuggles
            var snugglesGroup = Group(topBar.transform, "Snuggles");
            LE(snugglesGroup, prefW: 200, flexW: 1);
            HLayout(snugglesGroup, 6, new RectOffset(0, 0, 0, 0));
            Text(snugglesGroup.transform, "Icon", "🐾", 22, TEXT_L, TextAlignmentOptions.Center).raycastTarget = false;
            var snugglesCol = Group(snugglesGroup.transform, "Col");
            VLayout(snugglesCol, 0, new RectOffset(0, 0, 0, 0));
            _snugglesText = Text(snugglesCol.transform, "Amount", "0", 18, GOLD, TextAlignmentOptions.Left, FontStyles.Bold);
            _spsText      = Text(snugglesCol.transform, "SPS", "0/s",  11, new Color(1, 1, 1, 0.6f));

            // Separator
            var sep1 = Panel(topBar.transform, "Sep", new Color(1, 1, 1, 0.15f));
            LE(sep1, minW: 2, prefW: 2);

            // Golden Paw
            var gpGroup = Group(topBar.transform, "GoldenPaw");
            LE(gpGroup, prefW: 160, flexW: 1);
            HLayout(gpGroup, 6, new RectOffset(0, 0, 0, 0));
            Text(gpGroup.transform, "Icon", "✨", 22, TEXT_L, TextAlignmentOptions.Center).raycastTarget = false;
            _goldenPawText = Text(gpGroup.transform, "Amount", "0", 18, GOLD, TextAlignmentOptions.Left, FontStyles.Bold);

            // Separator
            var sep2 = Panel(topBar.transform, "Sep2", new Color(1, 1, 1, 0.15f));
            LE(sep2, minW: 2, prefW: 2);

            // Blueprints
            var bpGroup = Group(topBar.transform, "Blueprints");
            LE(bpGroup, prefW: 160, flexW: 1);
            HLayout(bpGroup, 6, new RectOffset(0, 0, 0, 0));
            Text(bpGroup.transform, "Icon", "📐", 22, TEXT_L, TextAlignmentOptions.Center).raycastTarget = false;
            _blueprintsText = Text(bpGroup.transform, "Amount", "0", 18, TEXT_L, TextAlignmentOptions.Left, FontStyles.Bold);

            // Spacer
            var spacer = Group(topBar.transform, "Spacer");
            LE(spacer, flexW: 1);

            // Purr Power
            var purrGroup = Group(topBar.transform, "PurrPower");
            LE(purrGroup, prefW: 200);
            VLayout(purrGroup, 2, new RectOffset(0, 0, 4, 4));
            var purrLabel = Group(purrGroup.transform, "PurrLabel");
            HLayout(purrLabel, 4, new RectOffset(0, 0, 0, 0));
            Text(purrLabel.transform, "Lbl", "✨ Purr Power", 11, new Color(1, 1, 1, 0.7f));
            _purrText = Text(purrLabel.transform, "Mult", "×1.00", 12, PURR_CLR, TextAlignmentOptions.Right, FontStyles.Bold);
            _purrSlider = SliderH(purrGroup.transform, "PurrBar", PURR_CLR, 14);
            LE(_purrSlider.gameObject, prefH: 14);

            // ── CENTER AREA ───────────────────────────────────────────────────
            var center = Panel(transform, "CenterArea", new Color(0.12f, 0.07f, 0.04f, 0.95f));
            StretchWithMargin(center, 72, 64);
            VLayout(center, 16, new RectOffset(32, 32, 24, 24));

            // Haven title
            Text(center.transform, "HavenTitle", "~ Whisker Haven ~", 28, CREAM_ALT, TextAlignmentOptions.Center, FontStyles.Bold | FontStyles.Italic);

            // Hall level
            _hallLevelText = Text(center.transform, "HallLevel", "Haven Lv.1", 20, GOLD, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_hallLevelText.gameObject, prefH: 28);

            // Hall progress bar + text
            var hallProgressGroup = Group(center.transform, "HallProgress");
            VLayout(hallProgressGroup, 4, new RectOffset(0, 0, 0, 0));
            LE(hallProgressGroup, prefH: 40);
            _hallSlider = SliderH(hallProgressGroup.transform, "HallBar", AMBER, 18);
            LE(_hallSlider.gameObject, prefH: 18);
            _hallProgressText = Text(hallProgressGroup.transform, "HallProgressText", "0 / 500", 11, new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Center);
            LE(_hallProgressText.gameObject, prefH: 16);

            // Cat count
            var catCountGroup = Group(center.transform, "CatCount");
            HLayout(catCountGroup, 8, new RectOffset(0, 0, 0, 0));
            LE(catCountGroup, prefH: 28);
            Text(catCountGroup.transform, "Lbl", "🐱 Cats rescued:", 15, CREAM, TextAlignmentOptions.Right);
            _catCountText = Text(catCountGroup.transform, "Count", "0 / 15", 15, GOLD, TextAlignmentOptions.Left, FontStyles.Bold);

            // Big action hint
            Text(center.transform, "Hint", "Use the buttons below to manage your sanctuary", 13, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.Center);

            // ── BOTTOM NAV ────────────────────────────────────────────────────
            var navBar = Panel(transform, "NavBar", BG_DARK);
            AnchorBot(navBar, 64);
            HLayout(navBar, 4, new RectOffset(8, 8, 8, 8));

            NavBtn(navBar.transform, "🐱\nCats",        () => _ui?.ShowCats());
            NavBtn(navBar.transform, "🏠\nHabitats",    () => _ui?.ShowHabitats());
            NavBtn(navBar.transform, "📋\nMissions",    () => _ui?.ShowMissions());
            NavBtn(navBar.transform, "🏆\nAchievements",() => _ui?.ShowAchievements());
            NavBtn(navBar.transform, "⚙️\nSettings",   () => _ui?.ShowSettings());
        }

        private void NavBtn(Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var btn = Btn(parent, label.Split('\n')[0], label, BG_MED, TEXT_L, 11f);
            btn.onClick.AddListener(action);
            var le = btn.gameObject.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minHeight = 48;
        }

        // ── Refresh ───────────────────────────────────────────────────────────
        private void RefreshAll()
        {
            RefreshResources();
            RefreshSPS();
            RefreshPurrPower(PurrPowerSystem.Instance?.CurrentMultiplier ?? 1f);
            RefreshHall();
            RefreshCatCount();
        }

        private void RefreshResources()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;
            if (_snugglesText)   _snugglesText.text   = NumberFormatter.Format(rm.Snuggles);
            if (_goldenPawText)  _goldenPawText.text  = NumberFormatter.Format(rm.GoldenPaw);
            if (_blueprintsText) _blueprintsText.text = NumberFormatter.Format(rm.Blueprints);
            RefreshCatCount();
        }

        private void RefreshSPS()
        {
            if (_spsText && IdleManager.Instance != null)
                _spsText.text = NumberFormatter.Format(IdleManager.Instance.SnugglesPerSecond) + "/s";
        }

        private void RefreshPurrPower(float mult)
        {
            if (_purrSlider) _purrSlider.value = Mathf.Clamp01((mult - 1f) / 4f);
            if (_purrText)   _purrText.text    = $"×{mult:F2}";
        }

        private void RefreshHall()
        {
            var save   = GameManager.Instance?.Save;
            var config = GameManager.Instance?.Config;
            if (save == null || config == null) return;

            if (_hallLevelText) _hallLevelText.text = $"Haven  Lv.{save.hallLevel}";

            if (config.hallLevelThresholds != null && _hallSlider)
            {
                int  next = save.hallLevel;
                if (next < config.hallLevelThresholds.Length)
                {
                    double cur = next > 0 ? config.hallLevelThresholds[next - 1] : 0;
                    double req = config.hallLevelThresholds[next];
                    _hallSlider.value = Mathf.Clamp01((float)((save.lifetimeSnuggles - cur) / (req - cur)));
                    if (_hallProgressText)
                        _hallProgressText.text = $"{NumberFormatter.Format(save.lifetimeSnuggles - cur)} / {NumberFormatter.Format(req - cur)} Snuggles";
                }
                else
                {
                    _hallSlider.value = 1f;
                    if (_hallProgressText) _hallProgressText.text = "MAX LEVEL";
                }
            }
        }

        private void RefreshCatCount()
        {
            var cm = CatManager.Instance;
            if (_catCountText && cm != null) _catCountText.text = $"{cm.OwnedCount} / {cm.TotalCats}";
        }

        public void UpdateResource(ResourceType type, double newAmount)
        {
            switch (type)
            {
                case ResourceType.Snuggles:   if (_snugglesText)   _snugglesText.text   = NumberFormatter.Format(newAmount); break;
                case ResourceType.GoldenPaw:  if (_goldenPawText)  _goldenPawText.text  = NumberFormatter.Format(newAmount); break;
                case ResourceType.Blueprints: if (_blueprintsText) _blueprintsText.text = NumberFormatter.Format(newAmount); break;
            }
            RefreshCatCount();
        }
    }
}
