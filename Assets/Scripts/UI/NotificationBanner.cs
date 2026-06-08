using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerHaven.UI
{
    public enum NotificationType { Default, Achievement, Mission, CatRescue, Warning }

    public class NotificationBanner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image           bannerBg;
        [SerializeField] private Image           iconImage;
        [SerializeField] private Sprite          achievementIcon;
        [SerializeField] private Sprite          missionIcon;
        [SerializeField] private Sprite          catIcon;
        [SerializeField] private float           displayDuration = 3f;

        private static readonly Color[] TypeColors =
        {
            new Color(0.2f, 0.2f, 0.2f, 0.9f),   // Default
            new Color(0.6f, 0.4f, 0.0f, 0.95f),  // Achievement
            new Color(0.2f, 0.5f, 0.2f, 0.95f),  // Mission
            new Color(0.4f, 0.2f, 0.6f, 0.95f),  // CatRescue
            new Color(0.7f, 0.2f, 0.1f, 0.95f),  // Warning
        };

        private Queue<(string msg, NotificationType type)> _queue = new();
        private bool _showing;

        public void Show(string message, NotificationType type = NotificationType.Default)
        {
            _queue.Enqueue((message, type));
            if (!_showing) StartCoroutine(ShowNext());
        }

        private IEnumerator ShowNext()
        {
            while (_queue.Count > 0)
            {
                _showing = true;
                var (msg, type) = _queue.Dequeue();

                if (messageText) messageText.text = msg;
                if (bannerBg)    bannerBg.color   = TypeColors[(int)type];
                if (iconImage)
                {
                    iconImage.sprite = type switch
                    {
                        NotificationType.Achievement => achievementIcon,
                        NotificationType.Mission     => missionIcon,
                        NotificationType.CatRescue   => catIcon,
                        _ => null
                    };
                    iconImage.gameObject.SetActive(iconImage.sprite != null);
                }

                var cg = GetComponent<CanvasGroup>();
                if (cg) { cg.alpha = 0f; cg.gameObject.SetActive(true); }

                // Fade in
                yield return Fade(cg, 0f, 1f, 0.2f);
                yield return new WaitForSeconds(displayDuration);
                // Fade out
                yield return Fade(cg, 1f, 0f, 0.3f);
                if (cg) cg.gameObject.SetActive(false);
            }
            _showing = false;
        }

        private IEnumerator Fade(CanvasGroup cg, float from, float to, float dur)
        {
            if (cg == null) yield break;
            float t = 0;
            while (t < dur)
            {
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, t / dur);
                yield return null;
            }
            cg.alpha = to;
        }
    }
}
