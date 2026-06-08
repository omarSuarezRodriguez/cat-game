using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WhiskerHaven.Utils;

namespace WhiskerHaven.UI
{
    public class FloatingNumberPool : MonoBehaviour
    {
        [SerializeField] private FloatingNumber prefab;
        [SerializeField] private int poolSize = 15;
        [SerializeField] private Canvas worldCanvas;

        private Queue<FloatingNumber> _pool = new();

        public void Init()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.gameObject.SetActive(false);
                obj.OnComplete += () => ReturnToPool(obj);
                _pool.Enqueue(obj);
            }
        }

        public void Spawn(Vector3 worldPos, double amount, bool positive)
        {
            if (_pool.Count == 0) return;
            var obj = _pool.Dequeue();

            Vector3 screenPos = Camera.main != null
                ? Camera.main.WorldToScreenPoint(worldPos)
                : worldPos;

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
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float           riseDuration  = 1.2f;
        [SerializeField] private float           riseDistance  = 80f;
        [SerializeField] private Color           positiveColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] private Color           negativeColor = new Color(1f, 0.4f, 0.4f);

        public event System.Action OnComplete;

        public void Play(double amount, bool positive)
        {
            if (label)
            {
                label.text  = (positive ? "+" : "-") + NumberFormatter.Format(System.Math.Abs(amount));
                label.color = positive ? positiveColor : negativeColor;
            }
            StartCoroutine(Rise());
        }

        private IEnumerator Rise()
        {
            Vector3 start = transform.position;
            float elapsed = 0f;
            var cg = GetComponent<CanvasGroup>();

            while (elapsed < riseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / riseDuration;
                transform.position = start + Vector3.up * (riseDistance * t);
                if (cg) cg.alpha = 1f - t;
                yield return null;
            }

            OnComplete?.Invoke();
        }
    }

}
