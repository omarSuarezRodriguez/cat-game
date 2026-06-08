using UnityEngine;

namespace WhiskerHaven.Data
{
    public enum MissionType { Daily, Story, Achievement, Event }
    public enum MissionCondition
    {
        EarnSnuggles,
        RescueCats,
        UpgradeHabitat,
        ReachPurrPower,
        CollectRareCat,
        AssignVolunteers,
        SpendBlueprints,
        LoginDays,
        EarnGoldenPaw
    }

    [System.Serializable]
    public class MissionReward
    {
        public double snuggles;
        public double goldenPaw;
        public double blueprints;
        public int xp;
        [Tooltip("Optional specific cat unlock reward")]
        public CatData catReward;
    }

    [CreateAssetMenu(fileName = "Mission_", menuName = "WhiskerHaven/Mission Data")]
    public class MissionData : ScriptableObject
    {
        [Header("Identity")]
        public string missionId;
        public string missionName;
        [TextArea(2, 3)] public string description;
        public Sprite icon;

        [Header("Type & Condition")]
        public MissionType type;
        public MissionCondition condition;
        public double targetAmount;
        [Tooltip("Optional: specific cat/habitat ID to narrow condition")]
        public string targetId;

        [Header("Reward")]
        public MissionReward reward;

        [Header("Daily Config")]
        [Tooltip("Weight for daily random selection (higher = more likely)")]
        public int dailyWeight = 10;

        [Header("Story Config")]
        public int storyOrder;
        [Tooltip("Required completed missions before this unlocks")]
        public string[] prerequisiteMissionIds;
    }
}
