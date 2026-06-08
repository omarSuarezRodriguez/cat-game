using UnityEngine;

namespace WhiskerHaven.Data
{
    public enum CatRarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum CatPersonality { Lazy, Playful, Grumpy, Sweet, Curious, Royal, Wild }

    [CreateAssetMenu(fileName = "Cat_", menuName = "WhiskerHaven/Cat Data")]
    public class CatData : ScriptableObject
    {
        [Header("Identity")]
        public string catId;
        public string catName;
        [TextArea(2, 4)] public string description;
        public string lore;

        [Header("Visuals")]
        public Sprite portrait;          // 256x256 cat portrait
        public Sprite idleSprite;        // small sprite for habitat display
        public Color accentColor = Color.white;

        [Header("Rarity & Personality")]
        public CatRarity rarity;
        public CatPersonality personality;

        [Header("Rescue")]
        public double rescueCostSnuggles;
        public double rescueCostGoldenPaw;
        [Tooltip("Required Main Hall level to unlock rescue")]
        public int requiredHallLevel = 1;
        [Tooltip("Habitat type this cat thrives in (optional)")]
        public string preferredHabitatId;

        [Header("Production")]
        [Tooltip("Base Snuggles/second while in a habitat")]
        public double baseProductionPerSecond = 1.0;
        [Tooltip("Happiness gained per second naturally")]
        public float happinessGainRate = 0.5f;
        [Tooltip("Max happiness cap")]
        public float maxHappiness = 100f;

        [Header("Purr Power")]
        [Tooltip("Contributes this much to global Purr Power when happy")]
        public float purrPowerContribution = 1f;

        [Header("Personality Traits")]
        [Tooltip("Production multiplier from personality quirk")]
        public float personalityProductionMult = 1f;
        [Tooltip("Happiness decay per second when NOT in preferred habitat")]
        public float happinessDecayRate = 0.1f;

        [Header("Quotes & Flavor")]
        public string[] idleQuotes;

        // Runtime helpers
        public double GetProductionForLevel(int catLevel)
            => baseProductionPerSecond * (1 + (catLevel - 1) * 0.15);

        public string GetRarityColor()
        {
            return rarity switch
            {
                CatRarity.Common    => "#AAAAAA",
                CatRarity.Uncommon  => "#55AA55",
                CatRarity.Rare      => "#5599FF",
                CatRarity.Epic      => "#AA55FF",
                CatRarity.Legendary => "#FFAA00",
                _ => "#FFFFFF"
            };
        }
    }
}
