using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SettingsView : BaseView
    {
        private Slider          _masterSlider, _sfxSlider, _musicSlider;
        private TextMeshProUGUI _masterLabel, _sfxLabel, _musicLabel;
        private TextMeshProUGUI _lastSaveText, _versionText;
        private GameObject      _deleteConfirm;

        protected override void Awake() { base.Awake(); BuildUI(); }
        protected override void OnShow() => RefreshSave();

        private void BuildUI()
        {
            var root = Panel(transform, "BG", BG_MED); Stretch(root);

            // Header
            var header = Panel(root.transform, "Header", BG_DARK);
            AnchorTop(header, 56);
            HLayout(header, 12, new RectOffset(16, 16, 8, 8));
            Text(header.transform, "Title", "⚙️  Settings", 20, CREAM, TextAlignmentOptions.Left, FontStyles.Bold);
            var sp = Group(header.transform, "Sp"); LE(sp, flexW: 1);
            var back = Btn(header.transform, "Back", "← Back", AMBER_D, TEXT_L, 13);
            LE(back.gameObject, prefW: 90, prefH: 38);
            back.onClick.AddListener(() => FindFirstObjectByType<UIManager>()?.GoBack());

            // Content
            var scroll = Group(root.transform, "Content");
            StretchWithMargin(scroll, 56, 0);
            var (_, content) = ScrollV(scroll.transform, "SettingsScroll");
            VLayout(content.gameObject, 16, new RectOffset(40, 40, 24, 24));
            Fitter(content.gameObject);

            // Audio section
            Section(content.transform, "🔊  Audio");

            (_masterSlider, _masterLabel) = VolumeRow(content.transform, "Master Volume", 1f, v => AudioManager.Instance?.SetMasterVolume(v));
            (_sfxSlider,    _sfxLabel)    = VolumeRow(content.transform, "SFX Volume",    0.8f, v => AudioManager.Instance?.SetSFXVolume(v));
            (_musicSlider,  _musicLabel)  = VolumeRow(content.transform, "Music Volume",  0.5f, v => AudioManager.Instance?.SetMusicVolume(v));

            // Save section
            Section(content.transform, "💾  Save");

            var saveRow = Group(content.transform, "SaveRow");
            LE(saveRow, prefH: 44);
            HLayout(saveRow, 16, new RectOffset(0, 0, 0, 0));
            _lastSaveText = Text(saveRow.transform, "LastSave", "Last saved: never", 13, CREAM_ALT);
            var saveNowBtn = Btn(saveRow.transform, "SaveNow", "Save Now", SUCCESS, TEXT_L, 13);
            LE(saveNowBtn.gameObject, prefW: 120, prefH: 40);
            saveNowBtn.onClick.AddListener(() => { SaveSystem.Instance?.Save(); RefreshSave(); AudioManager.Instance?.PlaySFX("ui_click"); });

            // Danger zone
            Section(content.transform, "⚠️  Danger Zone");

            var delBtn = Btn(content.transform, "DeleteBtn", "🗑 Delete All Data", DANGER, TEXT_L, 13);
            LE(delBtn.gameObject, prefH: 44);
            delBtn.onClick.AddListener(() => _deleteConfirm?.SetActive(true));

            // Delete confirm modal (in-panel)
            _deleteConfirm = Panel(content.transform, "DeleteConfirm", new Color(0.6f, 0, 0, 0.9f));
            LE(_deleteConfirm, prefH: 120);
            VLayout(_deleteConfirm, 12, new RectOffset(20, 20, 16, 16));
            _deleteConfirm.SetActive(false);
            Text(_deleteConfirm.transform, "Q", "Are you sure? This cannot be undone.", 14, TEXT_L, TextAlignmentOptions.Center);
            var btnRow = Group(_deleteConfirm.transform, "BtnRow");
            HLayout(btnRow, 16, new RectOffset(0, 0, 0, 0));
            LE(btnRow, prefH: 40);
            var confirmBtn = Btn(btnRow.transform, "ConfirmDel", "Yes, Delete", DANGER, TEXT_L, 13);
            confirmBtn.onClick.AddListener(() => { SaveSystem.Instance?.DeleteSave(); _deleteConfirm.SetActive(false); });
            var cancelBtn = Btn(btnRow.transform, "Cancel", "Cancel", BG_LIGHT, TEXT_L, 13);
            cancelBtn.onClick.AddListener(() => _deleteConfirm.SetActive(false));

            // Version
            _versionText = Text(content.transform, "Version", $"Whisker Haven  v{Application.version}", 11, new Color(1, 1, 1, 0.4f), TextAlignmentOptions.Center);
            LE(_versionText.gameObject, prefH: 20);
        }

        private void Section(Transform parent, string label)
        {
            var sep = Panel(parent, "Sep", new Color(1, 1, 1, 0.1f));
            LE(sep, prefH: 2);
            var lbl = Text(parent, "SectionLabel", label, 15, AMBER, TextAlignmentOptions.Left, FontStyles.Bold);
            LE(lbl.gameObject, prefH: 24);
        }

        private (Slider slider, TextMeshProUGUI label) VolumeRow(Transform parent, string name, float defaultVal, System.Action<float> onChange)
        {
            var row = Group(parent, name + "Row");
            LE(row, prefH: 50);
            VLayout(row, 4, new RectOffset(0, 0, 0, 0));

            var labelRow = Group(row.transform, "LabelRow");
            HLayout(labelRow, 0, new RectOffset(0, 0, 0, 0));
            LE(labelRow, prefH: 22);
            Text(labelRow.transform, "Name", name, 13, CREAM_ALT);
            var valLabel = Text(labelRow.transform, "Value", $"{(int)(defaultVal * 100)}%", 13, GOLD, TextAlignmentOptions.Right, FontStyles.Bold);
            LE(valLabel.gameObject, prefW: 50);

            var slider = SliderH(row.transform, "Slider", AMBER, 16);
            slider.value = defaultVal;
            LE(slider.gameObject, prefH: 20);
            slider.onValueChanged.AddListener(v =>
            {
                valLabel.text = $"{(int)(v * 100)}%";
                onChange?.Invoke(v);
            });

            return (slider, valLabel);
        }

        private void RefreshSave()
        {
            if (_lastSaveText == null) return;
            var save = SaveSystem.Instance?.Current;
            if (save == null || string.IsNullOrEmpty(save.lastSaveTimeUtc)) { _lastSaveText.text = "Last saved: never"; return; }
            if (System.DateTime.TryParse(save.lastSaveTimeUtc, out var dt))
                _lastSaveText.text = $"Last saved: {dt.ToLocalTime():g}";
        }
    }
}
