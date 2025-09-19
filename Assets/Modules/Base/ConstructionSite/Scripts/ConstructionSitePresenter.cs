using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using R3;
using Unit = R3.Unit;

namespace Modules.Base.ConstructionSite.Scripts
{
    /// <summary>
    /// Presenter for ConstructionSite module that handles business logic and coordinates between Model and View
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

            var commands = new ConstructionSiteCommands(_openMainMenuCommand);

            _constructionSiteView.SetupEventListeners(commands);
            SubscribeToUIUpdates();

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

        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand.Execute(ModulesMap.MainMenu);
        }

    }
}
