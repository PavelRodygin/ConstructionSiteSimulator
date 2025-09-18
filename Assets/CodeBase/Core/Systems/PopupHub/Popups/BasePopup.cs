using System.Threading.Tasks;
using CodeBase.Core.UI;
using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace CodeBase.Core.Systems.PopupHub.Popups
{
    //TODO вынести системы в глобальный контекст (Scripts) чтобы их мог видеть кор и импоементейшн
    //TODO не, не надо. Абстракция(кор) не должен знать о уже реализованных системах. Надо что-то придумать. Точно! Интерфей для попап хаба и прочего.
    //Это позволит нам создавать хорошие базовые классы работающие с абстрацкией модулей и нужных систем!
    public class BasePopup : MonoBehaviour
    {
        [Inject][HideInInspector] public BasePopupCanvas Canvas;
        //[Inject] protected ISoundService soundService;
        [Inject] protected IPopupHub PopupHub;
        
        [SerializeField] protected Transform overlayTransform;
        [SerializeField] protected Transform spinnerTransform;
        [SerializeField] private BaseAnimationElement animationElement;
        
        public Button closeButton;
        
        private TaskCompletionSource<bool> Tcs => _tcs ??= new TaskCompletionSource<bool>();

        //TODO ДОДЕЛАТЬ! Сделать абстракцию для этой очереди
        [SerializeField] protected PopupsPriority priority = PopupsPriority.Medium;
        public PopupsPriority Priority => priority;

        private bool _isClosed;
        private TaskCompletionSource<bool> _tcs;

        protected virtual void Awake()
        {
            gameObject.SetActive(false);
     
            if (closeButton != null)
                closeButton.onClick.AddListener(() => Close().Forget());
        }
        
        protected void ShowSpinner()
        {
            overlayTransform.gameObject.SetActive(true);
            spinnerTransform.gameObject.SetActive(true);
            spinnerTransform.DORotate(new Vector3(0, 0, -360), 1.5f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental);
        }
        
        protected void HideSpinner()
        {
            overlayTransform.gameObject.SetActive(false);
            spinnerTransform.gameObject.SetActive(false);
            spinnerTransform.DOKill();
        }

        public virtual UniTask Open<T>(T param)
        {
            gameObject.SetActive(true);

            return animationElement.Show();
        }

        public virtual async UniTask Close()
        {
            if(_isClosed)  // if a popup is already closed, return
                return;
            try
            {
                _isClosed = true;
                //soundService.Play(GeneralSoundTypes.GeneralPopupClose).Forget();
                await animationElement.Hide();
                transform.DOKill();
            }
            finally
            {
                Tcs?.TrySetResult(true);
                Destroy(gameObject);
                PopupHub.NotifyPopupClosed(); 
            }
        }
        
        public Task<bool> WaitForCompletion() => Tcs.Task;

        private void OnDestroy() => _isClosed = true;
    }
}
