using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using MediatR;
using R3;
using VContainer;

namespace Modules.Template.TemplateModule.Scripts
{
    /// <summary>
        /// Main controller for Template module that manages the module lifecycle
        /// and coordinates between Presenter, Model and View
        /// 
        /// IMPORTANT: This is a template file for ModuleCreator system.
        /// When creating a new module, this file will be copied and modified.
        /// 
        /// Key points for customization:
        /// 1. Change class name from TemplateModuleController to YourModuleNameModuleController
        /// 2. Update namespace to match your module location
        /// 3. Customize module lifecycle management if needed
        /// 4. Add specific initialization logic for your module
        /// 5. Implement custom exit conditions if required
    /// </summary>
    public class TemplateModuleController : IModuleController
    {
        [Inject] private IMediator _mediator;
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly TemplateModuleModel _templateModuleModel;
        private readonly TemplatePresenter _templatePresenter;
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        
        private readonly CompositeDisposable _disposables = new();
        
        public TemplateModuleController(IScreenStateMachine screenStateMachine, TemplateModuleModel templateModuleModel, 
            TemplatePresenter templatePresenter)
        {
            _templateModuleModel = templateModuleModel ?? throw new ArgumentNullException(nameof(templateModuleModel));
            _templatePresenter = templatePresenter ?? throw new ArgumentNullException(nameof(templatePresenter));
            _screenStateMachine = screenStateMachine ?? throw new ArgumentNullException(nameof(screenStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _templatePresenter.HideInstantly();
            
            await _templatePresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _templatePresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _templatePresenter.Dispose();
            
            _templateModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_templateModuleModel.ModuleTransitionThrottleDelay))
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
