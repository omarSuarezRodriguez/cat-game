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
    /// <summary>
    /// Cat card in the collection grid.
    /// Self-builds its own UI on first Populate() call if no children exist.
    /// </summary>
    public class CatCardUI : MonoBehaviour
    {
        private Image            _portrait;
        private TextMeshProUGUI  _nameText, _rarityText;
        private Image            _lockedOverlay;
        private GameObject       _newBadge;
        private Button           _btn;

        public string CatId { get; private set; }

        public void Populate(CatData data, bool owned, Action onClicked)
        {
            CatId = data.catId;
            if (_btn == null) Build();

            if (_portrait)    _portrait.sprite  = data.portrait;
            if (_nameText)    _nameText.text     = data.catName;
            if (_rarityText)
            {
                _rarityText.text  = data.rarity.ToString();
                _rarityText.color = RarityColor(data.rarity);
            }
            if (_lockedOverlay) _lockedOverlay.gameObject.SetActive(!owned);

            var entry = CatManager.Instance?.GetEntry(data.catId);
            if (_newBadge) _newBadge.SetActive(owned && entry != null && entry.isNew);

            _btn?.onClick.RemoveAllListeners();
            _btn?.onClick.AddListener(() => onClicked?.Invoke());
        }

        public void SetOwned(bool owned)
        {
            if (_lockedOverlay) _lockedOverlay.gameObject.SetActive(!owned);
        }

        private void Build()
        {
            // Background image + Button
            var bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            bg.color = CREAM_ALT;
            _btn = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            _btn.targetGraphic = bg;
            var nav = _btn.navigation; nav.mode = Navigation.Mode.None; _btn.navigation = nav;
            var colors = _btn.colors;
            colors.highlightedColor = AMBER.WithAlpha(0.3f);
            colors.pressedColor     = AMBER.WithAlpha(0.6f);
            _btn.colors = colors;

            VLayout(gameObject, 6, new RectOffset(8, 8, 8, 8));

            // Portrait area
            var portGo = Panel(transform, "Portrait", new Color(0.8f, 0.8f, 0.8f, 0.4f));
            LE(portGo, prefH: 100, minH: 100);
            _portrait = portGo.GetComponent<Image>();

            // NEW badge
            _newBadge = Panel(portGo.transform, "NewBadge", new Color(0.9f, 0.3f, 0.1f));
            var nbRt = _newBadge.GetComponent<RectTransform>();
            nbRt.anchorMin = new Vector2(1, 1); nbRt.anchorMax = new Vector2(1, 1);
            nbRt.pivot = new Vector2(1, 1);
            nbRt.sizeDelta = new Vector2(36, 20);
            nbRt.anchoredPosition = Vector2.zero;
            Text(_newBadge.transform, "Label", "NEW", 9, TEXT_L, TextAlignmentOptions.Center, FontStyles.Bold);
            _newBadge.SetActive(false);

            // Name
            _nameText = Text(transform, "Name", "", 13, TEXT_D, TextAlignmentOptions.Center, FontStyles.Bold);
            _nameText.enableWordWrapping = false;
            LE(_nameText.gameObject, prefH: 18);

            // Rarity
            _rarityText = Text(transform, "Rarity", "", 11, COMMON, TextAlignmentOptions.Center);
            LE(_rarityText.gameObject, prefH: 14);

            // Lock overlay
            _lockedOverlay = Panel(transform, "Locked", new Color(0, 0, 0, 0.55f)).GetComponent<Image>();
            var lockRt = _lockedOverlay.GetComponent<RectTransform>();
            lockRt.anchorMin = Vector2.zero; lockRt.anchorMax = Vector2.one;
            lockRt.offsetMin = Vector2.zero; lockRt.offsetMax = Vector2.zero;
            _lockedOverlay.transform.SetSiblingIndex(0);
            Text(_lockedOverlay.transform, "Lock", "🔒", 28, TEXT_L, TextAlignmentOptions.Center);
        }
    }
}
