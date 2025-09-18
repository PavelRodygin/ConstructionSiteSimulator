using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CodeBase.Implementation.UI.Views.Animations.ShowHideAnimations
{
    // There might be problems when there is ScrollView element on view. Don't use if it's so.
    [RequireComponent(typeof(CanvasGroup))]
    public class BubbleFadeAnimation : BaseAnimationElement
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        [Header("Animation Parameters")]
        [SerializeField] private float scaleUpFactor = 1.1f;
        [SerializeField] private float scaleDuration = 0.25f;
        [SerializeField] private float fadeDuration = 0.25f;
        
        protected void Awake()
        {
            if (!canvasGroup)
                TryGetComponent(out canvasGroup);
            if (!canvasGroup) 
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public override async UniTask Show()
        {
            transform.localScale = Vector3.zero;
            
            Sequence = DOTween.Sequence();
            // plays scaling(firstly makes the object slightly bigger than it should be) and fading animations together,
            // after that scales it to the needed size 
            await Sequence
                .Append(transform.DOScale(scaleUpFactor, scaleDuration / 2))
                .Join(canvasGroup.DOFade(1, fadeDuration))
                .Append(transform.DOScale(1, scaleDuration / 2));
        }

        public override async UniTask Hide()
        {
            Sequence = DOTween.Sequence();
            await Sequence
                .Append(transform.DOScale(scaleUpFactor, scaleDuration / 2))
                .Join(canvasGroup.DOFade(0, fadeDuration))
                .Append(transform.DOScale(0, scaleDuration / 2));
        }
    }
}