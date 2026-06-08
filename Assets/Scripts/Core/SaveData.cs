using System;
using System.Collections.Generic;

namespace WhiskerHaven.Core
{
    [Serializable]
    public class SaveData
    {
        // ── Meta ────────────────────────────────────────────────────────────
        public int saveVersion = 1;
        public string lastSaveTimeUtc;   // ISO 8601

        // ── Resources ───────────────────────────────────────────────────────
        public double snuggles;
        public double goldenPaw;
        public double blueprints;
        public double lifetimeSnuggles;

        // ── Progression ─────────────────────────────────────────────────────
        public int hallLevel = 1;
        public int loginDays;
        public string lastLoginDateUtc;
        public bool tutorialComplete;
        public int tutorialStep;

        // ── Cats ────────────────────────────────────────────────────────────
        public List<CatSaveEntry> ownedCats = new();

        // ── Habitats ────────────────────────────────────────────────────────
        public List<HabitatSaveEntry> habitats = new();

        // ── Volunteers ──────────────────────────────────────────────────────
        public List<VolunteerSaveEntry> volunteers = new();

        // ── Missions ────────────────────────────────────────────────────────
        public List<MissionSaveEntry> missionProgress = new();
        public string dailyMissionResetTimeUtc;
        public List<string> activeDailyMissionIds = new();

        // ── Achievements ────────────────────────────────────────────────────
        public List<string> unlockedAchievementIds = new();

        // ── Stats (for achievements) ─────────────────────────────────────────
        public double totalCatsRescued;
        public double totalHabitatsUpgraded;
        public double totalBlueprintsSpent;
        public double totalGoldenPawSpent;
        public float maxPurrPowerReached;
    }

    [Serializable]
    public class CatSaveEntry
    {
        public string catId;
        public int level;
        public float happiness;
        public string assignedHabitatId;
        public string rescueDateUtc;
        public bool isNew;              // shows "new" badge in collection
    }

    [Serializable]
    public class HabitatSaveEntry
    {
        public string habitatId;
        public bool isUnlocked;
        public int level;
        public List<string> catIds = new();
    }

    [Serializable]
    public class VolunteerSaveEntry
    {
        public string volunteerId;
        public bool isUnlocked;
        public string assignedHabitatId;
    }

    [Serializable]
    public class MissionSaveEntry
    {
        public string missionId;
        public double progress;
        public bool isCompleted;
        public bool rewardClaimed;
    }
}
