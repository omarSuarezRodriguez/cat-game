using UnityEngine;

namespace WhiskerHaven.Data
{
    [System.Serializable]
    public class HabitatUpgradeLevel
    {
        public int level;
        public string levelName;
        public double upgradeCostSnuggles;
        public double upgradeCostBlueprints;
        public int maxCatSlots;
        public float productionMultiplier;
        [TextArea(1, 2)] public string upgradeDescription;
        public Sprite levelSprite;        // visual for this level
    }

    [CreateAssetMenu(fileName = "Habitat_", menuName = "WhiskerHaven/Habitat Data")]
    public class HabitatData : ScriptableObject
    {
        [Header("Identity")]
        public string habitatId;
        public string habitatName;
        [TextArea(2, 3)] public string description;
        public Sprite icon;

        [Header("Unlock")]
        public int requiredHallLevel = 1;
        public double unlockCostSnuggles;
        public double unlockCostBlueprints;

        [Header("Levels")]
        public HabitatUpgradeLevel[] levels;   // index 0 = level 1

        [Header("Production Bonus")]
        [Tooltip("Additional Snuggles/sec bonus this habitat gives passively")]
        public double passiveBonusPerSecond = 0.5;
        [Tooltip("Type of cats that get a 2× bonus here")]
        public CatPersonality preferredPersonality;

        [Header("Visuals")]
        public Color habitatThemeColor = Color.white;

        public HabitatUpgradeLevel GetLevel(int level)
        {
            int idx = Mathf.Clamp(level - 1, 0, levels.Length - 1);
            return levels[idx];
        }

        public bool CanUpgrade(int currentLevel) => currentLevel < levels.Length;

        public int MaxLevel => levels.Length;
    }
}
