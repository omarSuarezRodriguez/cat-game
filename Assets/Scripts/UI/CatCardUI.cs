using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using WhiskerHaven.Data;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Individual cat card in the collection grid.
    /// </summary>
    public class CatCardUI : MonoBehaviour
    {
        [SerializeField] private Image            portrait;
        [SerializeField] private TextMeshProUGUI  nameText;
        [SerializeField] private TextMeshProUGUI  rarityText;
        [SerializeField] private Image            rarityBadge;
        [SerializeField] private Image            lockedOverlay;
        [SerializeField] private GameObject       newBadge;
        [SerializeField] private Button           clickButton;
        [SerializeField] private Image            selectionBorder;

        public string CatId { get; private set; }

        private static readonly Color[] RarityColors =
        {
            new Color(0.67f, 0.67f, 0.67f), // Common
            new Color(0.33f, 0.67f, 0.33f), // Uncommon
            new Color(0.33f, 0.60f, 1.00f), // Rare
            new Color(0.67f, 0.33f, 1.00f), // Epic
            new Color(1.00f, 0.67f, 0.00f), // Legendary
        };

        public void Populate(CatData data, bool owned, Action onClicked)
        {
            CatId = data.catId;

            if (portrait)   portrait.sprite = data.portrait;
            if (nameText)   nameText.text   = data.catName;
            if (rarityText)
            {
                rarityText.text  = data.rarity.ToString();
                rarityText.color = RarityColors[(int)data.rarity];
            }
            if (rarityBadge) rarityBadge.color = RarityColors[(int)data.rarity];
            if (lockedOverlay) lockedOverlay.gameObject.SetActive(!owned);

            var entry = WhiskerHaven.Gameplay.CatManager.Instance?.GetEntry(data.catId);
            if (newBadge) newBadge.SetActive(owned && entry != null && entry.isNew);

            clickButton?.onClick.RemoveAllListeners();
            clickButton?.onClick.AddListener(() => onClicked?.Invoke());

            if (selectionBorder) selectionBorder.gameObject.SetActive(false);
        }

        public void SetOwned(bool owned)
        {
            if (lockedOverlay) lockedOverlay.gameObject.SetActive(!owned);
        }

        public void SetSelected(bool selected)
        {
            if (selectionBorder) selectionBorder.gameObject.SetActive(selected);
        }
    }
}
