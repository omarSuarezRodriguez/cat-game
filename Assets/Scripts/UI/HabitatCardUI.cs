using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class HabitatCardUI : MonoBehaviour
    {
        [SerializeField] private Image           icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI catCountText;
        [SerializeField] private Image           lockedOverlay;
        [SerializeField] private Button          clickBtn;
        [SerializeField] private Image           cardBackground;

        private HabitatData _data;

        public void Populate(HabitatData data, Action onClicked)
        {
            _data = data;
            clickBtn?.onClick.RemoveAllListeners();
            clickBtn?.onClick.AddListener(() => onClicked?.Invoke());
            Refresh();
        }

        public void Refresh()
        {
            if (_data == null) return;
            var hm    = HabitatManager.Instance;
            var entry = hm?.GetEntry(_data.habitatId);
            bool unlocked = entry?.isUnlocked ?? false;

            if (icon)     icon.sprite = _data.icon;
            if (nameText) nameText.text = _data.habitatName;
            if (levelText) levelText.text = unlocked ? $"Lv.{entry.level}" : "Locked";
            if (lockedOverlay) lockedOverlay.gameObject.SetActive(!unlocked);
            if (cardBackground) cardBackground.color = _data.habitatThemeColor.WithAlpha(unlocked ? 1f : 0.4f);

            if (catCountText && unlocked && entry != null)
            {
                var levelData = _data.GetLevel(entry.level);
                catCountText.text = $"{entry.catIds.Count}/{levelData.maxCatSlots}";
            }
            else if (catCountText) catCountText.text = "";
        }
    }
}
