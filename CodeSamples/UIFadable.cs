using System;
using System.Collections;
using TheUpload.Core.Async;
using UnityEngine;

namespace TheUpload.Core.UI
{
    /*
    a parent class for any fadable UI that you can show/hide with animation
    */
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFadable : MonoBehaviour
    {
        public enum FadableState
        {
            Showing,
            Hiding,
            Shown,
            Hidden,
        }

        [SerializeField]
        private bool _startHidden = true;

        private Coroutine _fadeCoroutine;

        protected float _showingDuration;
        protected float _hidingDuration;

        protected CanvasGroup _canvasGroup;
        protected GameConfig _gameConfig;
        protected RectTransform _rectTransform;

        public CanvasGroup CanvasGroup => _canvasGroup;
        public FadableState State { get; protected set; }
        public bool IsPaused { get; set; }
        public event Action OnShowingEnd;
        public event Action OnHidingEnd;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            if (_startHidden)
            {
                _canvasGroup.alpha = 0;
                State = FadableState.Hidden;
            }
            else
            {
                _canvasGroup.alpha = 1;
                State = FadableState.Shown;
            }
        }

        public virtual void Initialize(GameConfig config)
        {
            _gameConfig = config;
            _showingDuration = config.UIFadeDuration;
            _hidingDuration = config.UIFadeDuration;
        }

        protected virtual void FadeAction(float t)
        {

        }

        public virtual void Show()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
            _fadeCoroutine = this.StartGameCoroutine(ShowAsync());
        }

        public virtual void Hide()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
           
            _fadeCoroutine = this.StartGameCoroutine(HideAsync());
        }

        public virtual void HideQuickly()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _fadeCoroutine = this.StartGameCoroutine(HideAsync(_hidingDuration/1.5f));
        }

        protected virtual IEnumerator FadeIn()
        {
            var time = 0.0f;
            time = _canvasGroup.alpha;
            var duration = _showingDuration;
            var curve = _gameConfig.UIFadeCurve;
            while (time < duration)
            {
                time += Time.deltaTime;
                var t = curve.Evaluate(time / duration);
                _canvasGroup.alpha = t;
                FadeAction(t);
                yield return null;
            }
        }

        protected virtual IEnumerator FadeOut(float duration = -1)
        {
            var time = 1 - _canvasGroup.alpha;
            if (duration == -1)
            {
                duration = _hidingDuration;
            }
            var t = 0.0f;
            var curve = _gameConfig.UIFadeCurve;
            while (time < duration)
            {
                time += Time.deltaTime;
                t = curve.Evaluate((_gameConfig.UIFadeDuration - time) / duration);
                _canvasGroup.alpha = t;
                FadeAction(t);
                yield return null;
            }

            _canvasGroup.alpha = 0.0f;
        }

        private IEnumerator ShowAsync()
        {
            State = FadableState.Showing;
            yield return  FadeIn();
            State = FadableState.Shown;
            OnShowingEnd?.Invoke();
            _fadeCoroutine = null;
        }

        private IEnumerator HideAsync(float duration = -1)
        {
            State = FadableState.Hiding;
            yield return FadeOut(duration);
            State = FadableState.Hidden;
            OnHidingEnd?.Invoke();
            _fadeCoroutine = null;
        }
    }
}
