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
    public class HabitatView : BaseView
    {
        [Header("Habitat List")]
        [SerializeField] private Transform          habitatListParent;
        [SerializeField] private HabitatCardUI      habitatCardPrefab;

        [Header("Detail Panel")]
        [SerializeField] private GameObject         detailPanel;
        [SerializeField] private Image              habitatIcon;
        [SerializeField] private TextMeshProUGUI    habitatName;
        [SerializeField] private TextMeshProUGUI    habitatLevel;
        [SerializeField] private TextMeshProUGUI    habitatDescription;
        [SerializeField] private TextMeshProUGUI    productionInfo;
        [SerializeField] private TextMeshProUGUI    slotInfo;
        [SerializeField] private Button             upgradeButton;
        [SerializeField] private TextMeshProUGUI    upgradeCostText;
        [SerializeField] private TextMeshProUGUI    upgradeDescText;
        [SerializeField] private Button             unlockButton;
        [SerializeField] private TextMeshProUGUI    unlockCostText;
        [SerializeField] private Button             closeDetailBtn;

        [Header("Cat Slots")]
        [SerializeField] private Transform          catSlotsParent;
        [SerializeField] private HabitatCatSlotUI   catSlotPrefab;

        [Header("Volunteer Slot")]
        [SerializeField] private TextMeshProUGUI    volunteerText;
        [SerializeField] private Button             assignVolunteerBtn;

        private HabitatData _selectedHabitat;
        private List<HabitatCardUI> _cards = new();

        public void Init()
        {
            EventBus.Subscribe<OnHabitatUpgraded>(OnHabitatUpgraded);
            closeDetailBtn?.onClick.AddListener(CloseDetail);
            upgradeButton?.onClick.AddListener(OnUpgradeClicked);
            unlockButton?.onClick.AddListener(OnUnlockClicked);
            assignVolunteerBtn?.onClick.AddListener(OnAssignVolunteerClicked);
            detailPanel?.SetActive(false);
        }

        private void OnDestroy() => EventBus.Unsubscribe<OnHabitatUpgraded>(OnHabitatUpgraded);

        protected override void OnShow() => RebuildList();

        private void RebuildList()
        {
            habitatListParent.DestroyAllChildren();
            _cards.Clear();
            var hm = HabitatManager.Instance;
            if (hm == null) return;

            foreach (var data in hm.GetAllHabitats())
            {
                if (data == null) continue;
                var card = Instantiate(habitatCardPrefab, habitatListParent);
                card.Populate(data, () => OnHabitatClicked(data));
                _cards.Add(card);
            }
        }

        private void OnHabitatClicked(HabitatData data)
        {
            _selectedHabitat = data;
            ShowDetail(data);
            AudioManager.Instance?.PlaySFX("ui_click");
        }

        private void ShowDetail(HabitatData data)
        {
            detailPanel?.SetActive(true);
            var hm     = HabitatManager.Instance;
            var entry  = hm?.GetEntry(data.habitatId);
            bool unlocked = entry?.isUnlocked ?? false;
            int  level    = entry?.level ?? 0;

            if (habitatIcon)    habitatIcon.sprite  = data.icon;
            if (habitatName)    habitatName.text    = data.habitatName;
            if (habitatLevel)   habitatLevel.text   = unlocked ? $"Level {level}" : "Locked";
            if (habitatDescription) habitatDescription.text = data.description;

            if (unlocked)
            {
                var levelData = data.GetLevel(level);
                if (productionInfo)
                    productionInfo.text = $"×{levelData.productionMultiplier:F1} production bonus";
                if (slotInfo)
                    slotInfo.text = $"{entry.catIds.Count}/{levelData.maxCatSlots} cats";

                // Upgrade button
                bool canUpgrade = data.CanUpgrade(level);
                upgradeButton?.gameObject.SetActive(canUpgrade);
                unlockButton?.gameObject.SetActive(false);
                if (canUpgrade)
                {
                    var next = data.GetLevel(level + 1);
                    bool afford = ResourceManager.Instance?.CanAfford(next.upgradeCostSnuggles, 0, next.upgradeCostBlueprints) ?? false;
                    if (upgradeButton) upgradeButton.interactable = afford;
                    string cost = $"🐾 {NumberFormatter.Format(next.upgradeCostSnuggles)}";
                    if (next.upgradeCostBlueprints > 0)
                        cost += $"  📐 {NumberFormatter.Format(next.upgradeCostBlueprints)}";
                    if (upgradeCostText) upgradeCostText.text = cost;
                    if (upgradeDescText) upgradeDescText.text = next.upgradeDescription;
                }
            }
            else
            {
                upgradeButton?.gameObject.SetActive(false);
                unlockButton?.gameObject.SetActive(true);
                string save   = GameManager.Instance?.Save?.hallLevel.ToString() ?? "?";
                bool   meets  = (GameManager.Instance?.Save?.hallLevel ?? 0) >= data.requiredHallLevel;
                bool   afford = meets && (ResourceManager.Instance?.CanAfford(data.unlockCostSnuggles, 0, data.unlockCostBlueprints) ?? false);
                if (unlockButton) unlockButton.interactable = afford;
                string cost = $"🐾 {NumberFormatter.Format(data.unlockCostSnuggles)}";
                if (data.unlockCostBlueprints > 0) cost += $"  📐 {NumberFormatter.Format(data.unlockCostBlueprints)}";
                if (unlockCostText) unlockCostText.text = meets ? cost : $"Requires Haven Lv.{data.requiredHallLevel}";
            }

            // Cat slots
            RefreshCatSlots(data, entry);

            // Volunteer
            RefreshVolunteer(data.habitatId);
        }

        private void RefreshCatSlots(HabitatData data, HabitatSaveEntry entry)
        {
            catSlotsParent?.gameObject.SetActive(entry?.isUnlocked ?? false);
            catSlotsParent?.DestroyAllChildren();
            if (entry == null || !entry.isUnlocked || catSlotPrefab == null) return;

            int maxSlots = data.GetLevel(entry.level).maxCatSlots;
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Instantiate(catSlotPrefab, catSlotsParent);
                string catId = i < entry.catIds.Count ? entry.catIds[i] : null;
                slot.Populate(catId, data.habitatId);
            }
        }

        private void RefreshVolunteer(string habitatId)
        {
            if (volunteerText == null) return;
            var vs = VolunteerSystem.Instance;
            if (vs == null) { volunteerText.text = "No volunteer"; return; }

            string assigned = null;
            foreach (var v in vs.GetAllVolunteers())
            {
                var e = vs.GetEntry(v.volunteerId);
                if (e?.assignedHabitatId == habitatId) { assigned = v.volunteerName; break; }
            }
            volunteerText.text = assigned != null ? $"Volunteer: {assigned}" : "No volunteer assigned";
        }

        private void CloseDetail()
        {
            detailPanel?.SetActive(false);
            _selectedHabitat = null;
        }

        private void OnUpgradeClicked()
        {
            if (_selectedHabitat == null) return;
            bool ok = HabitatManager.Instance?.TryUpgrade(_selectedHabitat.habitatId) ?? false;
            if (ok) ShowDetail(_selectedHabitat);
            else    AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnUnlockClicked()
        {
            if (_selectedHabitat == null) return;
            bool ok = HabitatManager.Instance?.TryUnlock(_selectedHabitat.habitatId) ?? false;
            if (ok) { RebuildList(); ShowDetail(_selectedHabitat); }
            else    AudioManager.Instance?.PlaySFX("ui_error");
        }

        private void OnAssignVolunteerClicked()
        {
            Debug.Log("[UI] Volunteer assignment modal — todo");
        }

        private void OnHabitatUpgraded(OnHabitatUpgraded evt)
        {
            if (_selectedHabitat?.habitatId == evt.HabitatId)
                ShowDetail(_selectedHabitat);
            foreach (var c in _cards) c.Refresh();
        }
    }
}
