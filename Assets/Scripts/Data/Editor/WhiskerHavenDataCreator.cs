#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using WhiskerHaven.Data;

/// <summary>
/// Editor utility: creates all ScriptableObject data assets (cats, habitats, missions, achievements)
/// from scratch. Run once via menu: WhiskerHaven > Create All Data Assets
/// </summary>
public static class WhiskerHavenDataCreator
{
    private const string CatsPath        = "Assets/ScriptableObjects/Cats";
    private const string HabitatsPath    = "Assets/ScriptableObjects/Habitats";
    private const string MissionsPath    = "Assets/ScriptableObjects/Missions";
    private const string AchievementsPath = "Assets/ScriptableObjects/Achievements";
    private const string ConfigPath      = "Assets/Resources/Config";
    private const string VolunteersPath  = "Assets/ScriptableObjects/Volunteers";

    [MenuItem("WhiskerHaven/Create All Data Assets")]
    public static void CreateAll()
    {
        EnsureDir(CatsPath);
        EnsureDir(HabitatsPath);
        EnsureDir(MissionsPath);
        EnsureDir(AchievementsPath);
        EnsureDir(ConfigPath);
        EnsureDir(VolunteersPath);

        var cats        = CreateCats();
        var habitats    = CreateHabitats();
        var missions    = CreateMissions();
        var achievements = CreateAchievements();
        var volunteers  = CreateVolunteers();
        CreateGameConfig(cats, habitats, missions, achievements, volunteers);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[WhiskerHaven] All data assets created successfully!");
    }

