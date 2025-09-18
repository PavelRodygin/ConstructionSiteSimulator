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
using Unit = R3.Unit;

namespace Modules.Base.ThirdPersonMPModule.Scripts
{
    /// <summary>
    /// Request handler for ThirdPersonMP module operations
    /// </summary>
    public class ThirdPersonMPRequest : IRequest<string> { }

    /// <summary>
    /// Handler for ThirdPersonMP module requests
    /// </summary>
    public class ThirdPersonMPHandler : IRequestHandler<ThirdPersonMPRequest, string>
    {
        public Task<string> Handle(ThirdPersonMPRequest request, CancellationToken cancellationToken) => 
            Task.FromResult("ThirdPersonMP Handler Invoked!");
    }
    
    /// <summary>
    /// Presenter for ThirdPersonMP module that handles business logic and coordinates between Model and View
    /// 
    /// IMPORTANT: This is a thirdPersonMP file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from ThirdPersonMPPresenter to YourModuleNamePresenter
    /// 2. Update namespace Modules.Base.ThirdPersonMPModule.Scripts match your module location
    /// 3. Add your specific business logic and commands
    /// 4. Customize module navigation logic
    /// 5. Implement your specific UI event handling
    /// 6. Add any additional services or systems your module needs
    /// 
    /// NOTE: Navigation to MainMenuModule is already implemented via exit button
    /// </summary>
    public class ConstructionSitePresenter : IDisposable
    {
        private readonly ConstructionSiteModuleModel _constructionSiteModuleModel;
        private readonly ConstructionSiteView _constructionSiteView;
        private readonly AudioSystem _audioSystem;
        private readonly IPopupHub _popupHub;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();

        public ConstructionSitePresenter(
            ConstructionSiteModuleModel constructionSiteModuleModel,
            ConstructionSiteView constructionSiteView,
            AudioSystem audioSystem,
            IPopupHub popupHub)
        {
            _constructionSiteModuleModel = constructionSiteModuleModel ?? throw new ArgumentNullException(nameof(constructionSiteModuleModel));
            _constructionSiteView = constructionSiteView ?? throw new ArgumentNullException(nameof(constructionSiteView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _constructionSiteView.HideInstantly();

            var commands = new ThirdPersonMPCommands(
                _openMainMenuCommand,
                _settingsPopupCommand,
                _toggleSoundCommand
            );

            _constructionSiteView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            // _thirdPersonMPView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _constructionSiteView.Show();
            
            _audioSystem.PlayMainMenuMelody();
        }

        public async UniTask Exit()
        {
            await _constructionSiteView.Hide();
        }
        
        public void HideInstantly() => _constructionSiteView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_constructionSiteModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_constructionSiteModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupButtonClicked())
                .AddTo(_disposables);

            _toggleSoundCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_constructionSiteModuleModel.CommandThrottleDelay))
                .Subscribe(OnSoundToggled)
                .AddTo(_disposables);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }

        private void OnSettingsPopupButtonClicked()
        {
            _popupHub.OpenSettingsPopup();
        }

        private void OnSoundToggled(bool isOn)
        {
            _audioSystem.SetMusicVolume(isOn ? 1f : 0f);
        }
    }
}
