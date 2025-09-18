using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Core.UI.Widgets.Buttons
{
    public class PulsatingButton : MonoBehaviour
    {
        public Button pulsatingButton;
        private Sequence _animationSequence;

        public void PlayAnimation()
        {
            //Ease-Out - The tween starts fast and slows down as it approaches the end
            //Quad - the rate of change follows a quadratic equation (t^2)
            // SetLoops(-1, LoopType.Restart): 
            // -1: Makes the tween loop forever.
            // LoopType.Restart: The tween resets to its initial state and starts over each time the loop begins
            _animationSequence = DOTween.Sequence();
            _animationSequence.Append(pulsatingButton.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutQuad))
                .Append(pulsatingButton.transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutQuad))
                .SetLoops(-1, LoopType.Restart);
            
            _animationSequence.Play();
        }

        public void StopAnimation()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
            {
                _animationSequence.Kill();
                pulsatingButton.transform.localScale = Vector3.one; // Сброс масштаба до исходного (returning to the initial scale)
            }
        }

        private void OnDestroy()
        {
            StopAnimation();
        }
    }
}