    // ── CATS ─────────────────────────────────────────────────────────────────
    private static CatData[] CreateCats()
    {
        var defs = new[]
        {
            // Common (8)
            ("Mittens",     CatRarity.Common,   CatPersonality.Lazy,     50.0,  0.0,  1,  1.0, "The quintessential cozy cat. Mittens takes 90% of her naps in sunbeams and 10% on your keyboard.",  "null"),
            ("Biscuit",     CatRarity.Common,   CatPersonality.Playful,  60.0,  0.0,  1,  1.2, "Named for his kneading habit. Biscuit makes excellent biscuits on every soft surface he encounters.", "null"),
            ("Luna",        CatRarity.Common,   CatPersonality.Curious,  55.0,  0.0,  1,  1.1, "Luna investigates every corner of the sanctuary. Science is important, she insists.",                "null"),
            ("Shadow",      CatRarity.Common,   CatPersonality.Grumpy,   45.0,  0.0,  1,  0.9, "Shadow disapproves of Mondays, Tuesdays, and most of the other days too. Yet she purrs.",           "null"),
            ("Cream Puff",  CatRarity.Common,   CatPersonality.Sweet,    65.0,  0.0,  1,  1.3, "Cream Puff is so sweet she actually makes other cats slightly sweeter. Scientifically unproven.",    "null"),
            ("Patches",     CatRarity.Common,   CatPersonality.Wild,     50.0,  0.0,  1,  1.0, "Every day is an adventure for Patches. She has mapped every inch of the sanctuary twice.",           "null"),
            ("Mochi",       CatRarity.Common,   CatPersonality.Lazy,     55.0,  0.0,  1,  1.0, "Mochi is soft, round, and produces an incredible amount of purring for her size.",                   "null"),
            ("Ginger",      CatRarity.Common,   CatPersonality.Playful,  60.0,  0.0,  1,  1.2, "Ginger was technically rescued from a cardboard box but insists she rescued the box.",               "null"),
            // Uncommon (5)
            ("Professor",   CatRarity.Uncommon, CatPersonality.Curious,  200.0, 0.0,  2,  1.8, "The Professor has opinions about everything. Nobody knows what field he is a professor of.",         "null"),
            ("Duchess",     CatRarity.Uncommon, CatPersonality.Royal,    250.0, 0.0,  2,  2.0, "Duchess requires her food to be served at exactly 18°C and pretends she doesn't know what kibble is.", "null"),
            ("Pixel",       CatRarity.Uncommon, CatPersonality.Curious,  180.0, 0.0,  2,  1.7, "Pixel was born in a server room and still dreams in binary. Her purrs sound slightly digital.",      "null"),
            ("Stormy",      CatRarity.Uncommon, CatPersonality.Wild,     220.0, 0.0,  2,  1.9, "Stormy showed up during a thunderstorm and has been plotting something ever since.",                  "null"),
            ("Butterscotch",CatRarity.Uncommon, CatPersonality.Sweet,    230.0, 0.0,  2,  2.0, "Butterscotch has been known to hug volunteers and once comforted a crying mailman.",                  "null"),
            // Rare (1)
            ("Astrid",      CatRarity.Rare,     CatPersonality.Royal,    1000.0,5.0,  3,  4.0, "Astrid descended from a long line of sanctuary founders. She radiates an aura of productive calm.",   "null"),
            // Easter egg Uncommon for variety
            ("Noodle",      CatRarity.Uncommon, CatPersonality.Lazy,     190.0, 0.0,  2,  1.6, "Noodle is extremely long and drapes herself across furniture like a particularly fluffy scarf.",       "null"),
        };

        var results = new CatData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var (name, rarity, personality, costS, costGP, hallLvl, prod, desc, preferredHabitat) = defs[i];
            string id = name.ToLower().Replace(" ", "_");
            var asset = LoadOrCreate<CatData>($"{CatsPath}/Cat_{id}.asset");

            asset.catId                  = id;
            asset.catName                = name;
            asset.description            = desc;
            asset.rarity                 = rarity;
            asset.personality            = personality;
            asset.rescueCostSnuggles     = costS;
            asset.rescueCostGoldenPaw    = costGP;
            asset.requiredHallLevel      = hallLvl;
            asset.baseProductionPerSecond = prod;
            asset.purrPowerContribution  = rarity == CatRarity.Rare ? 3f : rarity == CatRarity.Uncommon ? 2f : 1f;
            asset.happinessGainRate      = 0.5f;
            asset.maxHappiness           = 100f;
            asset.personalityProductionMult = GetPersonalityMult(personality);
            asset.happinessDecayRate     = personality == CatPersonality.Grumpy ? 0.3f : 0.1f;

            EditorUtility.SetDirty(asset);
            results[i] = asset;
        }
        return results;
    }

    private static float GetPersonalityMult(CatPersonality p) => p switch
    {
        CatPersonality.Lazy    => 0.8f,
        CatPersonality.Playful => 1.3f,
        CatPersonality.Grumpy  => 0.7f,
        CatPersonality.Sweet   => 1.2f,
        CatPersonality.Curious => 1.4f,
        CatPersonality.Royal   => 1.5f,
        CatPersonality.Wild    => 1.1f,
        _ => 1f
    };

    // ── HABITATS ─────────────────────────────────────────────────────────────
    private static HabitatData[] CreateHabitats()
    {
        var defs = new[]
        {
            ("sunroom",      "Sunroom",         "A warm, south-facing room drenched in afternoon light. Cats here nap at peak efficiency.", CatPersonality.Lazy,    1, 0,   0,   0.2),
            ("garden",       "Garden",          "A lush outdoor garden with butterflies, birds, and infinite things to investigate.",       CatPersonality.Curious,  1, 300, 5,   0.4),
            ("library",      "Library",         "Lined with books. The Professor has already claimed the best chair.",                      CatPersonality.Royal,    2, 800, 15,  0.6),
            ("kitchen",      "Kitchen",         "Warm, smells of good things, and has exactly the right amount of counter space.",          CatPersonality.Sweet,    2, 800, 15,  0.6),
            ("adventure_room","Adventure Room", "A sprawling indoor jungle gym. Stormy has claimed the highest point.",                    CatPersonality.Wild,     3, 2000,30,  1.0),
            ("royal_suite",  "Royal Suite",     "Gold filigree, velvet cushions, and temperature-controlled air. Duchess approves.",       CatPersonality.Royal,    4, 8000,80,  2.5),
        };

        var results = new HabitatData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var (id, name, desc, personality, hallLvl, unlockS, unlockBP, passiveBonus) = defs[i];
            var asset = LoadOrCreate<HabitatData>($"{HabitatsPath}/Habitat_{id}.asset");

            asset.habitatId              = id;
            asset.habitatName            = name;
            asset.description            = desc;
            asset.requiredHallLevel      = hallLvl;
            asset.unlockCostSnuggles     = unlockS;
            asset.unlockCostBlueprints   = unlockBP;
            asset.passiveBonusPerSecond  = passiveBonus;
            asset.preferredPersonality   = personality;
            asset.habitatThemeColor      = GetHabitatColor(id);

            asset.levels = BuildHabitatLevels(name, unlockS);

            EditorUtility.SetDirty(asset);
            results[i] = asset;
        }
        return results;
    }

    private static HabitatUpgradeLevel[] BuildHabitatLevels(string habitatName, double baseSnuggles)
    {
        var levels = new HabitatUpgradeLevel[10];
        for (int lvl = 1; lvl <= 10; lvl++)
        {
            double cost = baseSnuggles * Mathf.Pow(3f, lvl - 1) + 100;
            levels[lvl - 1] = new HabitatUpgradeLevel
            {
                level               = lvl,
                levelName           = $"{habitatName} Lv.{lvl}",
                upgradeCostSnuggles = cost,
                upgradeCostBlueprints = lvl * 2,
                maxCatSlots         = Mathf.Min(lvl + 1, 8),
                productionMultiplier = 1f + (lvl - 1) * 0.2f,
                upgradeDescription  = $"Increases cat slots to {Mathf.Min(lvl + 2, 8)} and production by {lvl * 20}%."
            };
        }
        return levels;
    }

    private static Color GetHabitatColor(string id) => id switch
    {
        "sunroom"       => new Color(1.0f, 0.9f, 0.6f),
        "garden"        => new Color(0.5f, 0.8f, 0.4f),
        "library"       => new Color(0.6f, 0.5f, 0.3f),
        "kitchen"       => new Color(0.9f, 0.7f, 0.5f),
        "adventure_room"=> new Color(0.4f, 0.7f, 0.9f),
        "royal_suite"   => new Color(0.8f, 0.6f, 0.9f),
        _ => Color.white
    };

    // ── MISSIONS ──────────────────────────────────────────────────────────────
    private static MissionData[] CreateMissions()
    {
        // (id, name, desc, type, condition, target, dailyWeight, rewards)
        var defs = new (string id, string name, string desc, MissionType type, MissionCondition cond, double target, int weight, double rewS, double rewGP, double rewBP)[]
        {
            ("d_snuggles_1",  "Snuggle Collector",    "Earn 1,000 Snuggles today.",           MissionType.Daily, MissionCondition.EarnSnuggles,   1000,  15, 200,  0,  0),
            ("d_snuggles_2",  "Purr Economy",         "Earn 10,000 Snuggles today.",          MissionType.Daily, MissionCondition.EarnSnuggles,  10000,  10, 1500, 0,  2),
            ("d_snuggles_3",  "Snuggle Millionaire",  "Earn 100,000 Snuggles today.",         MissionType.Daily, MissionCondition.EarnSnuggles, 100000,  5,  12000,1,  5),
            ("d_cats_1",      "First Rescue",         "Rescue 1 cat today.",                  MissionType.Daily, MissionCondition.RescueCats,         1,  20, 100,  0,  0),
            ("d_cats_3",      "Triple Rescue",        "Rescue 3 cats today.",                 MissionType.Daily, MissionCondition.RescueCats,         3,  10, 400,  0,  1),
            ("d_habitat_1",   "Home Improvement",     "Upgrade any habitat once today.",      MissionType.Daily, MissionCondition.UpgradeHabitat,     1,  15, 300,  0,  3),
            ("d_habitat_3",   "Builder",              "Upgrade habitats 3 times today.",      MissionType.Daily, MissionCondition.UpgradeHabitat,     3,  8,  1000, 0,  8),
            ("d_volunteer_1", "Call for Help",        "Assign a volunteer to a habitat.",     MissionType.Daily, MissionCondition.AssignVolunteers,   1,  10, 250,  0,  2),
            ("d_purr_2",      "Purr Power!",          "Reach Purr Power multiplier of 2×.",   MissionType.Daily, MissionCondition.ReachPurrPower,    2f,  12, 350,  0,  1),
            ("d_purr_3",      "Triple Purr",          "Reach Purr Power multiplier of 3×.",   MissionType.Daily, MissionCondition.ReachPurrPower,    3f,  8,  800,  1,  3),
        };

        var results = new MissionData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            var asset = LoadOrCreate<MissionData>($"{MissionsPath}/Mission_{d.id}.asset");
            asset.missionId    = d.id;
            asset.missionName  = d.name;
            asset.description  = d.desc;
            asset.type         = d.type;
            asset.condition    = d.cond;
            asset.targetAmount = d.target;
            asset.dailyWeight  = d.weight;
            asset.reward       = new MissionReward { snuggles = d.rewS, goldenPaw = d.rewGP, blueprints = d.rewBP };
            EditorUtility.SetDirty(asset);
            results[i] = asset;
        }
        return results;
    }

    // ── ACHIEVEMENTS ──────────────────────────────────────────────────────────
    private static AchievementData[] CreateAchievements()
    {
        var defs = new (string id, string name, string desc, AchievementCategory cat, MissionCondition cond, double target, double rewS, double rewGP, int pts, string title)[]
        {
            // Collection
            ("first_cat",      "First Resident",       "Rescue your first cat.",                     AchievementCategory.Collection,  MissionCondition.RescueCats,       1,    100,  0,   10, "Cat Whisperer"),
            ("five_cats",      "Growing Family",        "Rescue 5 cats.",                             AchievementCategory.Collection,  MissionCondition.RescueCats,       5,    500,  0,   20, "Animal Friend"),
            ("all_commons",    "Common Collector",      "Rescue all common cats.",                    AchievementCategory.Collection,  MissionCondition.RescueCats,       8,    1000, 0,   30, "Common Ground"),
            ("rare_cat",       "Rarity Seeker",         "Rescue a Rare cat.",                         AchievementCategory.Collection,  MissionCondition.CollectRareCat,   1,    2000, 1,   50, "Rare Taste"),
            ("all_cats",       "The Full Haven",        "Rescue all 15 cats.",                        AchievementCategory.Collection,  MissionCondition.RescueCats,       15,   5000, 5,   100,"Haven Master"),
            // Production
            ("1k_snuggles",    "First Thousand",        "Earn 1,000 lifetime Snuggles.",              AchievementCategory.Production,  MissionCondition.EarnSnuggles,    1000, 200,  0,   10, "Earning Purrs"),
            ("100k_snuggles",  "Snuggle Baron",         "Earn 100,000 lifetime Snuggles.",            AchievementCategory.Production,  MissionCondition.EarnSnuggles,  100000, 2000, 0,   25, "Snuggle Baron"),
            ("1m_snuggles",    "Millionaire Meow",      "Earn 1,000,000 lifetime Snuggles.",          AchievementCategory.Production,  MissionCondition.EarnSnuggles, 1000000, 15000,2,   50, "Millionaire Meow"),
            ("purr_3x",        "Purring Engine",        "Reach a Purr Power multiplier of 3×.",       AchievementCategory.Production,  MissionCondition.ReachPurrPower,  3f,    500,  0,   20, "Purring Engine"),
            ("purr_5x",        "Maximum Purr",          "Reach the maximum Purr Power of 5×.",        AchievementCategory.Production,  MissionCondition.ReachPurrPower,  5f,   2000, 1,   75, "Maximum Purr"),
            // Exploration
            ("first_habitat",  "Home Sweet Home",       "Unlock your first habitat.",                 AchievementCategory.Exploration, MissionCondition.UpgradeHabitat,   1,    150,  0,   10, "Home Builder"),
            ("habitat_5",      "Habitat Hoarder",       "Upgrade any habitat to level 5.",            AchievementCategory.Exploration, MissionCondition.UpgradeHabitat,   5,    1000, 0,   30, "Renovator"),
            ("all_habitats",   "Full House",            "Unlock all 6 habitats.",                     AchievementCategory.Exploration, MissionCondition.UpgradeHabitat,   6,    3000, 2,   50, "Full House"),
            // Social
            ("first_volunteer","Helping Hands",         "Unlock your first volunteer.",               AchievementCategory.Social,      MissionCondition.AssignVolunteers, 1,    200,  0,   15, "Team Player"),
            ("all_volunteers", "Dream Team",            "Unlock all 3 volunteers.",                   AchievementCategory.Social,      MissionCondition.AssignVolunteers, 3,    2500, 1,   40, "Dream Team"),
            // Milestone
            ("login_7",        "Week One",              "Log in 7 days in a row.",                    AchievementCategory.Milestone,   MissionCondition.LoginDays,        7,    500,  0,   25, "Dedicated Keeper"),
            ("login_30",       "A Month of Purrs",      "Log in 30 days.",                            AchievementCategory.Milestone,   MissionCondition.LoginDays,        30,   5000, 3,   75, "Devoted Haven Keeper"),
            ("spend_100bp",    "Blueprint Enthusiast",  "Spend 100 Blueprints.",                      AchievementCategory.Milestone,   MissionCondition.SpendBlueprints, 100,  500,  0,   20, "Architect"),
            ("hall_3",         "Growing Haven",         "Reach Haven Level 3.",                       AchievementCategory.Milestone,   MissionCondition.EarnSnuggles, 2500,   1000, 0,   30, "Haven Architect"),
            ("hall_5",         "Sanctuary Restored",    "Fully restore the Main Hall to level 5.",    AchievementCategory.Milestone,   MissionCondition.EarnSnuggles,  50000, 10000,5,   100,"Sanctuary Keeper"),
        };

        var results = new AchievementData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            var asset = LoadOrCreate<AchievementData>($"{AchievementsPath}/Achievement_{d.id}.asset");
            asset.achievementId     = d.id;
            asset.achievementName   = d.name;
            asset.description       = d.desc;
            asset.category          = d.cat;
            asset.condition         = d.cond;
            asset.targetAmount      = d.target;
            asset.snugglesReward    = d.rewS;
            asset.goldenPawReward   = d.rewGP;
            asset.points            = d.pts;
            asset.rewardTitle       = d.title;
            asset.steamAchievementId = $"ACH_{d.id.ToUpper()}";
            asset.isSteamAchievement = true;
            EditorUtility.SetDirty(asset);
            results[i] = asset;
        }
        return results;
    }

    // ── VOLUNTEERS ────────────────────────────────────────────────────────────
    private static VolunteerData[] CreateVolunteers()
    {
        var defs = new (string id, string name, string bio, VolunteerSpecialty spec, int hallLvl, double costS, double costGP, float prodBonus, float happBonus, double bpGen)[]
        {
            ("felix",   "Felix",   "A retired veterinarian with a gentle touch and encyclopedic knowledge of cat nutrition.", VolunteerSpecialty.Medic,      1, 500,  0, 0.25f, 0.30f, 0.5),
            ("mei",     "Mei",     "A structural engineer by day, passionate cat habitat designer by weekend.",              VolunteerSpecialty.Builder,    2, 0,    2, 0.40f, 0.10f, 2.0),
            ("jasmine", "Jasmine", "A behavioral researcher who can predict what any cat wants before it knows itself.",    VolunteerSpecialty.Researcher, 3, 1500, 1, 0.20f, 0.50f, 1.0),
        };

        var results = new VolunteerData[defs.Length];
        for (int i = 0; i < defs.Length; i++)
        {
            var d = defs[i];
            var asset = LoadOrCreate<VolunteerData>($"{VolunteersPath}/Volunteer_{d.id}.asset");
            asset.volunteerId         = d.id;
            asset.volunteerName       = d.name;
            asset.bio                 = d.bio;
            asset.specialty           = d.spec;
            asset.requiredHallLevel   = d.hallLvl;
            asset.unlockCostSnuggles  = d.costS;
            asset.unlockCostGoldenPaw = d.costGP;
            asset.productionBonus     = d.prodBonus;
            asset.happinessBonus      = d.happBonus;
            asset.blueprintGenPerHour = d.bpGen;
            asset.dailySnugglesCost   = d.costS * 0.01;
            EditorUtility.SetDirty(asset);
            results[i] = asset;
        }
        return results;
    }

    // ── GAME CONFIG ───────────────────────────────────────────────────────────
    private static void CreateGameConfig(CatData[] cats, HabitatData[] habitats,
        MissionData[] missions, AchievementData[] achievements, VolunteerData[] volunteers)
    {
        var asset = LoadOrCreate<GameConfig>($"{ConfigPath}/GameConfig.asset");

        asset.startingSnuggles   = 50;
        asset.startingBlueprints = 10;
        asset.maxOfflineHours    = 8f;
        asset.offlineEfficiency  = 0.5f;
        asset.purrPowerMin       = 1f;
        asset.purrPowerMax       = 5f;
        asset.happinessFullContribThreshold = 80f;
        asset.autoSaveIntervalSeconds = 60f;
        asset.saveFileName       = "whisker_save_v1.json";
        asset.dailyResetHour     = 0;
        asset.skipTutorialInEditor = false;
        asset.maxHallLevel       = 5;
        asset.hallLevelThresholds = new double[] { 0, 500, 2500, 10000, 50000 };
        asset.dailyMissionSlots  = 3;
        asset.allCats            = cats;
        asset.allHabitats        = habitats;
        asset.allMissions        = missions;
        asset.allAchievements    = achievements;
        asset.allVolunteers      = volunteers;

        EditorUtility.SetDirty(asset);
        Debug.Log("[WhiskerHaven] GameConfig created at " + ConfigPath + "/GameConfig.asset");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
