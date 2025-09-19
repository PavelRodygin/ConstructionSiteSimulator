using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.ConstructionSite.Scripts
{
    /// <summary>
    /// Main controller for ConstructionSite module that manages the module lifecycle
    /// and coordinates between Presenter, Model and View
    /// </summary>
    public class ConstructionSiteModuleController : IModuleController
    {
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly InputSystemService _inputSystemService;
        private readonly ConstructionSiteModuleModel _constructionSiteModuleModel;
        private readonly ConstructionSitePresenter _constructionSitePresenter;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly UniTaskCompletionSource _moduleCompletionSource;

        private readonly CompositeDisposable _disposables = new();
        
        public ConstructionSiteModuleController(IScreenStateMachine screenStateMachine, ConstructionSiteModuleModel constructionSiteModuleModel, 
            ConstructionSitePresenter constructionSitePresenter, InputSystemService inputSystemService)
        {
            _constructionSiteModuleModel = constructionSiteModuleModel ?? throw new ArgumentNullException(nameof(constructionSiteModuleModel));
            _constructionSitePresenter = constructionSitePresenter ?? throw new ArgumentNullException(nameof(constructionSitePresenter));
            _inputSystemService = inputSystemService;
            _screenStateMachine = screenStateMachine ?? throw new ArgumentNullException(nameof(screenStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _constructionSitePresenter.HideInstantly();
            
            _inputSystemService.SwitchToCrane();
            await _constructionSitePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _constructionSitePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _constructionSitePresenter.Dispose();
            
            _constructionSiteModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_constructionSiteModuleModel.ModuleTransitionThrottleDelay))
                .Subscribe(RunNewModule)
                .AddTo(_disposables);
        }

        private void RunNewModule(ModulesMap screen)
        {
            _moduleCompletionSource.TrySetResult();
            _screenStateMachine.RunModule(screen);
        }
    }
}
