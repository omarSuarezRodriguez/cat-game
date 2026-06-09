using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class FloatingNumberPool : MonoBehaviour
    {
        private readonly Queue<FloatingNumber> _pool = new();
        private FloatingNumber _prefab;

        public void Init()
        {
            // Build inline prefab
            var go = new GameObject("FloatingNumberPrefab");
            go.transform.SetParent(transform, false);
            _prefab = go.AddComponent<FloatingNumber>();
            go.SetActive(false);

            // Pre-warm
            for (int i = 0; i < 12; i++) ReturnToPool(CreateNew());
        }

        private FloatingNumber CreateNew()
        {
            var obj = Instantiate(_prefab, transform);
            obj.gameObject.SetActive(false);
            obj.OnComplete += () => ReturnToPool(obj);
            return obj;
        }

        public void Spawn(Vector3 worldPos, double amount, bool positive)
        {
            if (_pool.Count == 0) ReturnToPool(CreateNew());
            var obj = _pool.Dequeue();

            // Convert world → screen → canvas local
            Vector3 screenPos = Camera.main != null
                ? Camera.main.WorldToScreenPoint(worldPos)
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

            obj.transform.position = screenPos;
            obj.gameObject.SetActive(true);
            obj.Play(amount, positive);
        }

        private void ReturnToPool(FloatingNumber obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public class FloatingNumber : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        public event System.Action OnComplete;

        private void Awake()
        {
            var cg = gameObject.AddComponent<CanvasGroup>();
            var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 40);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero; textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero; textRt.offsetMax = Vector2.zero;

            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize  = 20;
            _label.alignment = TextAlignmentOptions.Center;
            _label.fontStyle = FontStyles.Bold;
            _label.raycastTarget = false;
        }

        public void Play(double amount, bool positive)
        {
            if (_label)
            {
                _label.text  = (positive ? "+" : "-") + NumberFormatter.Format(System.Math.Abs(amount));
                _label.color = positive ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);
            }
            StartCoroutine(Rise());
        }

        private IEnumerator Rise()
        {
            var cg    = GetComponent<CanvasGroup>();
            var start = transform.position;
            float dur = 1.2f, t = 0;

            while (t < dur)
            {
                t += Time.deltaTime;
                float pct = t / dur;
                transform.position = start + Vector3.up * (80f * pct);
                if (cg) cg.alpha = 1f - pct;
                yield return null;
            }
            OnComplete?.Invoke();
        }
    }
}
