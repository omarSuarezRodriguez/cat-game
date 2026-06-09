using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CatCollectionView : BaseView
    {
        // Prefab injected by AutoSetup
        [SerializeField] public CatCardUI catCardPrefab;

        // Runtime refs
        private RectTransform   _gridContent;
        private ScrollRect      _scroll;
        private GameObject      _detailPanel;
        private Image           _portrait;
        private TextMeshProUGUI _detailName, _detailRarity, _detailDesc, _detailProd, _detailHappText;
        private Slider          _happSlider;
        private Button          _rescueBtn, _assignBtn, _closeBtn;
        private TextMeshProUGUI _rescueCostText;
        private TextMeshProUGUI _ownedCountText;
        private TMP_Dropdown    _rarityFilter;

        private List<CatCardUI> _cards     = new();
        private CatData         _selected;

        protected override void Awake()
        {
            base.Awake();
            BuildUI();
        }

        protected override void OnShow()
        {
            RebuildGrid();
            RefreshStats();
        }

        protected override void OnHide() => _detailPanel?.SetActive(false);

        // ── Build ─────────────────────────────────────────────────────────────
        private void BuildUI()
        {
            var root = Panel(transform, "BG", BG_MED);
            Stretch(root);

            // Header
            var header = Panel(root.transform, "Header", BG_DARK);
            AnchorTop(header, 56);
            HLayout(header, 12, new RectOffset(16, 16, 8, 8));

            Text(header.transform, "Title", "🐱  Cat Collection", 20, CREAM, TextAlignmentOptions.Left, FontStyles.Bold);
            var spacer = Group(header.transform, "Spacer"); LE(spacer, flexW: 1);
            _ownedCountText = Text(header.transform, "OwnedCount", "0 / 15", 15, GOLD, TextAlignmentOptions.Right, FontStyles.Bold);
            LE(_ownedCountText.gameObject, prefW: 80);

            // Filter bar
            var filterBar = Panel(root.transform, "FilterBar", new Color(0, 0, 0, 0.3f));
            var filterRt = filterBar.GetComponent<RectTransform>();
            filterRt.anchorMin = new Vector2(0, 1); filterRt.anchorMax = new Vector2(1, 1);
            filterRt.pivot = new Vector2(0.5f, 1);
            filterRt.sizeDelta = new Vector2(0, 40);
            filterRt.anchoredPosition = new Vector2(0, -56);
            HLayout(filterBar, 12, new RectOffset(16, 16, 4, 4));

            Text(filterBar.transform, "FilterLbl", "Filter:", 13, CREAM_ALT);
            LE(Text(filterBar.transform, "FilterLbl", "Filter:", 13, CREAM_ALT).gameObject, prefW: 50);

            // Rarity dropdown (manual)
            var rarityGo = Make(filterBar.transform, "RarityFilter");
            LE(rarityGo, prefW: 150, prefH: 32);
            var rarityImg = rarityGo.AddComponent<Image>(); rarityImg.color = BG_LIGHT;
            _rarityFilter = rarityGo.AddComponent<TMP_Dropdown>();
            _rarityFilter.ClearOptions();
            _rarityFilter.AddOptions(new List<string> { "All", "Common", "Uncommon", "Rare", "Epic", "Legendary" });
            _rarityFilter.onValueChanged.AddListener(_ => RebuildGrid());

            var closeFilter = Group(filterBar.transform, "Spacer2"); LE(closeFilter, flexW: 1);

            // Back button
            var backBtn = Btn(filterBar.transform, "BackBtn", "← Back", AMBER_D, TEXT_L, 13);
            LE(backBtn.gameObject, prefW: 90, prefH: 32);
            backBtn.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            // Scroll area (left 2/3)
            var mainArea = Group(root.transform, "MainArea");
            StretchWithMargin(mainArea, 96, 0);
            HLayout(mainArea, 0, new RectOffset(0, 0, 0, 0));

            var scrollArea = Group(mainArea.transform, "ScrollArea");
            LE(scrollArea, flexW: 2);
            var (scroll, content) = ScrollV(scrollArea.transform, "CatScroll", new Color(0, 0, 0, 0.2f));
            _scroll      = scroll;
            _gridContent = content;
            Grid(content.gameObject, new Vector2(150, 190), 10);
            Fitter(content.gameObject);

            // Detail panel (right 1/3)
            BuildDetailPanel(mainArea.transform);

            EventBus.Subscribe<OnCatRescued>(_ => { if (gameObject.activeInHierarchy) { RebuildGrid(); RefreshStats(); } });
            EventBus.Subscribe<OnCatHappinessChanged>(OnHappChanged);
        }

        private void BuildDetailPanel(Transform parent)
        {
            _detailPanel = Panel(parent, "DetailPanel", CREAM);
            LE(_detailPanel, prefW: 280);
            _detailPanel.SetActive(false);
            VLayout(_detailPanel, 10, new RectOffset(16, 16, 16, 16));

            // Portrait
            var portraitGo = Panel(_detailPanel.transform, "Portrait", new Color(0.8f, 0.8f, 0.8f, 0.5f));
            LE(portraitGo, prefH: 160, minH: 160);
            _portrait = portraitGo.GetComponent<Image>();

            // Name + Rarity
            _detailName   = Text(_detailPanel.transform, "CatName", "Mittens", 20, TEXT_D, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_detailName.gameObject, prefH: 28);
            _detailRarity = Text(_detailPanel.transform, "Rarity", "Common", 13, COMMON, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_detailRarity.gameObject, prefH: 20);

            // Description
            _detailDesc = Text(_detailPanel.transform, "Desc", "", 12, TEXT_D, TextAlignmentOptions.Left);
            _detailDesc.overflowMode = TextOverflowModes.Overflow;
            _detailDesc.enableWordWrapping = true;
            LE(_detailDesc.gameObject, prefH: 60);

            // Production
            _detailProd = Text(_detailPanel.transform, "Production", "Production: 0/s", 13, BG_LIGHT, TextAlignmentOptions.Left);
            LE(_detailProd.gameObject, prefH: 20);

            // Happiness
            _detailHappText = Text(_detailPanel.transform, "Happiness", "Happiness: 50/100", 13, BG_LIGHT);
            LE(_detailHappText.gameObject, prefH: 20);
            _happSlider = SliderH(_detailPanel.transform, "HappBar", SUCCESS, 16);
            LE(_happSlider.gameObject, prefH: 16);

            // Cost
            _rescueCostText = Text(_detailPanel.transform, "Cost", "", 13, TEXT_D, TextAlignmentOptions.Center);
            LE(_rescueCostText.gameObject, prefH: 20);

            // Buttons
            _rescueBtn = Btn(_detailPanel.transform, "RescueBtn", "🐾 Rescue", SUCCESS, TEXT_L, 14);
            LE(_rescueBtn.gameObject, prefH: 40);
            _rescueBtn.onClick.AddListener(OnRescueClicked);

            _assignBtn = Btn(_detailPanel.transform, "AssignBtn", "🏠 Assign Habitat", AMBER, TEXT_L, 13);
            LE(_assignBtn.gameObject, prefH: 40);
            _assignBtn.onClick.AddListener(OnAssignClicked);

            _closeBtn = Btn(_detailPanel.transform, "CloseBtn", "✕ Close", AMBER_D, TEXT_L, 12);
            LE(_closeBtn.gameObject, prefH: 36);
            _closeBtn.onClick.AddListener(() => _detailPanel.SetActive(false));
        }

        // ── Grid ──────────────────────────────────────────────────────────────
        private void RebuildGrid()
        {
            _gridContent.DestroyAllChildren();
            _cards.Clear();

            var cm      = CatManager.Instance;
            var allCats = cm?.GetAllCats();
            if (allCats == null) return;

            int rarityIdx = _rarityFilter?.value ?? 0;

            foreach (var data in allCats)
            {
                if (data == null) continue;
                if (rarityIdx > 0 && (int)data.rarity != rarityIdx - 1) continue;

                bool owned = cm.IsOwned(data.catId);
                CatCardUI card;

                if (catCardPrefab != null)
                    card = Instantiate(catCardPrefab, _gridContent);
                else
                    card = BuildInlineCard(data, owned);

                card.Populate(data, owned, () => OnCardClicked(data));
                _cards.Add(card);
            }
        }

        // Inline card if no prefab assigned
        private CatCardUI BuildInlineCard(CatData data, bool owned)
        {
            var go    = Panel(_gridContent, data.catId, owned ? CREAM : new Color(0.5f, 0.5f, 0.5f, 0.5f));
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 190);
            VLayout(go, 4, new RectOffset(8, 8, 8, 8));

            var card = go.AddComponent<CatCardUI>();
            return card;
        }

        private void RefreshStats()
        {
            var cm = CatManager.Instance;
            if (_ownedCountText && cm != null) _ownedCountText.text = $"{cm.OwnedCount} / {cm.TotalCats}";
        }

        // ── Detail ────────────────────────────────────────────────────────────
        private void OnCardClicked(CatData data)
        {
            _selected = data;
            CatManager.Instance?.MarkCatSeen(data.catId);
            ShowDetail(data);
            AudioManager.Instance?.PlaySFX("ui_click");
        }

        private void ShowDetail(CatData data)
        {
            _detailPanel?.SetActive(true);

            bool owned = CatManager.Instance?.IsOwned(data.catId) ?? false;
            var  entry = CatManager.Instance?.GetEntry(data.catId);

            if (_portrait)     _portrait.sprite = data.portrait;
            if (_detailName)   _detailName.text  = data.catName;
            if (_detailRarity) { _detailRarity.text = data.rarity.ToString(); _detailRarity.color = RarityColor(data.rarity); }
            if (_detailDesc)   _detailDesc.text  = data.description;
            if (_detailProd)   _detailProd.text  = $"🐾 {NumberFormatter.Format(data.GetProductionForLevel(entry?.level ?? 1))}/s";

            float happy = entry?.happiness ?? 0;
            if (_happSlider) { _happSlider.gameObject.SetActive(owned); _happSlider.value = happy / data.maxHappiness; }
            if (_detailHappText) { _detailHappText.gameObject.SetActive(owned); _detailHappText.text = $"Happiness: {happy:F0}/{data.maxHappiness:F0}"; }

            bool canAfford = ResourceManager.Instance?.CanAfford(data.rescueCostSnuggles, data.rescueCostGoldenPaw) ?? false;
            if (_rescueBtn) { _rescueBtn.gameObject.SetActive(!owned); _rescueBtn.interactable = canAfford; }
            if (_rescueCostText)
            {
                _rescueCostText.gameObject.SetActive(!owned);
                string cost = data.rescueCostSnuggles > 0 ? $"🐾 {NumberFormatter.Format(data.rescueCostSnuggles)}" : "";
                if (data.rescueCostGoldenPaw > 0) cost += $"  ✨ {NumberFormatter.Format(data.rescueCostGoldenPaw)}";
                _rescueCostText.text = cost.Trim();
            }
            if (_assignBtn) _assignBtn.gameObject.SetActive(owned);
        }

        private void OnRescueClicked()
        {
            if (_selected == null) return;
            bool ok = CatManager.Instance?.TryRescueCat(_selected.catId) ?? false;
            if (ok) ShowDetail(_selected);
            else    AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnAssignClicked()
        {
            if (_selected == null) return;
            // Assign to first available unlocked habitat with free slot
            var save = GameManager.Instance?.Save;
            if (save?.habitats == null) return;
            foreach (var h in save.habitats)
            {
                if (!h.isUnlocked) continue;
                var hData = HabitatManager.Instance?.GetHabitatData(h.habitatId);
                if (hData == null) continue;
                int maxSlots = hData.GetLevel(h.level).maxCatSlots;
                if (h.catIds.Count < maxSlots)
                {
                    bool ok = CatManager.Instance?.TryAssignToHabitat(_selected.catId, h.habitatId) ?? false;
                    if (ok) { ShowDetail(_selected); return; }
                }
            }
            AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnHappChanged(OnCatHappinessChanged evt)
        {
            if (_selected?.catId != evt.CatId || !(_detailPanel?.activeSelf ?? false)) return;
            var entry = CatManager.Instance?.GetEntry(evt.CatId);
            if (entry != null && _selected != null)
            {
                if (_happSlider) _happSlider.value = entry.happiness / _selected.maxHappiness;
                if (_detailHappText) _detailHappText.text = $"Happiness: {entry.happiness:F0}/{_selected.maxHappiness:F0}";
            }
        }
    }
}
