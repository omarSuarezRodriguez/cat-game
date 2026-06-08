using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class MissionCardUI : MonoBehaviour
    {
        [SerializeField] private Image           icon;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private Slider          progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Button          claimBtn;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private GameObject      completedCheck;
        [SerializeField] private Image           cardBg;
        [SerializeField] private Color           defaultBgColor;
        [SerializeField] private Color           completedBgColor;

        public string MissionId { get; private set; }
        private Action _onClaim;

        public void Populate(MissionData data, MissionSaveEntry entry, Action onClaim)
        {
            MissionId = data.missionId;
            _onClaim  = onClaim;

            if (icon)      icon.sprite   = data.icon;
            if (titleText) titleText.text = data.missionName;
            if (descText)  descText.text  = data.description;

            // Reward text
            string reward = "";
            if (data.reward.snuggles   > 0) reward += $"🐾 {NumberFormatter.Format(data.reward.snuggles)}  ";
            if (data.reward.goldenPaw  > 0) reward += $"✨ {NumberFormatter.Format(data.reward.goldenPaw)}  ";
            if (data.reward.blueprints > 0) reward += $"📐 {NumberFormatter.Format(data.reward.blueprints)}";
            if (rewardText) rewardText.text = reward.Trim();

            claimBtn?.onClick.RemoveAllListeners();
            claimBtn?.onClick.AddListener(() => _onClaim?.Invoke());

            Refresh(entry);
        }

        public void Refresh(MissionSaveEntry entry)
        {
            if (entry == null) return;
            var ms   = MissionSystem.Instance;
            var data = ms?.GetData(MissionId);
            if (data == null) return;

            float pct = (float)(entry.progress / data.targetAmount);
            if (progressBar) progressBar.value = Mathf.Clamp01(pct);
            if (progressText)
                progressText.text = $"{NumberFormatter.Format(entry.progress)} / {NumberFormatter.Format(data.targetAmount)}";

            bool completed = entry.isCompleted;
            bool claimed   = entry.rewardClaimed;

            if (completedCheck) completedCheck.SetActive(completed);
            if (claimBtn)
            {
                claimBtn.gameObject.SetActive(completed && !claimed);
                claimBtn.interactable = completed && !claimed;
            }
            if (cardBg) cardBg.color = claimed ? completedBgColor : defaultBgColor;
        }
    }
}
