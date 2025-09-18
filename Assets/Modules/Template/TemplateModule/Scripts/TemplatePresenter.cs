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

namespace Modules.Template.TemplateModule.Scripts
{
    /// <summary>
    /// Request handler for Template module operations
    /// </summary>
    public class TemplateRequest : IRequest<string> { }

    /// <summary>
    /// Handler for Template module requests
    /// </summary>
    public class TemplateHandler : IRequestHandler<TemplateRequest, string>
    {
        public Task<string> Handle(TemplateRequest request, CancellationToken cancellationToken) => 
            Task.FromResult("Template Handler Invoked!");
    }
    
    /// <summary>
    /// Presenter for Template module that handles business logic and coordinates between Model and View
    /// 
    /// IMPORTANT: This is a template file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from TemplatePresenter to YourModuleNamePresenter
    /// 2. Update namespace to match your module location
    /// 3. Add your specific business logic and commands
    /// 4. Customize module navigation logic
    /// 5. Implement your specific UI event handling
    /// 6. Add any additional services or systems your module needs
    /// 
    /// NOTE: Navigation to MainMenuModule is already implemented via exit button
    /// </summary>
    public class TemplatePresenter : IDisposable
    {
        private readonly TemplateModuleModel _templateModuleModel;
        private readonly TemplateView _templateView;
        private readonly AudioSystem _audioSystem;
        private readonly IPopupHub _popupHub;
        
        private readonly CompositeDisposable _disposables = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        private readonly ReactiveCommand<Unit> _openMainMenuCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();

        public TemplatePresenter(
            TemplateModuleModel templateModuleModel,
            TemplateView templateView,
            AudioSystem audioSystem,
            IPopupHub popupHub)
        {
            _templateModuleModel = templateModuleModel ?? throw new ArgumentNullException(nameof(templateModuleModel));
            _templateView = templateView ?? throw new ArgumentNullException(nameof(templateView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _templateView.HideInstantly();

            var commands = new TemplateCommands(
                _openMainMenuCommand,
                _settingsPopupCommand,
                _toggleSoundCommand
            );

            _templateView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

            _templateView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _templateView.Show();
            
            _audioSystem.PlayMainMenuMelody();
        }

        public async UniTask Exit()
        {
            await _templateView.Hide();
        }
        
        public void HideInstantly() => _templateView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToUIUpdates()
        {
            _openMainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_templateModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_templateModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupButtonClicked())
                .AddTo(_disposables);

            _toggleSoundCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_templateModuleModel.CommandThrottleDelay))
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
