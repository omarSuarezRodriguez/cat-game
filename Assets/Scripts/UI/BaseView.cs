using System.Collections;
using UnityEngine;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Base class for all UI panels. Provides show/hide with fade animation.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField] protected float fadeDuration = 0.2f;
        protected CanvasGroup _canvasGroup;
        private Coroutine _fadeRoutine;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeTo(1f));
            OnShow();
        }

        public virtual void Hide()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeAndHide());
            OnHide();
        }

        protected virtual void OnShow()  { }
        protected virtual void OnHide()  { }

        private IEnumerator FadeTo(float target)
        {
            float start   = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = target;
        }

        private IEnumerator FadeAndHide()
        {
            yield return FadeTo(0f);
            gameObject.SetActive(false);
        }

        protected void SetInteractable(bool value)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable  = value;
            _canvasGroup.blocksRaycasts = value;
        }
    }
}
