using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Core.UI.Widgets.ProgressBars
{
    public abstract class BaseLegacyProgressBarView : MonoBehaviour, IProgress<float>
    {
        public abstract void Report(float value);
        public abstract void ReportToZero(float value);

        public bool canAnimate;
        public bool canAnimateToZero;
        private float _currentRatio = 0; // Initialized to 0 by default
        private CancellationTokenSource _cts; //  Controls cancellation

        public float CurrentRatio => _currentRatio;

        public async UniTask Animate(float duration, CancellationToken token, float value = 1f)
        {
            ResetAnimation(); // Ensures only one animation runs at a time
            canAnimate = true;
            _cts = CancellationTokenSource.CreateLinkedTokenSource(token); // Allows external control

            var ratio = _currentRatio;
            var multiplier = value / duration;

            try
            {
                while (ratio < value && canAnimate && !_cts.Token.IsCancellationRequested)
                {
                    _cts.Token.ThrowIfCancellationRequested(); // Proper cancellation handling

                    _currentRatio = ratio;
                    ratio += Time.deltaTime * multiplier;
                    Report(ratio);

                    await UniTask.Yield(_cts.Token); // Supports cancellation
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log(" Animation cancelled.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Animation error: {e}");
            }
            finally
            {
                canAnimate = false;

                //  Reset progress ONLY if animation was NOT cancelled
                if (!_cts.Token.IsCancellationRequested)
                {
                    _currentRatio = 0;
                }
            }
        }

        public async UniTask AnimateToZero(float duration, float currentValue)
        {
            canAnimateToZero = true;

            var ratio = currentValue;
            var multiplier = currentValue / duration;

            try
            {
                while (ratio > 0 && canAnimateToZero)
                {
                    ratio -= Time.deltaTime * multiplier;
                    ReportToZero(ratio);
                    await UniTask.Yield();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($" AnimateToZero error: {e}");
            }
            finally
            {
                canAnimateToZero = false;
            }
        }


        public void PauseAnimate()
        {
            _cts?.Cancel(); // Cancels any ongoing animation
            _cts?.Dispose();
            _cts = null;

            canAnimate = false;
            canAnimateToZero = false;
        }

        public void ResumeAnimation()
        {
            if (canAnimate) return; // Prevent multiple resumes

            _cts = new CancellationTokenSource();
            canAnimate = true;
            Animate(_currentRatio, _cts.Token).Forget(); // Uses correct cancellation handling
        }

        private void ResetAnimation()
        {
            _cts?.Cancel(); // Stops any ongoing animation
            _cts?.Dispose();
            _cts = new CancellationTokenSource(); // Creates a fresh cancellation token
        }
    }
}
