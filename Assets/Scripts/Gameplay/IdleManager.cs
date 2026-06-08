using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Utils;

namespace WhiskerHaven.Gameplay
{
    /// <summary>
    /// Drives all idle production: calculates Snuggles/sec from cats + habitats,
    /// applies Purr Power multiplier, ticks happiness, and handles offline earnings.
    /// </summary>
    public class IdleManager : Singleton<IdleManager>
    {
        private GameConfig _config;
        private SaveData   _save;

        private double _snugglesPerSecond;
        private double _blueprintsPerHour;
        private float  _tickAccumulator;

        private const float TickInterval = 0.5f; // production tick every 0.5s

        public double SnugglesPerSecond  => _snugglesPerSecond;
        public double BlueprintsPerHour  => _blueprintsPerHour;

        // ── Init ─────────────────────────────────────────────────────────────
        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save = save;
            RecalculateProduction();
        }

        // ── Offline Progress ──────────────────────────────────────────────────
        public void ProcessOfflineProgress(double secondsOffline)
        {
            if (secondsOffline <= 0) return;

            double maxOfflineSeconds = _config.maxOfflineHours * 3600f;
            double effectiveSeconds  = System.Math.Min(secondsOffline, maxOfflineSeconds);
            double earned = _snugglesPerSecond * effectiveSeconds * _config.offlineEfficiency;

            if (earned > 0)
            {
                ResourceManager.Instance.AddSnuggles(earned, silent: true);
                EventBus.Publish(new OnOfflineProgressReady
                {
                    SnugglesEarned    = earned,
                    TimeAwaySeconds   = effectiveSeconds
                });
            }
        }

        // ── Tick ─────────────────────────────────────────────────────────────
        private void Update()
        {
            _tickAccumulator += Time.deltaTime;
            if (_tickAccumulator >= TickInterval)
            {
                float delta = _tickAccumulator;
                _tickAccumulator = 0f;
                Tick(delta);
            }
        }

        private void Tick(float deltaSeconds)
        {
            float purrMult = PurrPowerSystem.Instance != null
                ? PurrPowerSystem.Instance.CurrentMultiplier
                : 1f;

            // Snuggles production
            double snugglesThisTick = _snugglesPerSecond * deltaSeconds * purrMult;
            if (snugglesThisTick > 0)
                ResourceManager.Instance.AddSnuggles(snugglesThisTick);

            // Blueprints production
            double bpThisTick = (_blueprintsPerHour / 3600.0) * deltaSeconds;
            if (bpThisTick > 0)
                ResourceManager.Instance.AddBlueprints(bpThisTick);

            // Tick cat happiness (handled by CatManager)
            CatManager.Instance?.TickHappiness(deltaSeconds);

            // Report mission progress
            MissionSystem.Instance?.TrackProduction(snugglesThisTick);
        }

        // ── Recalculate ───────────────────────────────────────────────────────
        public void RecalculateProduction()
        {
            if (_config == null || _save == null) return;

            _snugglesPerSecond = 0;
            _blueprintsPerHour = 0;

            var habitatMgr    = HabitatManager.Instance;
            var volunteerMgr  = VolunteerSystem.Instance;

            foreach (var catEntry in _save.ownedCats)
            {
                if (string.IsNullOrEmpty(catEntry.assignedHabitatId)) continue;

                CatData catData = FindCatData(catEntry.catId);
                if (catData == null) continue;

                double baseProd = catData.GetProductionForLevel(catEntry.level);
                double happinessMult = Mathf.Lerp(0.1f, 1f, catEntry.happiness / catData.maxHappiness);

                // Habitat multiplier
                float habitatMult = 1f;
                if (habitatMgr != null)
                    habitatMult = habitatMgr.GetProductionMultiplier(catEntry.assignedHabitatId, catEntry.catId);

                // Volunteer bonus
                float volunteerBonus = 1f;
                if (volunteerMgr != null)
                    volunteerBonus += volunteerMgr.GetProductionBonus(catEntry.assignedHabitatId);

                _snugglesPerSecond += baseProd * happinessMult * habitatMult * volunteerBonus;
            }

            // Habitat passive bonuses
            if (habitatMgr != null)
                _snugglesPerSecond += habitatMgr.GetTotalPassiveBonus();

            // Volunteer blueprint gen
            if (volunteerMgr != null)
                _blueprintsPerHour += volunteerMgr.GetTotalBlueprintGen();

            Debug.Log($"[IdleManager] Recalculated: {NumberFormatter.Format(_snugglesPerSecond)} Snuggles/s, {NumberFormatter.Format(_blueprintsPerHour)} BP/h");
        }

        private CatData FindCatData(string id)
        {
            if (_config.allCats == null) return null;
            foreach (var c in _config.allCats)
                if (c != null && c.catId == id) return c;
            return null;
        }
    }
}
