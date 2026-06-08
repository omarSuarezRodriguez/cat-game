using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;

namespace WhiskerHaven.UI
{
    public class SettingsView : BaseView
    {
        [Header("Audio")]
        [SerializeField] private Slider          masterVolumeSlider;
        [SerializeField] private Slider          sfxVolumeSlider;
        [SerializeField] private Slider          musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeLabel;
        [SerializeField] private TextMeshProUGUI sfxVolumeLabel;
        [SerializeField] private TextMeshProUGUI musicVolumeLabel;

        [Header("Save")]
        [SerializeField] private Button          saveNowBtn;
        [SerializeField] private TextMeshProUGUI lastSaveText;
        [SerializeField] private Button          deleteDataBtn;
        [SerializeField] private GameObject      deleteConfirmPanel;
        [SerializeField] private Button          confirmDeleteBtn;
        [SerializeField] private Button          cancelDeleteBtn;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private Button          backBtn;

        public void Init()
        {
            masterVolumeSlider?.onValueChanged.AddListener(v => {
                AudioManager.Instance?.SetMasterVolume(v);
                if (masterVolumeLabel) masterVolumeLabel.text = $"{(int)(v * 100)}%";
            });
            sfxVolumeSlider?.onValueChanged.AddListener(v => {
                AudioManager.Instance?.SetSFXVolume(v);
                if (sfxVolumeLabel) sfxVolumeLabel.text = $"{(int)(v * 100)}%";
            });
            musicVolumeSlider?.onValueChanged.AddListener(v => {
                AudioManager.Instance?.SetMusicVolume(v);
                if (musicVolumeLabel) musicVolumeLabel.text = $"{(int)(v * 100)}%";
            });

            saveNowBtn?.onClick.AddListener(() => {
                SaveSystem.Instance?.Save();
                AudioManager.Instance?.PlaySFX("ui_click");
                RefreshSaveInfo();
            });

            deleteDataBtn?.onClick.AddListener(() => deleteConfirmPanel?.SetActive(true));
            confirmDeleteBtn?.onClick.AddListener(() => {
                SaveSystem.Instance?.DeleteSave();
                deleteConfirmPanel?.SetActive(false);
                AudioManager.Instance?.PlaySFX("ui_click");
            });
            cancelDeleteBtn?.onClick.AddListener(() => deleteConfirmPanel?.SetActive(false));
            backBtn?.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            deleteConfirmPanel?.SetActive(false);
            if (versionText) versionText.text = $"v{Application.version}";

            // Restore slider values
            var am = AudioManager.Instance;
            if (am != null)
            {
                if (masterVolumeSlider) masterVolumeSlider.value = am.masterVolume;
                if (sfxVolumeSlider)    sfxVolumeSlider.value    = am.sfxVolume;
                if (musicVolumeSlider)  musicVolumeSlider.value  = am.musicVolume;
            }
        }

        protected override void OnShow() => RefreshSaveInfo();

        private void RefreshSaveInfo()
        {
            var save = SaveSystem.Instance?.Current;
            if (lastSaveText == null || save == null) return;
            if (System.DateTime.TryParse(save.lastSaveTimeUtc, out System.DateTime dt))
                lastSaveText.text = $"Last saved: {dt.ToLocalTime():g}";
        }
    }
}
