using System;
using System.Collections.Generic;
using System.Threading;
using CodeBase.Core.Patterns.ObjectCreation;
using CodeBase.Core.Systems.PopupHub;
using CodeBase.Core.Systems.PopupHub.Popups;
using CodeBase.Core.UI;
using CodeBase.Services;
using CodeBase.Services.EventMediator;
using CodeBase.Systems.PopupHub.Popups.FirstPopup;
using CodeBase.Systems.PopupHub.Popups.RebindingPopup;
using CodeBase.Systems.PopupHub.Popups.SecondPopup;
using CodeBase.Systems.PopupHub.Popups.SettingsPopup;
using CodeBase.Systems.PopupHub.Popups.ThirdPopup;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace CodeBase.Systems.PopupHub
{
    //TODO Действия аналогичные с ModuleCanvas, или придумать что-то более подходящее

    public class PopupHub : IPopupHub
    {
        [NonSerialized] public BasePopup CurrentPopup;

        [Inject] private BasePopupCanvas _canvas;

        [Inject] private IBasePopupFactory<FirstPopup> _firstPopupFactory;
        [Inject] private IBasePopupFactory<SecondPopup> _secondPopupFactory;
        [Inject] private IBasePopupFactory<ThirdPopup> _thirdPopupFactory;
        [Inject] private IBasePopupFactory<SettingsPopup> _settingsPopupFactory;
        //[Inject] private IBasePopupFactory<RebindingPopup> _rebindingPopupFactory;
        [Inject] private EventMediator _eventMediator;

        private readonly PopupsPriorityQueue _popup = new();
        private readonly Stack<BasePopup> _popups = new();
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        private void CreateAndOpenPopup(IFactory<Transform, BasePopup> basePopupFactory)
        {
            var popup = basePopupFactory.Create(_canvas.PopupParent);
            EnqueuePopup(popup);
        }

        private void CreateAndOpenPopup<T>(IFactory<Transform, BasePopup> basePopupFactory, T param)
        {
            var popup = basePopupFactory.Create(_canvas.PopupParent);
            EnqueuePopup(popup, param);
        }

        private void EnqueuePopup(BasePopup popup)
        {
            _popup.Enqueue(popup); 
            TryOpenNextPopup().Forget();
        }

        private void EnqueuePopup<T>(BasePopup popup, T param)
        {
            _popup.Enqueue(popup);  
            TryOpenNextPopup(param).Forget();
        }

        public async UniTask TryOpenNextPopup() => await TryOpenNextPopup<object>(null);

        public async UniTask TryOpenNextPopup<T>(T param)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                if (CurrentPopup == null && _popup.TryDequeue(out var nextPopup)) 
                {
                    CurrentPopup = nextPopup;
                    _popups.Push(CurrentPopup);
                    CurrentPopup.gameObject.SetActive(true);
                    await CurrentPopup.Open(param);
                    _eventMediator.Publish(new PopupOpenedEvent(CurrentPopup.GetType().Name));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while opening popup '{CurrentPopup.name}' : {ex.Message}");
            }
            finally { _semaphoreSlim.Release(); }
        }


        public async UniTask CloseCurrentPopup()
        {
            if (CurrentPopup != null)
            {
                await CurrentPopup.Close();
                CurrentPopup = null;
                TryOpenNextPopup().Forget();  
            }
        }

        public void NotifyPopupClosed()
        {
            CurrentPopup = null;
            TryOpenNextPopup().Forget();
        }

        public void OpenFirstPopup() => CreateAndOpenPopup(_firstPopupFactory);
        public void OpenSecondPopup() => CreateAndOpenPopup(_secondPopupFactory);
        public void OpenThirdPopup() => CreateAndOpenPopup(_thirdPopupFactory);
        public void OpenSettingsPopup () => CreateAndOpenPopup(_settingsPopupFactory);
    }
}
