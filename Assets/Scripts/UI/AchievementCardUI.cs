using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Data;

namespace WhiskerHaven.UI
{
    public class AchievementCardUI : MonoBehaviour
    {
        [SerializeField] private Image           icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private Image           lockedOverlay;
        [SerializeField] private GameObject      unlockedCheck;
        [SerializeField] private Image           categoryBadge;

        private static readonly Color[] CategoryColors =
        {
            new Color(0.4f, 0.8f, 0.4f),  // Collection
            new Color(0.8f, 0.6f, 0.2f),  // Production
            new Color(0.4f, 0.6f, 1.0f),  // Exploration
            new Color(1.0f, 0.4f, 0.8f),  // Social
            new Color(0.8f, 0.4f, 1.0f),  // Milestone
        };

        public void Populate(AchievementData data, bool unlocked)
        {
            if (icon)        icon.sprite          = data.icon;
            if (titleText)   titleText.text        = data.achievementName;
            if (descText)    descText.text          = unlocked ? data.description : "???";
            if (pointsText)  pointsText.text        = $"{data.points} pts";
            if (lockedOverlay) lockedOverlay.gameObject.SetActive(!unlocked);
            if (unlockedCheck) unlockedCheck.SetActive(unlocked);
            if (categoryBadge) categoryBadge.color  = CategoryColors[(int)data.category];
        }
    }
}
