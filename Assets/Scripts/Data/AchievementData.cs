using UnityEngine;

namespace WhiskerHaven.Data
{
    public enum AchievementCategory { Collection, Production, Exploration, Social, Milestone }

    [CreateAssetMenu(fileName = "Achievement_", menuName = "WhiskerHaven/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        [Header("Identity")]
        public string achievementId;
        public string achievementName;
        [TextArea(1, 3)] public string description;
        public Sprite icon;
        public AchievementCategory category;

        [Header("Unlock Condition")]
        public MissionCondition condition;
        public double targetAmount;
        public string targetId;     // optional filter

        [Header("Reward")]
        public double snugglesReward;
        public double goldenPawReward;
        [Tooltip("Reward title / badge shown on profile")]
        public string rewardTitle;

        [Header("Steam")]
        public string steamAchievementId;  // for Steamworks integration
        public bool isSteamAchievement = true;

        [Header("Display")]
        public bool isSecret;
        [Tooltip("Points for leaderboard / prestige")]
        public int points = 10;
    }
}
