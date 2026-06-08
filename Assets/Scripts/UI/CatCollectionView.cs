using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class CatCollectionView : BaseView
    {
        [Header("Grid")]
        [SerializeField] private Transform        catGridParent;
        [SerializeField] private CatCardUI        catCardPrefab;
        [SerializeField] private ScrollRect       scrollRect;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown     rarityFilter;
        [SerializeField] private TMP_InputField   searchInput;
        [SerializeField] private Toggle           showOwnedToggle;

        [Header("Detail Panel")]
        [SerializeField] private GameObject       detailPanel;
        [SerializeField] private Image            detailPortrait;
        [SerializeField] private TextMeshProUGUI  detailName;
        [SerializeField] private TextMeshProUGUI  detailRarity;
        [SerializeField] private TextMeshProUGUI  detailDescription;
        [SerializeField] private TextMeshProUGUI  detailProductionText;
        [SerializeField] private TextMeshProUGUI  detailHappinessText;
        [SerializeField] private Slider           happinessSlider;
        [SerializeField] private Button           rescueButton;
        [SerializeField] private TextMeshProUGUI  rescueCostText;
        [SerializeField] private Button           assignButton;
        [SerializeField] private Button           closeDetailBtn;

        [Header("Stats Banner")]
        [SerializeField] private TextMeshProUGUI  ownedCountText;
        [SerializeField] private TextMeshProUGUI  totalCatsText;

        private List<CatCardUI> _cards = new();
        private CatData _selectedCat;

        public void Init()
        {
            EventBus.Subscribe<OnCatRescued>(OnCatRescued);
            EventBus.Subscribe<OnCatHappinessChanged>(OnHappinessChanged);

            rarityFilter?.onValueChanged.AddListener(_ => RebuildGrid());
            searchInput?.onValueChanged.AddListener(_ => RebuildGrid());
            showOwnedToggle?.onValueChanged.AddListener(_ => RebuildGrid());
            closeDetailBtn?.onClick.AddListener(CloseDetail);
            rescueButton?.onClick.AddListener(OnRescueClicked);
            assignButton?.onClick.AddListener(OnAssignClicked);

            detailPanel?.SetActive(false);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnCatRescued>(OnCatRescued);
            EventBus.Unsubscribe<OnCatHappinessChanged>(OnHappinessChanged);
        }

        protected override void OnShow()
        {
            RebuildGrid();
            RefreshStats();
        }

        // ── Grid ──────────────────────────────────────────────────────────────
        private void RebuildGrid()
        {
            catGridParent.DestroyAllChildren();
            _cards.Clear();

            var cm      = CatManager.Instance;
            var allCats = cm?.GetAllCats();
            if (allCats == null) return;

            bool showOwned  = showOwnedToggle == null || showOwnedToggle.isOn;
            int  rarityIdx  = rarityFilter?.value ?? 0; // 0=All
            string search   = searchInput?.text.ToLower() ?? "";

            foreach (var data in allCats)
            {
                if (data == null) continue;

                bool owned = cm.IsOwned(data.catId);
                if (showOwned && !owned) continue;

                if (rarityIdx > 0 && (int)data.rarity != rarityIdx - 1) continue;

                if (!string.IsNullOrEmpty(search) &&
                    !data.catName.ToLower().Contains(search)) continue;

                var card = Instantiate(catCardPrefab, catGridParent);
                card.Populate(data, owned, () => OnCardClicked(data));
                _cards.Add(card);
            }
        }

        private void RefreshStats()
        {
            var cm = CatManager.Instance;
            if (cm == null) return;
            if (ownedCountText) ownedCountText.text = cm.OwnedCount.ToString();
            if (totalCatsText)  totalCatsText.text  = cm.TotalCats.ToString();
        }

        // ── Detail Panel ───────────────────────────────────────────────────────
        private void OnCardClicked(CatData data)
        {
            _selectedCat = data;
            CatManager.Instance?.MarkCatSeen(data.catId);
            ShowDetail(data);
            AudioManager.Instance?.PlaySFX("ui_click");
        }

        private void ShowDetail(CatData data)
        {
            detailPanel?.SetActive(true);
            if (detailPortrait)    detailPortrait.sprite = data.portrait;
            if (detailName)        detailName.text       = data.catName;
            if (detailRarity)
            {
                detailRarity.text  = data.rarity.ToString().ToUpper();
                detailRarity.color = HexToColor(data.GetRarityColor());
            }
            if (detailDescription) detailDescription.text = data.description;

            bool owned = CatManager.Instance?.IsOwned(data.catId) ?? false;
            var entry  = CatManager.Instance?.GetEntry(data.catId);

            // Production
            int level = owned && entry != null ? entry.level : 1;
            if (detailProductionText)
                detailProductionText.text = $"Production: {NumberFormatter.Format(data.GetProductionForLevel(level))}/s";

            // Happiness
            float happiness = entry?.happiness ?? 0f;
            if (happinessSlider)
            {
                happinessSlider.gameObject.SetActive(owned);
                happinessSlider.value = happiness / data.maxHappiness;
            }
            if (detailHappinessText)
            {
                detailHappinessText.gameObject.SetActive(owned);
                detailHappinessText.text = $"Happiness: {happiness:F0}/{data.maxHappiness:F0}";
            }

            // Rescue button
            if (rescueButton)
            {
                rescueButton.gameObject.SetActive(!owned);
                bool canAfford = ResourceManager.Instance?.CanAfford(data.rescueCostSnuggles, data.rescueCostGoldenPaw) ?? false;
                rescueButton.interactable = canAfford;
            }
            if (rescueCostText)
            {
                rescueCostText.gameObject.SetActive(!owned);
                string cost = "";
                if (data.rescueCostSnuggles > 0)  cost += $"🐾 {NumberFormatter.Format(data.rescueCostSnuggles)}  ";
                if (data.rescueCostGoldenPaw > 0) cost += $"✨ {NumberFormatter.Format(data.rescueCostGoldenPaw)}";
                rescueCostText.text = cost.Trim();
            }

            if (assignButton) assignButton.gameObject.SetActive(owned);
        }

        private void CloseDetail()
        {
            detailPanel?.SetActive(false);
            _selectedCat = null;
        }

        private void OnRescueClicked()
        {
            if (_selectedCat == null) return;
            bool success = CatManager.Instance?.TryRescueCat(_selectedCat.catId) ?? false;
            if (success)
            {
                ShowDetail(_selectedCat);
                RefreshStats();
                // Update card
                foreach (var card in _cards)
                    if (card.CatId == _selectedCat.catId) card.SetOwned(true);
            }
            else AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnAssignClicked()
        {
            // Opens habitat assignment sub-panel (simplified: assign to first available)
            if (_selectedCat == null) return;
            AudioManager.Instance?.PlaySFX("ui_click");
            // In full implementation: show habitat picker modal
            Debug.Log($"[UI] Assign {_selectedCat.catName} — habitat picker not yet wired");
        }

        // ── Event Handlers ────────────────────────────────────────────────────
        private void OnCatRescued(OnCatRescued evt)
        {
            if (gameObject.activeInHierarchy) RebuildGrid();
            RefreshStats();
        }

        private void OnHappinessChanged(OnCatHappinessChanged evt)
        {
            if (_selectedCat != null && _selectedCat.catId == evt.CatId)
            {
                var entry = CatManager.Instance?.GetEntry(evt.CatId);
                if (happinessSlider && entry != null)
                    happinessSlider.value = entry.happiness / _selectedCat.maxHappiness;
                if (detailHappinessText && entry != null)
                    detailHappinessText.text = $"Happiness: {entry.happiness:F0}/{_selectedCat.maxHappiness:F0}";
            }
        }

        private Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
