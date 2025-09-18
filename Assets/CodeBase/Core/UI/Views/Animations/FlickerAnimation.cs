using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Core.UI.Views.Animations
{
    public class FlickerAnimation
    {
        private readonly CanvasGroup _lightingCanvasGroup;
        private readonly Image _overlay;
        private const float FlickerDuration = 0.2f;

        public FlickerAnimation(CanvasGroup lightingCanvasGroup, Image overlay)
        {
            _lightingCanvasGroup = lightingCanvasGroup;
            _overlay = overlay;
        }

        /// <summary>
        /// Creates a continuous flickering effect by alternating the opacity of UI elements.
        /// The flickering effect involves fading a lighting canvas group and an overlay back and forth,
        /// with random opacity and delays for a natural look. The process continues until a cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that allows external control to stop the flickering effect gracefully.
        /// </param>
        /// <remarks>
        /// This method uses DOTween animations with the <see cref="Ease.Flash"/> easing function to create a flashing effect.
        /// Randomized opacity and delay values ensure the flickering appears dynamic and unpredictable.
        /// </remarks>
        public async UniTaskVoid StartFlickering(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //The Ease enum in DOTween defines a variety of easing functions that control the rate of change
                //of a tween's value over time
                const Ease easy = Ease.Flash;
                var opacity = UnityEngine.Random.Range(0f, 0.3f);

                await UniTask.WhenAll(
                    _lightingCanvasGroup
                        .DOFade(opacity, FlickerDuration)
                        .SetEase(easy)
                        .ToUniTask(cancellationToken: cancellationToken),
                    _overlay
                        .DOFade(1 - opacity, FlickerDuration)
                        .SetEase(easy)
                        .ToUniTask(cancellationToken: cancellationToken)
                );

                //reverse animation of the previous WhenAll above
                await UniTask.WhenAll(
                    _lightingCanvasGroup
                        .DOFade(1, FlickerDuration)
                        .SetEase(easy)
                        .ToUniTask(cancellationToken: cancellationToken),
                    _overlay
                        .DOFade(0, FlickerDuration)
                        .SetEase(easy)
                        .ToUniTask(cancellationToken: cancellationToken)
                );

                //Adds a random delay between 0.2 and 0.8 seconds before the next flickering cycle starts
                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(0.2f, 0.8f)),
                    cancellationToken: cancellationToken);
            }
        }
    }
}