using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CodeBase.Core.UI.Widgets.ProgressBars
{
    public abstract class BaseProgressBar : MonoBehaviour
    {
        [SerializeField] protected float animationDuration = 1f;
        private Tween _currentTween;
        private float _currentProgress;

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        /// <param name="targetProgress">The target progress value (0-1).</param>
        /// <returns>UniTask for asynchronous waiting for the progress to update.</returns>
        public async UniTask UpdateProgress(float targetProgress)
        {
            _currentTween?.Kill();

            _currentTween = DOTween.To(GetCurrentProgress, UpdateProgressVisual, targetProgress, animationDuration);
            await _currentTween.ToUniTask();
        }

        /// <summary>
        /// Resets the progress bar to its initial state.
        /// </summary>
        public virtual void ResetProgress()
        {
            _currentTween?.Kill();
            SetProgress(0f);
        }

        /// <summary>
        /// Gets the current progress value.
        /// </summary>
        /// <returns>The current progress value (0-1).</returns>
        protected abstract float GetCurrentProgress();

        /// <summary>
        /// Updates the visual representation of the progress.
        /// </summary>
        /// <param name="progress">The progress value (0-1).</param>
        protected abstract void UpdateProgressVisual(float progress);

        /// <summary>
        /// Sets the progress to a specific value without animation.
        /// </summary>
        /// <param name="progress">The progress value (0-1).</param>
        public abstract void SetProgress(float progress);

        /// <summary>
        /// Sets the display text for the progress bar.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public virtual void SetDisplayValue(string text) { }

        protected float CalculateExponentialProgress(float progress)
        {
            var expValue = Math.Exp(progress);
            var minExp = Math.Exp(0);
            var maxExp = Math.Exp(1);
            return (float)((expValue - minExp) / (maxExp - minExp));
        }
        
        protected void OnDestroy() => _currentTween?.Kill();
    }
}