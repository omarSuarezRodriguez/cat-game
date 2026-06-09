using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class MissionCardUI : MonoBehaviour
    {
        private TextMeshProUGUI _title, _desc, _progressText, _reward;
        private Slider          _progressBar;
        private Button          _claimBtn;
        private Image           _bg;

        public string MissionId { get; private set; }
        private Action _onClaim;

        public void Populate(MissionData data, MissionSaveEntry entry, Action onClaim)
        {
            MissionId = data.missionId;
            _onClaim  = onClaim;
            if (_title == null) Build();

            if (_title) _title.text = data.missionName;
            if (_desc)  _desc.text  = data.description;

            string reward = "";
            if (data.reward.snuggles   > 0) reward += $"🐾 {NumberFormatter.Format(data.reward.snuggles)}  ";
            if (data.reward.goldenPaw  > 0) reward += $"✨ {NumberFormatter.Format(data.reward.goldenPaw)}  ";
            if (data.reward.blueprints > 0) reward += $"📐 {NumberFormatter.Format(data.reward.blueprints)}";
            if (_reward) _reward.text = reward.Trim();

            _claimBtn?.onClick.RemoveAllListeners();
            _claimBtn?.onClick.AddListener(() => _onClaim?.Invoke());

            Refresh(entry);
        }

        public void Refresh(MissionSaveEntry entry)
        {
            if (entry == null || _title == null) return;
            var ms   = MissionSystem.Instance;
            var data = ms?.GetData(MissionId);
            if (data == null) return;

            float pct = (float)(entry.progress / data.targetAmount);
            if (_progressBar)  _progressBar.value = Mathf.Clamp01(pct);
            if (_progressText) _progressText.text = $"{NumberFormatter.Format(entry.progress)} / {NumberFormatter.Format(data.targetAmount)}";

            bool claimed = entry.rewardClaimed;
            if (_claimBtn)
            {
                _claimBtn.gameObject.SetActive(entry.isCompleted && !claimed);
                _claimBtn.interactable = entry.isCompleted && !claimed;
            }
            if (_bg) _bg.color = claimed ? SUCCESS.WithAlpha(0.2f) : CREAM_ALT;
        }

        private void Build()
        {
            _bg = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            _bg.color = CREAM_ALT;
            VLayout(gameObject, 6, new RectOffset(14, 14, 12, 12));

            // Title row
            var titleRow = Group(transform, "TitleRow");
            HLayout(titleRow, 8, new RectOffset(0, 0, 0, 0));
            LE(titleRow, prefH: 24);
            _title = Text(titleRow.transform, "Title", "", 15, TEXT_D, TextAlignmentOptions.Left, FontStyles.Bold);
            _reward = Text(titleRow.transform, "Reward", "", 12, AMBER, TextAlignmentOptions.Right, FontStyles.Bold);
            LE(_reward.gameObject, prefW: 140);

            // Desc
            _desc = Text(transform, "Desc", "", 12, BG_LIGHT, TextAlignmentOptions.Left);
            _desc.enableWordWrapping = true;
            LE(_desc.gameObject, prefH: 18);

            // Progress row
            var progRow = Group(transform, "ProgRow");
            HLayout(progRow, 8, new RectOffset(0, 0, 0, 0));
            LE(progRow, prefH: 22);
            _progressBar  = SliderH(progRow.transform, "Bar", AMBER, 16);
            _progressBar.interactable = false;
            _progressText = Text(progRow.transform, "Text", "0/1", 11, BG_LIGHT, TextAlignmentOptions.Right);
            LE(_progressText.gameObject, prefW: 100);

            // Claim button
            _claimBtn = Btn(transform, "Claim", "✅ Claim Reward", SUCCESS, TEXT_L, 13);
            LE(_claimBtn.gameObject, prefH: 36);
            _claimBtn.gameObject.SetActive(false);
        }
    }
}
