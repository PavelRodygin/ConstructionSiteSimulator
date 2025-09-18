using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;
using VContainer;

namespace Modules.Base.Bootstrap.Scripts
{
    public class BootstrapModuleController : IModuleController
    {
        [Inject] private readonly IScreenStateMachine _screenStateMachine;
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly BootstrapModuleModel _bootstrapModuleModel;
        private readonly BootstrapPresenter _bootstrapPresenter;
        
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        
        public BootstrapModuleController(IScreenStateMachine screenStateMachine, 
            BootstrapModuleModel bootstrapModuleModel, 
            BootstrapPresenter bootstrapPresenter)
        {
            _screenStateMachine = screenStateMachine;
            _bootstrapModuleModel = bootstrapModuleModel;
            _bootstrapPresenter = bootstrapPresenter;
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();
            
            _bootstrapPresenter.HideInstantly();
            await _bootstrapPresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _bootstrapPresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _bootstrapPresenter.Dispose();
            _bootstrapModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_bootstrapModuleModel.ModuleTransitionThrottleDelay))
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
