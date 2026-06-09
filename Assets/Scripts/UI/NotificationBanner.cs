using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public enum NotificationType { Default, Achievement, Mission, CatRescue, Warning }

    public class NotificationBanner : MonoBehaviour
    {
        private TextMeshProUGUI _msg;
        private Image           _bg;
        private CanvasGroup     _cg;
        private bool            _showing;

        private readonly Queue<(string msg, NotificationType type)> _queue = new();

        private static readonly Color[] TypeColors =
        {
            new Color(0.18f, 0.10f, 0.06f, 0.95f), // Default
            new Color(0.55f, 0.37f, 0.05f, 0.97f), // Achievement
            new Color(0.15f, 0.42f, 0.15f, 0.97f), // Mission
            new Color(0.32f, 0.14f, 0.48f, 0.97f), // CatRescue
            new Color(0.60f, 0.15f, 0.08f, 0.97f), // Warning
        };

        private void Awake()
        {
            _cg  = gameObject.AddComponent<CanvasGroup>();
            _cg.alpha = 0;

            // Position: top-center, below resource bar
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.2f, 1);
            rt.anchorMax = new Vector2(0.8f, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 52);
            rt.anchoredPosition = new Vector2(0, -72);

            _bg  = gameObject.AddComponent<Image>();
            _bg.color = TypeColors[0];

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 8, 8);
            layout.childForceExpandWidth  = true;
            layout.childForceExpandHeight = true;

            var textGo = Make(transform, "Msg");
            Stretch(textGo);
            _msg = textGo.AddComponent<TextMeshProUGUI>();
            _msg.fontSize  = 15;
            _msg.color     = Color.white;
            _msg.alignment = TextAlignmentOptions.Center;
            _msg.fontStyle = FontStyles.Bold;
            _msg.raycastTarget = false;
        }

        public void Show(string message, NotificationType type = NotificationType.Default)
        {
            _queue.Enqueue((message, type));
            if (!_showing) StartCoroutine(Dequeue());
        }

        private IEnumerator Dequeue()
        {
            while (_queue.Count > 0)
            {
                _showing = true;
                var (msg, type) = _queue.Dequeue();
                _msg.text  = msg;
                _bg.color  = TypeColors[(int)type];
                yield return Fade(0f, 1f, 0.2f);
                yield return new WaitForSeconds(2.8f);
                yield return Fade(1f, 0f, 0.25f);
            }
            _showing = false;
        }

        private IEnumerator Fade(float from, float to, float dur)
        {
            float t = 0;
            while (t < dur) { t += Time.deltaTime; _cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null; }
            _cg.alpha = to;
        }
    }
}
