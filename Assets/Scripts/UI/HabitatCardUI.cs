using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class HabitatCardUI : MonoBehaviour
    {
        private Image            _bg, _icon;
        private TextMeshProUGUI  _name, _level, _catCount;
        private Image            _lockOverlay;
        private Button           _btn;
        private HabitatData      _data;

        public void Populate(HabitatData data, Action onClicked)
        {
            _data = data;
            if (_btn == null) Build();
            _btn?.onClick.RemoveAllListeners();
            _btn?.onClick.AddListener(() => onClicked?.Invoke());
            Refresh();
        }

        public void Refresh()
        {
            if (_data == null || _btn == null) return;
            var entry = HabitatManager.Instance?.GetEntry(_data.habitatId);
            bool unlocked = entry?.isUnlocked ?? false;

            if (_bg)      _bg.color       = unlocked ? _data.habitatThemeColor.WithAlpha(0.25f) : new Color(0.5f, 0.5f, 0.5f, 0.15f);
            if (_icon)    _icon.sprite    = _data.icon;
            if (_name)    _name.text      = _data.habitatName;
            if (_level)   _level.text     = unlocked ? $"Lv.{entry.level}" : "🔒";
            if (_lockOverlay) _lockOverlay.gameObject.SetActive(!unlocked);

            if (_catCount && unlocked && entry != null)
            {
                var ld = _data.GetLevel(entry.level);
                _catCount.text = $"{entry.catIds.Count}/{ld.maxCatSlots} 🐱";
            }
            else if (_catCount) _catCount.text = "";
        }

        private void Build()
        {
            _bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            _btn = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            _btn.targetGraphic = _bg;
            var nav = _btn.navigation; nav.mode = Navigation.Mode.None; _btn.navigation = nav;
            var colors = _btn.colors;
            colors.highlightedColor = AMBER.WithAlpha(0.2f);
            _btn.colors = colors;

            HLayout(gameObject, 12, new RectOffset(12, 12, 10, 10));

            // Icon
            var iconGo = Panel(transform, "Icon", new Color(0.8f, 0.8f, 0.8f, 0.3f));
            LE(iconGo, prefW: 56, prefH: 56);
            _icon = iconGo.GetComponent<Image>();
            Text(iconGo.transform, "Emoji", "🏠", 28, TEXT_D, TextAlignmentOptions.Center).raycastTarget = false;

            // Content
            var col = Group(transform, "Content");
            LE(col, flexW: 1);
            VLayout(col, 4, new RectOffset(0, 0, 0, 0));
            _name     = Text(col.transform, "Name",    "", 15, TEXT_D, TextAlignmentOptions.Left, FontStyles.Bold);
            _catCount = Text(col.transform, "Cats",    "", 12, BG_LIGHT);
            LE(_name.gameObject, prefH: 22); LE(_catCount.gameObject, prefH: 18);

            // Level badge (right)
            var right = Group(transform, "Right");
            LE(right, prefW: 52);
            _level = Text(right.transform, "Level", "", 14, AMBER, TextAlignmentOptions.Right, FontStyles.Bold);

            // Lock overlay
            _lockOverlay = Panel(transform, "Locked", new Color(0, 0, 0, 0.4f)).GetComponent<Image>();
            Stretch(_lockOverlay.gameObject);
            _lockOverlay.transform.SetSiblingIndex(0);
        }
    }
}
