using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;
using WhiskerHaven.Data;
using WhiskerHaven.Gameplay;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class MissionView : BaseView
    {
        [Header("Daily Missions")]
        [SerializeField] private Transform         dailyMissionParent;
        [SerializeField] private MissionCardUI     missionCardPrefab;
        [SerializeField] private TextMeshProUGUI   resetTimerText;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI   loginStreakText;

        private List<MissionCardUI> _cards = new();
        private float _timerUpdate;

        public void Init()
        {
            EventBus.Subscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Subscribe<OnMissionProgress>(OnMissionProgress);
            EventBus.Subscribe<OnDailyReset>(OnDailyReset);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<OnMissionCompleted>(OnMissionComplete);
            EventBus.Unsubscribe<OnMissionProgress>(OnMissionProgress);
            EventBus.Unsubscribe<OnDailyReset>(OnDailyReset);
        }

        protected override void OnShow() => Rebuild();

        private void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            _timerUpdate += Time.deltaTime;
            if (_timerUpdate >= 1f) { _timerUpdate = 0f; UpdateResetTimer(); }
        }

        private void Rebuild()
        {
            dailyMissionParent.DestroyAllChildren();
            _cards.Clear();

            var ms = MissionSystem.Instance;
            if (ms == null) return;

            var ids = ms.GetActiveDailyIds();
            foreach (var id in ids)
            {
                var data  = ms.GetData(id);
                var entry = ms.GetEntry(id);
                if (data == null || missionCardPrefab == null) continue;

                var card = Instantiate(missionCardPrefab, dailyMissionParent);
                card.Populate(data, entry, () => OnClaimClicked(id));
                _cards.Add(card);
            }

            if (loginStreakText)
                loginStreakText.text = $"Day {GameManager.Instance?.Save?.loginDays ?? 1} streak";

            UpdateResetTimer();
        }

        private void UpdateResetTimer()
        {
            if (resetTimerText == null) return;
            var save = GameManager.Instance?.Save;
            if (save == null || string.IsNullOrEmpty(save.dailyMissionResetTimeUtc)) return;

            if (System.DateTime.TryParse(save.dailyMissionResetTimeUtc, out System.DateTime lastReset))
            {
                var nextReset = lastReset.Date.AddDays(1);
                var remaining = nextReset - System.DateTime.UtcNow;
                if (remaining.TotalSeconds > 0)
                    resetTimerText.text = $"Resets in {NumberFormatter.FormatTime(remaining.TotalSeconds)}";
                else
                    resetTimerText.text = "Refreshing...";
            }
        }

        private void OnClaimClicked(string missionId)
        {
            bool ok = MissionSystem.Instance?.TryClaimReward(missionId) ?? false;
            if (ok) Rebuild();
        }

        private void OnMissionComplete(OnMissionCompleted evt)
        {
            RefreshCard(evt.MissionId);
        }

        private void OnMissionProgress(OnMissionProgress evt)
        {
            RefreshCard(evt.MissionId);
        }

        private void OnDailyReset(OnDailyReset _)
        {
            if (gameObject.activeInHierarchy) Rebuild();
        }

        private void RefreshCard(string id)
        {
            var ms    = MissionSystem.Instance;
            var entry = ms?.GetEntry(id);
            var data  = ms?.GetData(id);
            if (entry == null || data == null) return;

            foreach (var card in _cards)
                if (card.MissionId == id) card.Refresh(entry);
        }
    }
}
