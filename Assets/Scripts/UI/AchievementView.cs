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
    public class AchievementView : BaseView
    {
        [SerializeField] public AchievementCardUI cardPrefab;

        private RectTransform    _listContent;
        private TextMeshProUGUI  _unlockedText, _pointsText;
        private Slider           _completionBar;
        private TMP_Dropdown     _categoryFilter;

        protected override void Awake() { base.Awake(); BuildUI(); }
        protected override void OnShow() => Rebuild();

        private void BuildUI()
        {
            var root = Panel(transform, "BG", BG_MED); Stretch(root);

            // Header
            var header = Panel(root.transform, "Header", BG_DARK);
            AnchorTop(header, 56);
            HLayout(header, 12, new RectOffset(16, 16, 8, 8));
            Text(header.transform, "Title", "🏆  Achievements", 20, CREAM, TextAlignmentOptions.Left, FontStyles.Bold);
            var sp = Group(header.transform, "Sp"); LE(sp, flexW: 1);
            _unlockedText = Text(header.transform, "Unlocked", "0/20", 14, GOLD, TextAlignmentOptions.Right, FontStyles.Bold);
            LE(_unlockedText.gameObject, prefW: 60);
            var back = Btn(header.transform, "Back", "← Back", AMBER_D, TEXT_L, 13);
            LE(back.gameObject, prefW: 90, prefH: 38);
            back.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            // Stats bar
            var statsBar = Panel(root.transform, "Stats", new Color(0, 0, 0, 0.3f));
            var srt = statsBar.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 1); srt.anchorMax = new Vector2(1, 1);
            srt.pivot = new Vector2(0.5f, 1); srt.sizeDelta = new Vector2(0, 50);
            srt.anchoredPosition = new Vector2(0, -56);
            VLayout(statsBar, 4, new RectOffset(16, 16, 4, 4));
            _completionBar = SliderH(statsBar.transform, "CompBar", GOLD, 14);
            LE(_completionBar.gameObject, prefH: 14);
            var statsRow = Group(statsBar.transform, "Row");
            HLayout(statsRow, 8, new RectOffset(0, 0, 0, 0));
            Text(statsRow.transform, "PtsLbl", "Points earned:", 12, CREAM_ALT);
            _pointsText = Text(statsRow.transform, "Pts", "0", 12, GOLD, TextAlignmentOptions.Right, FontStyles.Bold);

            // Filter
            var filterBar = Panel(root.transform, "Filter", new Color(0, 0, 0, 0.2f));
            var frt = filterBar.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0, 1); frt.anchorMax = new Vector2(1, 1);
            frt.pivot = new Vector2(0.5f, 1); frt.sizeDelta = new Vector2(0, 36);
            frt.anchoredPosition = new Vector2(0, -106);
            HLayout(filterBar, 12, new RectOffset(16, 16, 4, 4));
            Text(filterBar.transform, "Lbl", "Category:", 12, CREAM_ALT);
            LE(Text(filterBar.transform, "Lbl2", "Category:", 12, CREAM_ALT).gameObject, prefW: 80);
            var catGo = Make(filterBar.transform, "CatFilter");
            LE(catGo, prefW: 200, prefH: 28);
            catGo.AddComponent<Image>().color = BG_LIGHT;
            _categoryFilter = catGo.AddComponent<TMP_Dropdown>();
            _categoryFilter.ClearOptions();
            _categoryFilter.AddOptions(new List<string> { "All", "Collection", "Production", "Exploration", "Social", "Milestone" });
            _categoryFilter.onValueChanged.AddListener(_ => Rebuild());

            // Scroll
            var scrollArea = Group(root.transform, "ScrollArea");
            StretchWithMargin(scrollArea, 142, 0);
            var (_, content) = ScrollV(scrollArea.transform, "AchScroll");
            _listContent = content;
            VLayout(content.gameObject, 8, new RectOffset(12, 12, 12, 12));
            Fitter(content.gameObject);

            EventBus.Subscribe<OnAchievementUnlocked>(_ => { if (gameObject.activeInHierarchy) Rebuild(); });
        }

        private void Rebuild()
        {
            _listContent.DestroyAllChildren();
            var ach = AchievementSystem.Instance;
            var all = ach?.GetAll();
            if (all == null) return;

            int cat = _categoryFilter?.value ?? 0;
            int totalPts = 0, unlockedPts = 0;

            foreach (var data in all)
            {
                if (data == null) continue;
                bool unlocked = ach.IsUnlocked(data.achievementId);
                if (cat > 0 && (int)data.category != cat - 1) continue;
                if (data.isSecret && !unlocked) continue;

                AchievementCardUI card;
                if (cardPrefab != null) card = Instantiate(cardPrefab, _listContent);
                else { var go = Panel(_listContent, data.achievementId, unlocked ? CREAM : CREAM_ALT); LE(go, prefH: 80); card = go.AddComponent<AchievementCardUI>(); }
                card.Populate(data, unlocked);

                totalPts += data.points;
                if (unlocked) unlockedPts += data.points;
            }

            if (_unlockedText) _unlockedText.text = $"{ach?.UnlockedCount ?? 0}/{all.Count}";
            if (_pointsText)   _pointsText.text   = $"{unlockedPts} / {totalPts} pts";
            if (_completionBar && all.Count > 0) _completionBar.value = (float)(ach?.UnlockedCount ?? 0) / all.Count;
        }
    }
}
