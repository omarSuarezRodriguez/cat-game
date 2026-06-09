using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Gameplay;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class HabitatCatSlotUI : MonoBehaviour
    {
        private Image            _portrait;
        private TextMeshProUGUI  _name;
        private Slider           _happBar;
        private Button           _removeBtn;
        private GameObject       _emptySlot;

        private string _catId, _habitatId;

        public void Populate(string catId, string habitatId)
        {
            _catId     = catId;
            _habitatId = habitatId;
            if (_portrait == null) Build();

            bool hasCat = !string.IsNullOrEmpty(catId);
            _emptySlot?.SetActive(!hasCat);
            if (_portrait)  _portrait.gameObject.SetActive(hasCat);
            if (_name)      _name.gameObject.SetActive(hasCat);
            if (_happBar)   _happBar.gameObject.SetActive(hasCat);
            if (_removeBtn) _removeBtn.gameObject.SetActive(hasCat);

            if (!hasCat) return;
            var data  = CatManager.Instance?.GetData(catId);
            var entry = CatManager.Instance?.GetEntry(catId);
            if (_portrait && data != null) _portrait.sprite = data.portrait;
            if (_name && data != null)     _name.text       = data.catName;
            if (_happBar && entry != null && data != null)
                _happBar.value = entry.happiness / data.maxHappiness;

            _removeBtn?.onClick.RemoveAllListeners();
            _removeBtn?.onClick.AddListener(() => { CatManager.Instance?.UnassignCat(_catId); Populate(null, _habitatId); });
        }

        private void Build()
        {
            var bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            bg.color = CREAM_ALT;
            VLayout(gameObject, 3, new RectOffset(4, 4, 4, 4));

            // Empty slot indicator
            _emptySlot = Group(gameObject, "EmptySlot");
            Text(_emptySlot.transform, "Plus", "+", 24, new Color(0, 0, 0, 0.2f), TextAlignmentOptions.Center);
            LE(_emptySlot, prefH: 44);

            // Cat portrait
            var portGo = Panel(transform, "Portrait", new Color(0.8f, 0.8f, 0.8f, 0.4f));
            LE(portGo, prefH: 40); _portrait = portGo.GetComponent<Image>();

            _name    = Text(transform, "Name", "", 9, TEXT_D, TextAlignmentOptions.Center);
            LE(_name.gameObject, prefH: 12);

            _happBar = SliderH(transform, "HappBar", SUCCESS, 8);
            _happBar.interactable = false;
            LE(_happBar.gameObject, prefH: 8);

            _removeBtn = Btn(transform, "Remove", "✕", DANGER, TEXT_L, 10);
            LE(_removeBtn.gameObject, prefH: 18);
        }
    }
}
