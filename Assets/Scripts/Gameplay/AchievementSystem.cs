using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Data;

namespace WhiskerHaven.Gameplay
{
    public class AchievementSystem : Singleton<AchievementSystem>
    {
        private GameConfig _config;
        private SaveData   _save;
        private HashSet<string> _unlocked = new();

        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;
            _unlocked = new HashSet<string>(_save.unlockedAchievementIds ?? new());
        }

        public void Check()
        {
            if (_config.allAchievements == null) return;
            foreach (var ach in _config.allAchievements)
            {
                if (ach == null || _unlocked.Contains(ach.achievementId)) continue;
                if (EvaluateCondition(ach))
                    Unlock(ach);
            }
        }

        private bool EvaluateCondition(AchievementData ach)
        {
            double value = GetStatValue(ach.condition, ach.targetId);
            return value >= ach.targetAmount;
        }

        private double GetStatValue(MissionCondition cond, string targetId = null)
        {
            return cond switch
            {
                MissionCondition.EarnSnuggles      => _save.lifetimeSnuggles,
                MissionCondition.RescueCats        => _save.totalCatsRescued,
                MissionCondition.UpgradeHabitat    => _save.totalHabitatsUpgraded,
                MissionCondition.SpendBlueprints   => _save.totalBlueprintsSpent,
                MissionCondition.EarnGoldenPaw     => _save.goldenPaw,
                MissionCondition.ReachPurrPower    => _save.maxPurrPowerReached,
                MissionCondition.LoginDays         => _save.loginDays,
                MissionCondition.AssignVolunteers  => VolunteerSystem.Instance?.AssignedCount() ?? 0,
                MissionCondition.CollectRareCat    => CountRareCats(),
                _ => 0
            };
        }

        private double CountRareCats()
        {
            int count = 0;
            if (CatManager.Instance == null) return 0;
            foreach (var e in CatManager.Instance.GetOwnedEntries())
            {
                var data = CatManager.Instance.GetData(e.catId);
                if (data != null && data.rarity >= CatRarity.Rare) count++;
            }
            return count;
        }

        private void Unlock(AchievementData ach)
        {
            _unlocked.Add(ach.achievementId);
            if (!_save.unlockedAchievementIds.Contains(ach.achievementId))
                _save.unlockedAchievementIds.Add(ach.achievementId);

            if (ach.snugglesReward  > 0) ResourceManager.Instance.AddSnuggles(ach.snugglesReward);
            if (ach.goldenPawReward > 0) ResourceManager.Instance.AddGoldenPaw(ach.goldenPawReward);

            EventBus.Publish(new OnAchievementUnlocked { AchievementId = ach.achievementId });
            AudioManager.Instance?.PlaySFX("achievement_unlock");
            Debug.Log($"[Achievement] Unlocked: {ach.achievementName}");
        }

        public bool IsUnlocked(string id)         => _unlocked.Contains(id);
        public int UnlockedCount                  => _unlocked.Count;
        public List<AchievementData> GetAll()     => new(_config.allAchievements);
    }
}
