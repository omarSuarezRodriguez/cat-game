using System.Collections.Generic;
using UnityEngine;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Utils;

namespace WhiskerHaven.Gameplay
{
    /// <summary>
    /// Aggregates cat happiness into a global multiplier.
    /// Purr Power = 1x (no cats) → 5x (all cats max happiness).
    /// </summary>
    public class PurrPowerSystem : Singleton<PurrPowerSystem>
    {
        private GameConfig _config;
        private SaveData   _save;

        private float _currentMultiplier = 1f;
        private float _normalizedHappiness = 0f;  // 0–1

        public float CurrentMultiplier  => _currentMultiplier;
        public float NormalizedHappiness => _normalizedHappiness;
        public float PurrPowerPercent   => (_currentMultiplier - _config.purrPowerMin) /
                                           (_config.purrPowerMax  - _config.purrPowerMin);

        public void Init(GameConfig config, SaveData save)
        {
            _config = config;
            _save   = save;
            Recalculate();
        }

        public void Recalculate()
        {
            if (_config == null || _save == null || _save.ownedCats == null || _save.ownedCats.Count == 0)
            {
                _currentMultiplier  = _config != null ? _config.purrPowerMin : 1f;
                _normalizedHappiness = 0f;
                return;
            }

            float totalContrib   = 0f;
            float maxContrib     = 0f;
            float fullThreshold  = _config.happinessFullContribThreshold;

            foreach (var cat in _save.ownedCats)
            {
                CatData data = FindCat(cat.catId);
                if (data == null) continue;

                float happinessPct  = Mathf.Clamp01(cat.happiness / data.maxHappiness);
                float contribution  = data.purrPowerContribution;

                totalContrib += happinessPct * contribution;
                maxContrib   += contribution;
            }

            _normalizedHappiness = maxContrib > 0 ? totalContrib / maxContrib : 0f;
            _currentMultiplier   = Mathf.Lerp(_config.purrPowerMin, _config.purrPowerMax, _normalizedHappiness);

            if (_currentMultiplier > _save.maxPurrPowerReached)
                _save.maxPurrPowerReached = _currentMultiplier;

            EventBus.Publish(new OnPurrPowerChanged { Multiplier = _currentMultiplier });
        }

        private CatData FindCat(string id)
        {
            foreach (var c in _config.allCats)
                if (c != null && c.catId == id) return c;
            return null;
        }
    }
}
