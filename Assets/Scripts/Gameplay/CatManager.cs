using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Data;

namespace WhiskerHaven.Gameplay
{
    public class CatManager : Singleton<CatManager>
    {
        private GameConfig _config;
        private SaveData   _save;

        private Dictionary<string, CatData>    _dataLookup  = new();
        private Dictionary<string, CatSaveEntry> _saveLookup = new();

        public int OwnedCount => _save?.ownedCats?.Count ?? 0;
        public int TotalCats  => _config?.allCats?.Length ?? 0;

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;

            _dataLookup.Clear();
            if (config.allCats != null)
                foreach (var c in config.allCats)
                    if (c != null) _dataLookup[c.catId] = c;

            RebuildSaveLookup();
        }

        private void RebuildSaveLookup()
        {
            _saveLookup.Clear();
            if (_save.ownedCats == null) _save.ownedCats = new List<CatSaveEntry>();
            foreach (var e in _save.ownedCats)
                _saveLookup[e.catId] = e;
        }

        // ── Rescue ───────────────────────────────────────────────────────────
        public bool TryRescueCat(string catId)
        {
            if (IsOwned(catId)) return false;
            if (!_dataLookup.TryGetValue(catId, out CatData data)) return false;
            if (_save.hallLevel < data.requiredHallLevel) return false;

            if (!ResourceManager.Instance.TrySpend(snuggles: data.rescueCostSnuggles,
                                                    goldenPaw: data.rescueCostGoldenPaw))
                return false;

            var entry = new CatSaveEntry
            {
                catId    = catId,
                level    = 1,
                happiness = 50f,
                rescueDateUtc = System.DateTime.UtcNow.ToString("o"),
                isNew = true
            };
            _save.ownedCats.Add(entry);
            _saveLookup[catId] = entry;
            _save.totalCatsRescued++;

            IdleManager.Instance?.RecalculateProduction();
            PurrPowerSystem.Instance?.Recalculate();
            MissionSystem.Instance?.TrackCatRescue(catId, data.rarity);
            AchievementSystem.Instance?.Check();

            EventBus.Publish(new OnCatRescued { CatId = catId });
            AudioManager.Instance?.PlaySFX("cat_rescue");
            return true;
        }

        // ── Assign to Habitat ─────────────────────────────────────────────────
        public bool TryAssignToHabitat(string catId, string habitatId)
        {
            if (!_saveLookup.TryGetValue(catId, out var entry)) return false;

            // Remove from previous habitat
            if (!string.IsNullOrEmpty(entry.assignedHabitatId))
            {
                var prevHabitat = GetHabitatEntry(entry.assignedHabitatId);
                prevHabitat?.catIds.Remove(catId);
            }

            var habitat = GetHabitatEntry(habitatId);
            if (habitat == null || !habitat.isUnlocked) return false;

            var habitatData = HabitatManager.Instance?.GetHabitatData(habitatId);
            if (habitatData != null)
            {
                int maxSlots = habitatData.GetLevel(habitat.level).maxCatSlots;
                if (habitat.catIds.Count >= maxSlots) return false;
            }

            entry.assignedHabitatId = habitatId;
            if (!habitat.catIds.Contains(catId))
                habitat.catIds.Add(catId);

            IdleManager.Instance?.RecalculateProduction();
            return true;
        }

        public void UnassignCat(string catId)
        {
            if (!_saveLookup.TryGetValue(catId, out var entry)) return;
            if (string.IsNullOrEmpty(entry.assignedHabitatId)) return;

            var habitat = GetHabitatEntry(entry.assignedHabitatId);
            habitat?.catIds.Remove(catId);
            entry.assignedHabitatId = null;
            IdleManager.Instance?.RecalculateProduction();
        }

        // ── Happiness Tick ───────────────────────────────────────────────────
        public void TickHappiness(float deltaSeconds)
        {
            bool anyChanged = false;
            foreach (var entry in _save.ownedCats)
            {
                if (!_dataLookup.TryGetValue(entry.catId, out var data)) continue;

                bool inPreferredHabitat = !string.IsNullOrEmpty(data.preferredHabitatId)
                    && entry.assignedHabitatId == data.preferredHabitatId;

                float gain  = data.happinessGainRate * deltaSeconds;
                float decay = inPreferredHabitat ? 0f : data.happinessDecayRate * deltaSeconds;

                // Volunteer happiness bonus
                float volunteerBonus = VolunteerSystem.Instance?.GetHappinessBonus(entry.assignedHabitatId) ?? 0f;
                gain += volunteerBonus * deltaSeconds;

                float prev = entry.happiness;
                entry.happiness = Mathf.Clamp(entry.happiness + gain - decay, 0f, data.maxHappiness);

                if (Mathf.Abs(entry.happiness - prev) > 0.01f)
                {
                    anyChanged = true;
                    EventBus.Publish(new OnCatHappinessChanged { CatId = entry.catId, Happiness = entry.happiness });
                }
            }

            if (anyChanged)
                PurrPowerSystem.Instance?.Recalculate();
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public bool IsOwned(string catId)                => _saveLookup.ContainsKey(catId);
        public CatData GetData(string catId)             => _dataLookup.TryGetValue(catId, out var d) ? d : null;
        public CatSaveEntry GetEntry(string catId)       => _saveLookup.TryGetValue(catId, out var e) ? e : null;
        public List<CatData> GetAllCats()                => _config?.allCats != null ? new List<CatData>(_config.allCats) : new();
        public List<CatSaveEntry> GetOwnedEntries()      => _save?.ownedCats ?? new();

        public List<CatData> GetAvailableToBuy()
        {
            var result = new List<CatData>();
            if (_config?.allCats == null) return result;
            foreach (var data in _config.allCats)
                if (data != null && !IsOwned(data.catId) && _save.hallLevel >= data.requiredHallLevel)
                    result.Add(data);
            return result;
        }

        public void MarkCatSeen(string catId)
        {
            if (_saveLookup.TryGetValue(catId, out var e)) e.isNew = false;
        }

        private HabitatSaveEntry GetHabitatEntry(string habitatId)
        {
            return _save.habitats?.Find(h => h.habitatId == habitatId);
        }
    }
}
