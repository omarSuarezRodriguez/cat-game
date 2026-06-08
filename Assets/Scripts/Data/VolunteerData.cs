using UnityEngine;

namespace WhiskerHaven.Data
{
    public enum VolunteerSpecialty { Caretaker, Researcher, Builder, Medic }

    [CreateAssetMenu(fileName = "Volunteer_", menuName = "WhiskerHaven/Volunteer Data")]
    public class VolunteerData : ScriptableObject
    {
        [Header("Identity")]
        public string volunteerId;
        public string volunteerName;
        [TextArea(1, 3)] public string bio;
        public Sprite portrait;
        public VolunteerSpecialty specialty;

        [Header("Unlock")]
        public int requiredHallLevel;
        public double unlockCostSnuggles;
        public double unlockCostGoldenPaw;

        [Header("Bonuses")]
        [Tooltip("Production multiplier applied to assigned habitat")]
        public float productionBonus = 0.25f;
        [Tooltip("Happiness gain boost for cats in assigned habitat")]
        public float happinessBonus = 0.15f;
        [Tooltip("Blueprint generation per hour when assigned")]
        public double blueprintGenPerHour = 0.5;

        [Header("Daily Cost")]
        public double dailySnugglesCost;   // upkeep
    }
}
