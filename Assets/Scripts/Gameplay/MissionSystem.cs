using System;
using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Core;
using WhiskerHaven.Data;

namespace WhiskerHaven.Gameplay
{
    public class MissionSystem : Singleton<MissionSystem>
    {
        private GameConfig _config;
        private SaveData   _save;
        private Dictionary<string, MissionData>    _dataLookup    = new();
        private Dictionary<string, MissionSaveEntry> _entryLookup = new();

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;

            _dataLookup.Clear();
            if (config.allMissions != null)
                foreach (var m in config.allMissions)
                    if (m != null) _dataLookup[m.missionId] = m;

            if (_save.missionProgress == null) _save.missionProgress = new();
            RebuildEntryLookup();

            CheckDailyReset();
            EnsureDailyMissions();
        }

        private void RebuildEntryLookup()
        {
            _entryLookup.Clear();
            foreach (var e in _save.missionProgress) _entryLookup[e.missionId] = e;
        }

        // ── Daily Reset ───────────────────────────────────────────────────────
        private void CheckDailyReset()
        {
            if (string.IsNullOrEmpty(_save.dailyMissionResetTimeUtc)) return;
            if (!DateTime.TryParse(_save.dailyMissionResetTimeUtc, out DateTime lastReset)) return;

            DateTime now = DateTime.UtcNow;
            DateTime nextReset = lastReset.Date.AddDays(1).AddHours(_config.dailyResetHour);

            if (now >= nextReset)
            {
                ResetDailyMissions();
                _save.dailyMissionResetTimeUtc = now.ToString("o");
                EventBus.Publish(new OnDailyReset());

                // Update login streak
                if ((now.Date - lastReset.Date).TotalDays <= 1)
                    _save.loginDays++;
                else
                    _save.loginDays = 1;
            }
        }

        private void ResetDailyMissions()
        {
            if (_save.activeDailyMissionIds == null) return;
            foreach (var id in _save.activeDailyMissionIds)
            {
                if (_entryLookup.TryGetValue(id, out var entry))
                {
                    entry.progress    = 0;
                    entry.isCompleted = false;
                    entry.rewardClaimed = false;
                }
            }
            _save.activeDailyMissionIds.Clear();
        }

        private void EnsureDailyMissions()
        {
            if (_save.activeDailyMissionIds == null) _save.activeDailyMissionIds = new();
            if (_save.activeDailyMissionIds.Count >= _config.dailyMissionSlots) return;

            // Build weighted pool of daily missions
            var pool = new List<(MissionData data, int weight)>();
            foreach (var m in _config.allMissions)
            {
                if (m == null || m.type != MissionType.Daily) continue;
                if (_save.activeDailyMissionIds.Contains(m.missionId)) continue;
                pool.Add((m, m.dailyWeight));
            }

            while (_save.activeDailyMissionIds.Count < _config.dailyMissionSlots && pool.Count > 0)
            {
                var picked = WeightedRandom(pool);
                _save.activeDailyMissionIds.Add(picked.missionId);

                if (!_entryLookup.ContainsKey(picked.missionId))
                {
                    var entry = new MissionSaveEntry { missionId = picked.missionId };
                    _save.missionProgress.Add(entry);
                    _entryLookup[picked.missionId] = entry;
                }

                pool.RemoveAll(p => p.data.missionId == picked.missionId);
            }
        }

        private MissionData WeightedRandom(List<(MissionData data, int weight)> pool)
        {
            int total = 0;
            foreach (var (_, w) in pool) total += w;
            int roll = UnityEngine.Random.Range(0, total);
            foreach (var (data, w) in pool)
            {
                roll -= w;
                if (roll < 0) return data;
            }
            return pool[0].data;
        }

        // ── Track ─────────────────────────────────────────────────────────────
        public void TrackProduction(double snugglesEarned)
        {
            TrackAll(MissionCondition.EarnSnuggles, snugglesEarned);
        }

        public void TrackCatRescue(string catId, CatRarity rarity)
        {
            TrackAll(MissionCondition.RescueCats, 1);
            if (rarity >= CatRarity.Rare)
                TrackAll(MissionCondition.CollectRareCat, 1);
        }

        public void TrackHabitatUpgrade(string habitatId, int level)
        {
            TrackAll(MissionCondition.UpgradeHabitat, 1, habitatId);
        }

        public void TrackVolunteerAssigned()
        {
            TrackAll(MissionCondition.AssignVolunteers, 1);
        }

        private void TrackAll(MissionCondition cond, double amount, string targetId = null)
        {
            bool anyCompleted = false;
            foreach (var id in _save.activeDailyMissionIds)
            {
                if (!_dataLookup.TryGetValue(id, out var data)) continue;
                if (data.condition != cond) continue;
                if (!string.IsNullOrEmpty(data.targetId) && data.targetId != targetId) continue;

                if (_entryLookup.TryGetValue(id, out var entry) && !entry.isCompleted)
                {
                    entry.progress = Math.Min(entry.progress + amount, data.targetAmount);
                    float pct = (float)(entry.progress / data.targetAmount);
                    EventBus.Publish(new OnMissionProgress { MissionId = id, Progress = pct });

                    if (entry.progress >= data.targetAmount)
                    {
                        entry.isCompleted = true;
                        anyCompleted = true;
                        EventBus.Publish(new OnMissionCompleted { MissionId = id });
                    }
                }
            }
            if (anyCompleted) AchievementSystem.Instance?.Check();
        }

        // ── Claim Reward ──────────────────────────────────────────────────────
        public bool TryClaimReward(string missionId)
        {
            if (!_dataLookup.TryGetValue(missionId, out var data)) return false;
            if (!_entryLookup.TryGetValue(missionId, out var entry)) return false;
            if (!entry.isCompleted || entry.rewardClaimed) return false;

            entry.rewardClaimed = true;
            var reward = data.reward;
            if (reward.snuggles   > 0) ResourceManager.Instance.AddSnuggles(reward.snuggles);
            if (reward.goldenPaw  > 0) ResourceManager.Instance.AddGoldenPaw(reward.goldenPaw);
            if (reward.blueprints > 0) ResourceManager.Instance.AddBlueprints(reward.blueprints);

            if (reward.catReward != null)
                CatManager.Instance?.TryRescueCat(reward.catReward.catId);

            AudioManager.Instance?.PlaySFX("mission_complete");
            return true;
        }

        // ── Queries ───────────────────────────────────────────────────────────
        public List<string> GetActiveDailyIds()   => _save.activeDailyMissionIds ?? new();
        public MissionData GetData(string id)     => _dataLookup.TryGetValue(id, out var d) ? d : null;
        public MissionSaveEntry GetEntry(string id) => _entryLookup.TryGetValue(id, out var e) ? e : null;
        public float GetProgress(string id)
        {
            if (!_dataLookup.TryGetValue(id, out var data)) return 0;
            if (!_entryLookup.TryGetValue(id, out var entry)) return 0;
            return (float)(entry.progress / data.targetAmount);
        }
    }
}
