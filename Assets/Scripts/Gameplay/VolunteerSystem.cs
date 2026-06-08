using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Core;
using WhiskerHaven.Data;

namespace WhiskerHaven.Gameplay
{
    public class VolunteerSystem : Singleton<VolunteerSystem>
    {
        private GameConfig _config;
        private SaveData   _save;
        private Dictionary<string, VolunteerData>     _dataLookup  = new();
        private Dictionary<string, VolunteerSaveEntry> _entryLookup = new();

        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;

            _dataLookup.Clear();
            if (config.allVolunteers != null)
                foreach (var v in config.allVolunteers)
                    if (v != null) _dataLookup[v.volunteerId] = v;

            if (_save.volunteers == null) _save.volunteers = new();
            foreach (var data in _config.allVolunteers)
            {
                if (data == null) continue;
                if (!_save.volunteers.Exists(v => v.volunteerId == data.volunteerId))
                    _save.volunteers.Add(new VolunteerSaveEntry { volunteerId = data.volunteerId });
            }
            RebuildLookup();
        }

        private void RebuildLookup()
        {
            _entryLookup.Clear();
            foreach (var e in _save.volunteers) _entryLookup[e.volunteerId] = e;
        }

        public bool TryUnlock(string volunteerId)
        {
            if (!_dataLookup.TryGetValue(volunteerId, out var data)) return false;
            if (!_entryLookup.TryGetValue(volunteerId, out var entry)) return false;
            if (entry.isUnlocked) return false;
            if (_save.hallLevel < data.requiredHallLevel) return false;

            if (!ResourceManager.Instance.TrySpend(snuggles: data.unlockCostSnuggles,
                                                    goldenPaw: data.unlockCostGoldenPaw))
                return false;

            entry.isUnlocked = true;
            return true;
        }

        public bool TryAssign(string volunteerId, string habitatId)
        {
            if (!_entryLookup.TryGetValue(volunteerId, out var entry)) return false;
            if (!entry.isUnlocked) return false;

            entry.assignedHabitatId = habitatId;
            IdleManager.Instance?.RecalculateProduction();

            EventBus.Publish(new OnVolunteerAssigned { VolunteerId = volunteerId, HabitatId = habitatId });
            MissionSystem.Instance?.TrackVolunteerAssigned();
            return true;
        }

        public void Unassign(string volunteerId)
        {
            if (_entryLookup.TryGetValue(volunteerId, out var entry))
            {
                entry.assignedHabitatId = null;
                IdleManager.Instance?.RecalculateProduction();
            }
        }

        public float GetProductionBonus(string habitatId)
        {
            float bonus = 0f;
            foreach (var entry in _save.volunteers)
            {
                if (!entry.isUnlocked || entry.assignedHabitatId != habitatId) continue;
                if (_dataLookup.TryGetValue(entry.volunteerId, out var data))
                    bonus += data.productionBonus;
            }
            return bonus;
        }

        public float GetHappinessBonus(string habitatId)
        {
            float bonus = 0f;
            foreach (var entry in _save.volunteers)
            {
                if (!entry.isUnlocked || entry.assignedHabitatId != habitatId) continue;
                if (_dataLookup.TryGetValue(entry.volunteerId, out var data))
                    bonus += data.happinessBonus;
            }
            return bonus;
        }

        public double GetTotalBlueprintGen()
        {
            double total = 0;
            foreach (var entry in _save.volunteers)
            {
                if (!entry.isUnlocked || string.IsNullOrEmpty(entry.assignedHabitatId)) continue;
                if (_dataLookup.TryGetValue(entry.volunteerId, out var data))
                    total += data.blueprintGenPerHour;
            }
            return total;
        }

        public List<VolunteerData> GetAllVolunteers() => new(_config.allVolunteers);
        public VolunteerSaveEntry GetEntry(string id) => _entryLookup.TryGetValue(id, out var e) ? e : null;
        public bool IsUnlocked(string id)             => _entryLookup.TryGetValue(id, out var e) && e.isUnlocked;
        public int AssignedCount()                     => _save.volunteers.FindAll(v => !string.IsNullOrEmpty(v.assignedHabitatId)).Count;
    }
}
