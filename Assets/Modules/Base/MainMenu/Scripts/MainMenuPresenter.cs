using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using MediatR;
using R3;
using UnityEngine;
using VContainer;
using Unit = R3.Unit;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuRequest : IRequest<string> { }

    public class MainMenuHandler : IRequestHandler<MainMenuRequest, string>
    {
        public Task<string> Handle(MainMenuRequest request, CancellationToken cancellationToken) => 
            Task.FromResult("MainMenu Handler Invoked!");
    }
    
    public class MainMenuPresenter : IDisposable
    {
        [Inject] private IMediator _mediator;
        private readonly MainMenuModuleModel _mainMenuModuleModel;
        private readonly MainMenuView _mainMenuView;
        private readonly IPopupHub _popupHub;
        private readonly AudioSystem _audioSystem;
        
        private readonly ReactiveCommand<Unit> _openConstructionSiteCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<Unit> _secondPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        
        public MainMenuPresenter(IPopupHub popupHub, MainMenuModuleModel mainMenuModuleModel, 
            MainMenuView mainMenuView, AudioSystem audioSystem)
        {
            _mainMenuModuleModel = mainMenuModuleModel ?? throw new ArgumentNullException(nameof(mainMenuModuleModel));
            _mainMenuView = mainMenuView ?? throw new ArgumentNullException(nameof(mainMenuView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));

            SubscribeToUIUpdates();
        }

        private void SubscribeToUIUpdates()
        {
            _openConstructionSiteCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnConstructionSiteCommand())
                .AddTo(_disposables);
            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupCommand())
                .AddTo(_disposables);
            _secondPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSecondPopupCommand())
                .AddTo(_disposables);
            _toggleSoundCommand.Subscribe(OnToggleSoundCommand).AddTo(_disposables);
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _mainMenuView.HideInstantly();

            var commands = new MainMenuCommands(
                _openConstructionSiteCommand,
                _settingsPopupCommand,
                _secondPopupCommand,
                _toggleSoundCommand
            );

            _mainMenuView.SetupEventListeners(commands);

            _mainMenuView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _mainMenuView.Show();
            
            var result = await _mediator.Send(new MainMenuRequest()); 
            Debug.Log($"MediatR request result: {result}");
            
            _audioSystem.PlayMainMenuMelody();
        }
        
        public async UniTask Exit()
        {
            await _mainMenuView.Hide();
            _audioSystem.StopMusic();
        }
        
        public void HideInstantly() => _mainMenuView.HideInstantly();

        public void Dispose()
        {
            _disposables?.Dispose();
            _mainMenuView?.Dispose();
            _mainMenuModuleModel?.Dispose();
        }

        private void OnConstructionSiteCommand() => _openNewModuleCommand.Execute(ModulesMap.ConstructionSite);
        private void OnSettingsPopupCommand() => _popupHub.OpenSettingsPopup();
        private void OnSecondPopupCommand() => _popupHub.OpenSecondPopup();
        private void OnToggleSoundCommand(bool isOn) => _audioSystem.SetMusicVolume(isOn ? 1 : 0);
    }
}