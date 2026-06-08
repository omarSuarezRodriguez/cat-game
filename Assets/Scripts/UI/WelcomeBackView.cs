using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Modal shown after returning from offline session.
    /// </summary>
    public class WelcomeBackView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI awayTimeText;
        [SerializeField] private TextMeshProUGUI earnedText;
        [SerializeField] private TextMeshProUGUI efficiencyText;
        [SerializeField] private Button          collectButton;
        [SerializeField] private CanvasGroup     canvasGroup;

        private double _earned;

        private void Awake()
        {
            collectButton?.onClick.AddListener(OnCollect);
        }

        public void SetData(double snugglesEarned, double secondsAway)
        {
            _earned = snugglesEarned;
            if (awayTimeText) awayTimeText.text = $"Away for {NumberFormatter.FormatTime(secondsAway)}";
            if (earnedText)   earnedText.text   = $"+{NumberFormatter.Format(snugglesEarned)} Snuggles";
            if (efficiencyText) efficiencyText.text = $"({GameManager.Instance.Config.offlineEfficiency * 100:F0}% efficiency)";
        }

        public void Show()
        {
            gameObject.SetActive(true);
            if (canvasGroup) { canvasGroup.alpha = 0f; canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true; }
            StartCoroutine(FadeIn());
        }

        private System.Collections.IEnumerator FadeIn()
        {
            float t = 0;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                if (canvasGroup) canvasGroup.alpha = t / 0.3f;
                yield return null;
            }
            if (canvasGroup) canvasGroup.alpha = 1f;
        }

        private void OnCollect()
        {
            AudioManager.Instance?.PlaySFX("coin_collect");
            gameObject.SetActive(false);
        }
    }
}
