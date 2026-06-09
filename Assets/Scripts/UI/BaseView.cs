using System.Collections;
using UnityEngine;

namespace WhiskerHaven.UI
{
    /// <summary>
    /// Base class for all full-screen UI panels. Provides show/hide with fade.
    /// Subclasses add their own children in Awake; this only manages the CanvasGroup.
    /// </summary>
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField] protected float fadeDuration = 0.18f;
        protected CanvasGroup _cg;
        private Coroutine _fadeRoutine;

        protected virtual void Awake()
        {
            _cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeTo(1f));
            _cg.interactable  = true;
            _cg.blocksRaycasts = true;
            OnShow();
        }

        public virtual void Hide()
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeAndHide());
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        private IEnumerator FadeTo(float target)
        {
            float start = _cg.alpha, elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _cg.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
                yield return null;
            }
            _cg.alpha = target;
        }

        private IEnumerator FadeAndHide()
        {
            _cg.interactable   = false;
            _cg.blocksRaycasts = false;
            yield return FadeTo(0f);
            gameObject.SetActive(false);
        }
    }
}
