using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;

namespace WhiskerHaven.UI
{
    public class AchievementView : BaseView
    {
        [Header("List")]
        [SerializeField] private Transform            listParent;
        [SerializeField] private AchievementCardUI    cardPrefab;
        [SerializeField] private TMP_Dropdown         categoryFilter;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI      unlockedCountText;
        [SerializeField] private TextMeshProUGUI      totalPointsText;
        [SerializeField] private Slider               completionBar;

        public void Init()
        {
            categoryFilter?.onValueChanged.AddListener(_ => Rebuild());
            EventBus.Subscribe<OnAchievementUnlocked>(_ => { if (gameObject.activeInHierarchy) Rebuild(); });
        }

        protected override void OnShow() => Rebuild();

        private void Rebuild()
        {
            listParent.DestroyAllChildren();
            var ach   = AchievementSystem.Instance;
            var all   = ach?.GetAll();
            if (all == null) return;

            int cat = categoryFilter?.value ?? 0;

            int totalPoints    = 0;
            int unlockedPoints = 0;

            foreach (var data in all)
            {
                if (data == null) continue;
                if (cat > 0 && (int)data.category != cat - 1) continue;

                bool unlocked = ach.IsUnlocked(data.achievementId);
                if (data.isSecret && !unlocked) continue;

                var card = Instantiate(cardPrefab, listParent);
                card.Populate(data, unlocked);

                totalPoints += data.points;
                if (unlocked) unlockedPoints += data.points;
            }

            if (unlockedCountText)
                unlockedCountText.text = $"{ach?.UnlockedCount ?? 0}/{all.Count}";
            if (totalPointsText)
                totalPointsText.text = $"{unlockedPoints} pts";
            if (completionBar)
                completionBar.value = all.Count > 0 ? (float)(ach?.UnlockedCount ?? 0) / all.Count : 0;
        }
    }
}
