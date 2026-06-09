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
    public class HabitatView : BaseView
    {
        [SerializeField] public HabitatCardUI     habitatCardPrefab;
        [SerializeField] public HabitatCatSlotUI  catSlotPrefab;

        private RectTransform    _listContent;
        private GameObject       _detailPanel;
        private Image            _habitatIcon;
        private TextMeshProUGUI  _habitatName, _habitatLevel, _habitatDesc, _productionInfo, _slotInfo, _volunteerText;
        private TextMeshProUGUI  _upgradeCostText, _unlockCostText;
        private Button           _upgradeBtn, _unlockBtn, _volunteerBtn;
        private Transform        _catSlotsParent;

        private HabitatData _selected;
        private List<HabitatCardUI> _cards = new();

        protected override void Awake() { base.Awake(); BuildUI(); }
        protected override void OnShow() { RebuildList(); }
        protected override void OnHide() => _detailPanel?.SetActive(false);

        private void BuildUI()
        {
            var root = Panel(transform, "BG", BG_MED); Stretch(root);

            // Header
            var header = Panel(root.transform, "Header", BG_DARK);
            AnchorTop(header, 56);
            HLayout(header, 12, new RectOffset(16, 16, 8, 8));
            Text(header.transform, "Title", "🏠  Habitats", 20, CREAM, TextAlignmentOptions.Left, FontStyles.Bold);
            var sp = Group(header.transform, "Sp"); LE(sp, flexW: 1);
            var backBtn = Btn(header.transform, "Back", "← Back", AMBER_D, TEXT_L, 13);
            LE(backBtn.gameObject, prefW: 90, prefH: 38);
            backBtn.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            // Main split
            var mainArea = Group(root.transform, "Main");
            StretchWithMargin(mainArea, 56, 0);
            HLayout(mainArea, 0, new RectOffset(0, 0, 0, 0));

            // Habitat list (left)
            var listArea = Group(mainArea.transform, "ListArea");
            LE(listArea, flexW: 1);
            var (_, content) = ScrollV(listArea.transform, "HabitatScroll");
            _listContent = content;
            VLayout(content.gameObject, 8, new RectOffset(12, 12, 12, 12));
            Fitter(content.gameObject);

            // Detail panel (right)
            BuildDetailPanel(mainArea.transform);

            EventBus.Subscribe<OnHabitatUpgraded>(evt => {
                if (_selected?.habitatId == evt.HabitatId) ShowDetail(_selected);
                foreach (var c in _cards) c.Refresh();
            });
        }

        private void BuildDetailPanel(Transform parent)
        {
            _detailPanel = Panel(parent, "DetailPanel", CREAM);
            LE(_detailPanel, prefW: 320);
            _detailPanel.SetActive(false);
            VLayout(_detailPanel, 10, new RectOffset(16, 16, 16, 16));

            // Icon
            var iconGo = Panel(_detailPanel.transform, "Icon", CREAM_ALT);
            LE(iconGo, prefH: 100, minH: 100);
            _habitatIcon = iconGo.GetComponent<Image>();

            _habitatName   = Text(_detailPanel.transform, "Name", "", 20, TEXT_D, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_habitatName.gameObject, prefH: 28);
            _habitatLevel  = Text(_detailPanel.transform, "Level", "", 14, AMBER, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_habitatLevel.gameObject, prefH: 22);
            _habitatDesc   = Text(_detailPanel.transform, "Desc", "", 12, TEXT_D);
            _habitatDesc.enableWordWrapping = true;
            LE(_habitatDesc.gameObject, prefH: 50);

            // Info row
            var infoRow = Group(_detailPanel.transform, "InfoRow");
            LE(infoRow, prefH: 22);
            HLayout(infoRow, 8, new RectOffset(0, 0, 0, 0));
            _productionInfo = Text(infoRow.transform, "Prod", "", 12, SUCCESS, TextAlignmentOptions.Left);
            _slotInfo       = Text(infoRow.transform, "Slots", "", 12, AMBER, TextAlignmentOptions.Right);

            // Cat slots header
            Text(_detailPanel.transform, "SlotHeader", "Cat Slots:", 13, TEXT_D, TextAlignmentOptions.Left, FontStyles.Bold);
            LE(Text(_detailPanel.transform, "SlotHeader", "Cat Slots:", 13, TEXT_D, TextAlignmentOptions.Left, FontStyles.Bold).gameObject, prefH: 20);

            var slotsContainer = Group(_detailPanel.transform, "SlotsContainer");
            LE(slotsContainer, prefH: 50);
            HLayout(slotsContainer, 6, new RectOffset(0, 0, 0, 0));
            _catSlotsParent = slotsContainer.transform;

            // Volunteer
            _volunteerText = Text(_detailPanel.transform, "Volunteer", "No volunteer assigned", 12, BG_LIGHT);
            LE(_volunteerText.gameObject, prefH: 20);
            _volunteerBtn = Btn(_detailPanel.transform, "AssignVolBtn", "👤 Assign Volunteer", BG_LIGHT, TEXT_L, 12);
            LE(_volunteerBtn.gameObject, prefH: 36);
            _volunteerBtn.onClick.AddListener(() => Debug.Log("Volunteer picker TODO"));

            // Upgrade / Unlock cost
            _upgradeCostText = Text(_detailPanel.transform, "UpgradeCost", "", 12, TEXT_D, TextAlignmentOptions.Center);
            LE(_upgradeCostText.gameObject, prefH: 20);
            _unlockCostText  = Text(_detailPanel.transform, "UnlockCost", "", 12, TEXT_D, TextAlignmentOptions.Center);
            LE(_unlockCostText.gameObject, prefH: 20);

            _upgradeBtn = Btn(_detailPanel.transform, "UpgradeBtn", "⬆ Upgrade", SUCCESS, TEXT_L, 14);
            LE(_upgradeBtn.gameObject, prefH: 44);
            _upgradeBtn.onClick.AddListener(OnUpgradeClicked);

            _unlockBtn = Btn(_detailPanel.transform, "UnlockBtn", "🔓 Unlock", AMBER, TEXT_L, 14);
            LE(_unlockBtn.gameObject, prefH: 44);
            _unlockBtn.onClick.AddListener(OnUnlockClicked);

            var closeBtn = Btn(_detailPanel.transform, "Close", "✕ Close", AMBER_D, TEXT_L, 12);
            LE(closeBtn.gameObject, prefH: 36);
            closeBtn.onClick.AddListener(() => _detailPanel.SetActive(false));
        }

        private void RebuildList()
        {
            _listContent.DestroyAllChildren();
            _cards.Clear();
            var hm = HabitatManager.Instance;
            if (hm == null) return;
            foreach (var data in hm.GetAllHabitats())
            {
                if (data == null) continue;
                HabitatCardUI card;
                if (habitatCardPrefab != null) card = Instantiate(habitatCardPrefab, _listContent);
                else { var go = Panel(_listContent, data.habitatId, CREAM_ALT); LE(go, prefH: 80); card = go.AddComponent<HabitatCardUI>(); }
                card.Populate(data, () => OnHabitatClicked(data));
                _cards.Add(card);
            }
        }

        private void OnHabitatClicked(HabitatData data)
        {
            _selected = data;
            ShowDetail(data);
            AudioManager.Instance?.PlaySFX("ui_click");
        }

        private void ShowDetail(HabitatData data)
        {
            _detailPanel?.SetActive(true);
            var hm     = HabitatManager.Instance;
            var entry  = hm?.GetEntry(data.habitatId);
            bool locked = !(entry?.isUnlocked ?? false);
            int  level  = entry?.level ?? 0;

            if (_habitatIcon)  _habitatIcon.sprite = data.icon;
            if (_habitatName)  _habitatName.text   = data.habitatName;
            if (_habitatLevel) _habitatLevel.text  = locked ? "🔒 Locked" : $"Level {level}";
            if (_habitatDesc)  _habitatDesc.text   = data.description;

            if (!locked)
            {
                var ld = data.GetLevel(level);
                if (_productionInfo) _productionInfo.text = $"×{ld.productionMultiplier:F1} production";
                if (_slotInfo && entry != null) _slotInfo.text = $"{entry.catIds.Count}/{ld.maxCatSlots} cats";

                bool canUpgrade = data.CanUpgrade(level);
                _upgradeBtn?.gameObject.SetActive(canUpgrade);
                _unlockBtn?.gameObject.SetActive(false);

                if (canUpgrade)
                {
                    var next = data.GetLevel(level + 1);
                    bool afford = ResourceManager.Instance?.CanAfford(next.upgradeCostSnuggles, 0, next.upgradeCostBlueprints) ?? false;
                    if (_upgradeBtn) _upgradeBtn.interactable = afford;
                    string cost = $"🐾 {NumberFormatter.Format(next.upgradeCostSnuggles)}";
                    if (next.upgradeCostBlueprints > 0) cost += $"  📐 {NumberFormatter.Format(next.upgradeCostBlueprints)}";
                    if (_upgradeCostText) _upgradeCostText.text = cost;
                }
                else if (_upgradeCostText) _upgradeCostText.text = "MAX LEVEL";

                RefreshCatSlots(data, entry);
                RefreshVolunteer(data.habitatId);
            }
            else
            {
                _upgradeBtn?.gameObject.SetActive(false);
                _unlockBtn?.gameObject.SetActive(true);
                bool hallOk = (GameManager.Instance?.Save?.hallLevel ?? 0) >= data.requiredHallLevel;
                bool afford = hallOk && (ResourceManager.Instance?.CanAfford(data.unlockCostSnuggles, 0, data.unlockCostBlueprints) ?? false);
                if (_unlockBtn) _unlockBtn.interactable = afford;
                string cost = hallOk ? $"🐾 {NumberFormatter.Format(data.unlockCostSnuggles)}"
                                     : $"Requires Haven Lv.{data.requiredHallLevel}";
                if (data.unlockCostBlueprints > 0 && hallOk) cost += $"  📐 {NumberFormatter.Format(data.unlockCostBlueprints)}";
                if (_unlockCostText) _unlockCostText.text = cost;
                if (_upgradeCostText) _upgradeCostText.text = "";
                _catSlotsParent?.gameObject.SetActive(false);
            }
        }

        private void RefreshCatSlots(HabitatData data, HabitatSaveEntry entry)
        {
            if (_catSlotsParent == null) return;
            _catSlotsParent.DestroyAllChildren();
            _catSlotsParent.gameObject.SetActive(true);
            if (entry == null) return;
            int max = data.GetLevel(entry.level).maxCatSlots;
            for (int i = 0; i < max; i++)
            {
                string catId = i < entry.catIds.Count ? entry.catIds[i] : null;
                HabitatCatSlotUI slot;
                if (catSlotPrefab != null) slot = Instantiate(catSlotPrefab, _catSlotsParent);
                else { var go = Panel(_catSlotsParent, $"Slot{i}", CREAM_ALT); LE(go, prefW: 44, prefH: 44); slot = go.AddComponent<HabitatCatSlotUI>(); }
                slot.Populate(catId, data.habitatId);
            }
        }

        private void RefreshVolunteer(string habitatId)
        {
            if (_volunteerText == null) return;
            string assigned = null;
            var vs = VolunteerSystem.Instance;
            if (vs != null)
                foreach (var v in vs.GetAllVolunteers())
                {
                    var e = vs.GetEntry(v.volunteerId);
                    if (e?.assignedHabitatId == habitatId) { assigned = v.volunteerName; break; }
                }
            _volunteerText.text = assigned != null ? $"👤 Volunteer: {assigned}" : "No volunteer assigned";
        }

        private void OnUpgradeClicked()
        {
            if (_selected == null) return;
            bool ok = HabitatManager.Instance?.TryUpgrade(_selected.habitatId) ?? false;
            if (ok) { ShowDetail(_selected); RebuildList(); }
            else AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnUnlockClicked()
        {
            if (_selected == null) return;
            bool ok = HabitatManager.Instance?.TryUnlock(_selected.habitatId) ?? false;
            if (ok) { RebuildList(); ShowDetail(_selected); }
            else AudioManager.Instance?.PlaySFX("ui_error");
        }
    }
}
