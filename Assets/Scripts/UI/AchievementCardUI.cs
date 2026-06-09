using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Data;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class AchievementCardUI : MonoBehaviour
    {
        private TextMeshProUGUI _title, _desc, _pts;
        private Image           _bg, _categoryBadge;
        private GameObject      _unlockedCheck;

        private static readonly Color[] CatColors =
        {
            new Color(0.36f, 0.62f, 0.36f),  // Collection
            new Color(0.75f, 0.55f, 0.20f),  // Production
            new Color(0.30f, 0.55f, 0.90f),  // Exploration
            new Color(0.85f, 0.35f, 0.70f),  // Social
            new Color(0.65f, 0.35f, 0.85f),  // Milestone
        };

        public void Populate(AchievementData data, bool unlocked)
        {
            if (_title == null) Build();
            if (_title)         _title.text          = data.achievementName;
            if (_desc)          _desc.text           = unlocked ? data.description : "???";
            if (_pts)           _pts.text            = $"{data.points} pts";
            if (_bg)            _bg.color            = unlocked ? CREAM : new Color(0.75f, 0.72f, 0.68f);
            if (_unlockedCheck) _unlockedCheck.SetActive(unlocked);
            if (_categoryBadge) _categoryBadge.color = CatColors[(int)data.category];
        }

        private void Build()
        {
            _bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            HLayout(gameObject, 12, new RectOffset(12, 12, 10, 10));

            // Category badge (left color strip)
            var badge = Panel(transform, "Badge", Color.white);
            _categoryBadge = badge.GetComponent<Image>();
            LE(badge, prefW: 6, minW: 6);

            // Content
            var content = Group(transform, "Content");
            LE(content, flexW: 1);
            VLayout(content, 4, new RectOffset(0, 0, 0, 0));

            _title = Text(content.transform, "Title", "", 14, TEXT_D, TextAlignmentOptions.Left, FontStyles.Bold);
            LE(_title.gameObject, prefH: 20);
            _desc  = Text(content.transform, "Desc",  "", 12, BG_LIGHT);
            _desc.enableWordWrapping = true;
            LE(_desc.gameObject, prefH: 32);

            // Right side: pts + check
            var right = Group(transform, "Right");
            LE(right, prefW: 64);
            VLayout(right, 4, new RectOffset(0, 0, 0, 0));

            _pts = Text(right.transform, "Pts", "0 pts", 12, AMBER, TextAlignmentOptions.Right, FontStyles.Bold);
            LE(_pts.gameObject, prefH: 18);

            _unlockedCheck = Panel(right.transform, "Check", SUCCESS.WithAlpha(0.15f));
            LE(_unlockedCheck, prefH: 26, prefW: 26);
            Text(_unlockedCheck.transform, "CheckMark", "✓", 16, SUCCESS, TextAlignmentOptions.Center, FontStyles.Bold);
        }
    }
}
