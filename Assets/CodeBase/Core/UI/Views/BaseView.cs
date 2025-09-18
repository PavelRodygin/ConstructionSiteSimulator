using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Core.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        protected CanvasGroup CanvasGroup;
        protected Canvas Canvas;
        
        [field: SerializeField] public BaseAnimationElement AnimationElement { get; private set; }
        public bool IsActive { get; protected set; } = true;
        public bool IsInteractable { get; private set; }

        protected virtual void Awake()
        {
            CanvasGroup = GetComponent<CanvasGroup>();
            Canvas = GetComponent<Canvas>();
        }

        public virtual async UniTask Show()
        {
            SetActive(true);
            if (IsActive && AnimationElement)
                await AnimationElement.Show();

            IsInteractable = true;
        }

        public virtual async UniTask Hide()
        {
            IsInteractable = false;

            if (IsActive && AnimationElement)
                await AnimationElement.Hide();
            SetActive(false);
        }
        
        protected void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;
            IsActive = isActive;
            if (Canvas != null) Canvas.enabled = isActive;

            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = isActive ? 1 : 0;
                CanvasGroup.blocksRaycasts = isActive;
                CanvasGroup.interactable = isActive;
            }

            gameObject.SetActive(isActive);
        }
        
        public virtual void HideInstantly() => SetActive(false);
        
        public virtual void Dispose() { } 
    }
}