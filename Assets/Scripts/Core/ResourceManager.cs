using UnityEngine;
using WhiskerHaven.Utils;

namespace WhiskerHaven.Core
{
    /// <summary>
    /// Single source of truth for all currency operations.
    /// All modifications MUST go through this manager.
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        private SaveData _save;

        public double Snuggles    => _save?.snuggles ?? 0;
        public double GoldenPaw   => _save?.goldenPaw ?? 0;
        public double Blueprints  => _save?.blueprints ?? 0;
        public double LifetimeSnuggles => _save?.lifetimeSnuggles ?? 0;

        public void Init(SaveData save) => _save = save;

        // ── Add ─────────────────────────────────────────────────────────────
        public void AddSnuggles(double amount, bool silent = false)
        {
            if (amount <= 0) return;
            _save.snuggles += amount;
            _save.lifetimeSnuggles += amount;
            if (!silent)
                EventBus.Publish(new OnResourceChanged { Type = ResourceType.Snuggles, NewAmount = _save.snuggles, Delta = amount });
        }

        public void AddGoldenPaw(double amount, bool silent = false)
        {
            if (amount <= 0) return;
            _save.goldenPaw += amount;
            if (!silent)
                EventBus.Publish(new OnResourceChanged { Type = ResourceType.GoldenPaw, NewAmount = _save.goldenPaw, Delta = amount });
        }

        public void AddBlueprints(double amount, bool silent = false)
        {
            if (amount <= 0) return;
            _save.blueprints += amount;
            if (!silent)
                EventBus.Publish(new OnResourceChanged { Type = ResourceType.Blueprints, NewAmount = _save.blueprints, Delta = amount });
        }

        // ── Spend ────────────────────────────────────────────────────────────
        public bool TrySpendSnuggles(double amount)
        {
            if (_save.snuggles < amount) return false;
            _save.snuggles -= amount;
            EventBus.Publish(new OnResourceChanged { Type = ResourceType.Snuggles, NewAmount = _save.snuggles, Delta = -amount });
            return true;
        }

        public bool TrySpendGoldenPaw(double amount)
        {
            if (_save.goldenPaw < amount) return false;
            _save.goldenPaw -= amount;
            _save.totalGoldenPawSpent += amount;
            EventBus.Publish(new OnResourceChanged { Type = ResourceType.GoldenPaw, NewAmount = _save.goldenPaw, Delta = -amount });
            return true;
        }

        public bool TrySpendBlueprints(double amount)
        {
            if (_save.blueprints < amount) return false;
            _save.blueprints -= amount;
            _save.totalBlueprintsSpent += amount;
            EventBus.Publish(new OnResourceChanged { Type = ResourceType.Blueprints, NewAmount = _save.blueprints, Delta = -amount });
            return true;
        }

        // ── Checks ───────────────────────────────────────────────────────────
        public bool CanAfford(double snuggles = 0, double goldenPaw = 0, double blueprints = 0)
            => _save.snuggles >= snuggles &&
               _save.goldenPaw >= goldenPaw &&
               _save.blueprints >= blueprints;

        public bool TrySpend(double snuggles = 0, double goldenPaw = 0, double blueprints = 0)
        {
            if (!CanAfford(snuggles, goldenPaw, blueprints)) return false;
            if (snuggles > 0)   TrySpendSnuggles(snuggles);
            if (goldenPaw > 0)  TrySpendGoldenPaw(goldenPaw);
            if (blueprints > 0) TrySpendBlueprints(blueprints);
            return true;
        }

        public string GetFormattedSnuggles()   => NumberFormatter.Format(Snuggles);
        public string GetFormattedGoldenPaw()  => NumberFormatter.Format(GoldenPaw);
        public string GetFormattedBlueprints() => NumberFormatter.Format(Blueprints);
    }
}
