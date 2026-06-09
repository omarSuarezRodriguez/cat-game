using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Data;

namespace WhiskerHaven.Gameplay
{
    public class HabitatManager : Singleton<HabitatManager>
    {
        private GameConfig _config;
        private SaveData   _save;
        private Dictionary<string, HabitatData>     _dataLookup = new();
        private Dictionary<string, HabitatSaveEntry> _entryLookup = new();

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;

            _dataLookup.Clear();
            if (config.allHabitats != null)
                foreach (var h in config.allHabitats)
                    if (h != null) _dataLookup[h.habitatId] = h;

            // Ensure all habitats have a save entry
            if (_save.habitats == null) _save.habitats = new();
            foreach (var data in _config.allHabitats)
            {
                if (data == null) continue;
                if (!_save.habitats.Exists(h => h.habitatId == data.habitatId))
                {
                    _save.habitats.Add(new HabitatSaveEntry
                    {
                        habitatId = data.habitatId,
                        isUnlocked = data.requiredHallLevel <= 1 && data.unlockCostSnuggles == 0,
                        level = 1,
                        catIds = new List<string>()
                    });
                }
            }

            RebuildEntryLookup();
        }

        private void RebuildEntryLookup()
        {
            _entryLookup.Clear();
            foreach (var e in _save.habitats)
                _entryLookup[e.habitatId] = e;
        }

        // ── Unlock ────────────────────────────────────────────────────────────
        public bool TryUnlock(string habitatId)
        {
            if (!_dataLookup.TryGetValue(habitatId, out var data)) return false;
            if (!_entryLookup.TryGetValue(habitatId, out var entry)) return false;
            if (entry.isUnlocked) return false;
            if (_save.hallLevel < data.requiredHallLevel) return false;

            if (!ResourceManager.Instance.TrySpend(snuggles: data.unlockCostSnuggles,
                                                    blueprints: data.unlockCostBlueprints))
                return false;

            entry.isUnlocked = true;
            IdleManager.Instance?.RecalculateProduction();
            AudioManager.Instance?.PlaySFX("habitat_unlock");
            return true;
        }

        // ── Upgrade ───────────────────────────────────────────────────────────
        public bool TryUpgrade(string habitatId)
        {
            if (!_dataLookup.TryGetValue(habitatId, out var data)) return false;
            if (!_entryLookup.TryGetValue(habitatId, out var entry)) return false;
            if (!entry.isUnlocked) return false;
            if (!data.CanUpgrade(entry.level)) return false;

            var nextLevel = data.GetLevel(entry.level + 1);
            if (!ResourceManager.Instance.TrySpend(snuggles: nextLevel.upgradeCostSnuggles,
                                                    blueprints: nextLevel.upgradeCostBlueprints))
                return false;

            entry.level++;
            _save.totalHabitatsUpgraded++;

            IdleManager.Instance?.RecalculateProduction();
            MissionSystem.Instance?.TrackHabitatUpgrade(habitatId, entry.level);
            AchievementSystem.Instance?.Check();

            EventBus.Publish(new OnHabitatUpgraded { HabitatId = habitatId, NewLevel = entry.level });
            AudioManager.Instance?.PlaySFX("habitat_upgrade");
            return true;
        }

        // ── Production Helpers ────────────────────────────────────────────────
        public float GetProductionMultiplier(string habitatId, string catId)
        {
            if (!_dataLookup.TryGetValue(habitatId, out var data)) return 1f;
            if (!_entryLookup.TryGetValue(habitatId, out var entry)) return 1f;

            var levelData = data.GetLevel(entry.level);
            float mult = levelData.productionMultiplier;

            // Check if cat has preferred personality match
            var catData = CatManager.Instance?.GetData(catId);
            if (catData != null && catData.personality == data.preferredPersonality)
                mult *= 2f;

            return mult;
        }

        public double GetTotalPassiveBonus()
        {
            double total = 0;
            foreach (var entry in _save.habitats)
            {
                if (!entry.isUnlocked) continue;
                if (_dataLookup.TryGetValue(entry.habitatId, out var data))
                    total += data.passiveBonusPerSecond;
            }
            return total;
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public HabitatData GetHabitatData(string id) => _dataLookup.TryGetValue(id, out var d) ? d : null;
        public HabitatSaveEntry GetEntry(string id)  => _entryLookup.TryGetValue(id, out var e) ? e : null;
        public List<HabitatData> GetAllHabitats()    => new List<HabitatData>(_config.allHabitats);
        public bool IsUnlocked(string id)            => _entryLookup.TryGetValue(id, out var e) && e.isUnlocked;
        public int GetLevel(string id)               => _entryLookup.TryGetValue(id, out var e) ? e.level : 0;

        public HabitatUpgradeLevel GetNextUpgrade(string habitatId)
        {
            if (!_dataLookup.TryGetValue(habitatId, out var data)) return null;
            if (!_entryLookup.TryGetValue(habitatId, out var entry)) return null;
            if (!data.CanUpgrade(entry.level)) return null;
            return data.GetLevel(entry.level + 1);
        }
    }
}
