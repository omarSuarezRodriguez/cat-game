using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class WelcomeBackView : MonoBehaviour
    {
        private TextMeshProUGUI _awayText, _earnedText, _effText;
        private CanvasGroup     _cg;
        private double          _earned;

        private void Awake()
        {
            _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            BuildUI();
        }

        private void BuildUI()
        {
            // Dark backdrop
            var backdrop = Panel(transform, "Backdrop", new Color(0, 0, 0, 0.7f));
            Stretch(backdrop);

            // Modal card
            var card = Panel(transform, "Card", CREAM);
            Center(card, new Vector2(480, 320));
            VLayout(card, 16, new RectOffset(32, 32, 28, 28));

            Text(card.transform, "Title", "🌙  Welcome Back!", 26, TEXT_D, TextAlignmentOptions.Center, FontStyles.Bold);
            _awayText  = Text(card.transform, "Away", "Away for 0s", 15, BG_LIGHT, TextAlignmentOptions.Center);
            LE(_awayText.gameObject, prefH: 24);
            _earnedText = Text(card.transform, "Earned", "+0 Snuggles", 28, SUCCESS, TextAlignmentOptions.Center, FontStyles.Bold);
            LE(_earnedText.gameObject, prefH: 36);
            _effText    = Text(card.transform, "Eff", "(50% offline efficiency)", 12, new Color(0.4f, 0.4f, 0.4f), TextAlignmentOptions.Center);
            LE(_effText.gameObject, prefH: 18);

            // Divider
            var div = Panel(card.transform, "Div", new Color(0, 0, 0, 0.1f));
            LE(div, prefH: 2);

            var collectBtn = Btn(card.transform, "CollectBtn", "🐾  Collect & Continue", AMBER, TEXT_L, 16);
            LE(collectBtn.gameObject, prefH: 52);
            collectBtn.onClick.AddListener(Collect);
        }

        public void SetData(double snuggles, double secondsAway)
        {
            _earned = snuggles;
            if (_awayText)  _awayText.text  = $"Away for {NumberFormatter.FormatTime(secondsAway)}";
            if (_earnedText) _earnedText.text = $"+{NumberFormatter.Format(snuggles)} Snuggles";
            float eff = GameManager.Instance?.Config?.offlineEfficiency ?? 0.5f;
            if (_effText) _effText.text = $"({(int)(eff * 100)}% offline efficiency)";
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _cg.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float t = 0;
            while (t < 0.35f) { t += Time.deltaTime; _cg.alpha = t / 0.35f; yield return null; }
            _cg.alpha = 1f;
        }

        private void Collect()
        {
            AudioManager.Instance?.PlaySFX("coin_collect");
            gameObject.SetActive(false);
        }
    }
}
