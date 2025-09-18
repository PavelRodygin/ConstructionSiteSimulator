using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using MediatR;
using R3;
using VContainer;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuModuleController : IModuleController
    {
        [Inject] private IMediator _mediator;
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly MainMenuModuleModel _mainMenuModuleModel;
        private readonly MainMenuPresenter _mainMenuPresenter;
        private readonly IScreenStateMachine _screenStateMachine;
        
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public MainMenuModuleController(IScreenStateMachine screenStateMachine, MainMenuModuleModel mainMenuModuleModel, 
            MainMenuPresenter mainMenuPresenter)
        {
            _mainMenuModuleModel = mainMenuModuleModel;
            _mainMenuPresenter = mainMenuPresenter;
            _screenStateMachine = screenStateMachine;
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _mainMenuPresenter.HideInstantly();
            
            await _mainMenuPresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _mainMenuPresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _mainMenuPresenter.Dispose();
            _mainMenuModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.ModuleTransitionThrottleDelay))
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