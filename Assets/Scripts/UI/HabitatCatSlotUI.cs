using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Gameplay;

namespace WhiskerHaven.UI
{
    public class HabitatCatSlotUI : MonoBehaviour
    {
        [SerializeField] private Image           catPortrait;
        [SerializeField] private TextMeshProUGUI catNameText;
        [SerializeField] private GameObject      emptySlotIndicator;
        [SerializeField] private Button          removeBtn;
        [SerializeField] private Slider          miniHappinessBar;

        private string _catId;
        private string _habitatId;

        public void Populate(string catId, string habitatId)
        {
            _catId     = catId;
            _habitatId = habitatId;

            bool hasCat = !string.IsNullOrEmpty(catId);
            if (emptySlotIndicator) emptySlotIndicator.SetActive(!hasCat);
            if (catPortrait)        catPortrait.gameObject.SetActive(hasCat);
            if (catNameText)        catNameText.gameObject.SetActive(hasCat);
            if (removeBtn)          removeBtn.gameObject.SetActive(hasCat);
            if (miniHappinessBar)   miniHappinessBar.gameObject.SetActive(hasCat);

            if (!hasCat) return;

            var data  = CatManager.Instance?.GetData(catId);
            var entry = CatManager.Instance?.GetEntry(catId);

            if (catPortrait && data != null)  catPortrait.sprite = data.portrait;
            if (catNameText && data != null)  catNameText.text   = data.catName;
            if (miniHappinessBar && entry != null && data != null)
                miniHappinessBar.value = entry.happiness / data.maxHappiness;

            removeBtn?.onClick.RemoveAllListeners();
            removeBtn?.onClick.AddListener(OnRemove);
        }

        private void OnRemove()
        {
            CatManager.Instance?.UnassignCat(_catId);
            Populate(null, _habitatId);
        }
    }
}
