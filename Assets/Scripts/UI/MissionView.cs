using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MissionView : BaseView
    {
        [SerializeField] public MissionCardUI missionCardPrefab;

        private RectTransform    _listContent;
        private TextMeshProUGUI  _timerText, _streakText;
        private float            _timerTick;

        private List<MissionCardUI> _cards = new();

        protected override void Awake() { base.Awake(); BuildUI(); }
        protected override void OnShow() => Rebuild();

        private void BuildUI()
        {
            var root = Panel(transform, "BG", BG_MED); Stretch(root);

            // Header
            var header = Panel(root.transform, "Header", BG_DARK);
            AnchorTop(header, 56);
            HLayout(header, 12, new RectOffset(16, 16, 8, 8));
            Text(header.transform, "Title", "📋  Daily Missions", 20, CREAM, TextAlignmentOptions.Left, FontStyles.Bold);
            var sp = Group(header.transform, "Sp"); LE(sp, flexW: 1);
            _streakText = Text(header.transform, "Streak", "Day 1", 13, GOLD, TextAlignmentOptions.Right);
            LE(_streakText.gameObject, prefW: 80);
            var back = Btn(header.transform, "Back", "← Back", AMBER_D, TEXT_L, 13);
            LE(back.gameObject, prefW: 90, prefH: 38);
            back.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            // Timer bar
            var timerBar = Panel(root.transform, "TimerBar", new Color(0, 0, 0, 0.3f));
            var trt = timerBar.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 1); trt.anchorMax = new Vector2(1, 1);
            trt.pivot = new Vector2(0.5f, 1); trt.sizeDelta = new Vector2(0, 36);
            trt.anchoredPosition = new Vector2(0, -56);
            HLayout(timerBar, 0, new RectOffset(16, 16, 6, 6));
            Text(timerBar.transform, "Lbl", "🕐 Resets in: ", 13, CREAM_ALT);
            _timerText = Text(timerBar.transform, "Timer", "--:--:--", 13, GOLD, TextAlignmentOptions.Left, FontStyles.Bold);

            // Scroll
            var scrollArea = Group(root.transform, "ScrollArea");
            StretchWithMargin(scrollArea, 92, 0);
            var (_, content) = ScrollV(scrollArea.transform, "MissionScroll");
            _listContent = content;
            VLayout(content.gameObject, 12, new RectOffset(16, 16, 16, 16));
            Fitter(content.gameObject);

            EventBus.Subscribe<OnMissionCompleted>(evt => RefreshCard(evt.MissionId));
            EventBus.Subscribe<OnMissionProgress>(evt => RefreshCard(evt.MissionId));
            EventBus.Subscribe<OnDailyReset>(_ => { if (gameObject.activeInHierarchy) Rebuild(); });
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            _timerTick += Time.deltaTime;
            if (_timerTick >= 1f) { _timerTick = 0; UpdateTimer(); }
        }

        private void Rebuild()
        {
            _listContent.DestroyAllChildren();
            _cards.Clear();

            var ms = MissionSystem.Instance;
            if (ms == null) return;

            if (_streakText) _streakText.text = $"Day {GameManager.Instance?.Save?.loginDays ?? 1} 🔥";

            foreach (var id in ms.GetActiveDailyIds())
            {
                var data  = ms.GetData(id);
                var entry = ms.GetEntry(id);
                if (data == null) continue;

                MissionCardUI card;
                if (missionCardPrefab != null)
                    card = Instantiate(missionCardPrefab, _listContent);
                else
                {
                    var go = Panel(_listContent, id, CREAM_ALT);
                    LE(go, prefH: 110);
                    card = go.AddComponent<MissionCardUI>();
                }
                card.Populate(data, entry, () => OnClaim(id));
                _cards.Add(card);
            }

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            if (_timerText == null) return;
            var save = GameManager.Instance?.Save;
            if (save == null || string.IsNullOrEmpty(save.dailyMissionResetTimeUtc)) { _timerText.text = "--:--:--"; return; }
            if (System.DateTime.TryParse(save.dailyMissionResetTimeUtc, out var last))
            {
                var next      = last.Date.AddDays(1);
                var remaining = next - System.DateTime.UtcNow;
                _timerText.text = remaining.TotalSeconds > 0 ? NumberFormatter.FormatTime(remaining.TotalSeconds) : "Refreshing...";
            }
        }

        private void OnClaim(string missionId)
        {
            bool ok = MissionSystem.Instance?.TryClaimReward(missionId) ?? false;
            if (ok) Rebuild();
        }

        private void RefreshCard(string id)
        {
            var entry = MissionSystem.Instance?.GetEntry(id);
            if (entry == null) return;
            foreach (var c in _cards) if (c.MissionId == id) { c.Refresh(entry); break; }
        }
    }
}
