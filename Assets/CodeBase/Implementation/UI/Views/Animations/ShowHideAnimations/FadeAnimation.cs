using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace CodeBase.Implementation.UI.Views.Animations.ShowHideAnimations
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeAnimation : BaseAnimationElement
    {
        [SerializeField] protected CanvasGroup canvasGroup;

        [Header("Animation Parameters")]
        [SerializeField] private float fadeDuration = 0.4f;

        protected void Awake()
        {
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
            if (!canvasGroup) 
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public override async UniTask Show()
        {
            canvasGroup.alpha = 0;
            await canvasGroup.DOFade(1, fadeDuration);
        }

        public override async UniTask Hide()
        {
                await canvasGroup.DOFade(0, fadeDuration);
        }
    }
}