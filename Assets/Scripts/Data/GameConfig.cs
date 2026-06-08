using UnityEngine;

namespace WhiskerHaven.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "WhiskerHaven/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Economy Balancing")]
        public double startingSnuggles = 50;
        public double startingBlueprints = 10;
        [Tooltip("Max offline progress hours")]
        public float maxOfflineHours = 8f;
        [Tooltip("Offline production efficiency (0–1)")]
        [Range(0f, 1f)] public float offlineEfficiency = 0.5f;

        [Header("Purr Power")]
        [Tooltip("Minimum multiplier (no happy cats)")]
        public float purrPowerMin = 1f;
        [Tooltip("Maximum multiplier (all cats max happy)")]
        public float purrPowerMax = 5f;
        [Tooltip("Happiness threshold to contribute full Purr Power")]
        [Range(0f, 100f)] public float happinessFullContribThreshold = 80f;

        [Header("Save System")]
        public float autoSaveIntervalSeconds = 60f;
        public string saveFileName = "whisker_save_v1.json";

        [Header("Daily Reset")]
        [Tooltip("UTC hour for daily mission reset")]
        [Range(0, 23)] public int dailyResetHour = 0;

        [Header("Tutorial")]
        public bool skipTutorialInEditor = false;

        [Header("Progression")]
        public int maxHallLevel = 5;
        [Tooltip("Snuggles needed to reach each hall level")]
        public double[] hallLevelThresholds = { 0, 500, 2500, 10000, 50000 };

        [Header("Cats")]
        public CatData[] allCats;

        [Header("Habitats")]
        public HabitatData[] allHabitats;

        [Header("Missions")]
        public MissionData[] allMissions;

        [Header("Achievements")]
        public AchievementData[] allAchievements;

        [Header("Volunteers")]
        public VolunteerData[] allVolunteers;

        [Header("Mission Pool")]
        [Tooltip("How many daily missions are active at once")]
        public int dailyMissionSlots = 3;

        public static GameConfig Load()
        {
            return Resources.Load<GameConfig>("Config/GameConfig");
        }
    }
}
