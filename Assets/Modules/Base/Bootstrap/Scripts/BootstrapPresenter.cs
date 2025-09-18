using System;
using System.Threading;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Unit = R3.Unit;

namespace Modules.Base.Bootstrap.Scripts
{
    public class BootstrapPresenter : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly BootstrapModuleModel _bootstrapModuleModel;
        private readonly BootstrapView _bootstrapView;
        
        private readonly ReactiveProperty<string> _progressStatus = new(string.Empty);
        private readonly ReactiveProperty<float> _exponentialProgress = new(0f);
        private readonly ReactiveCommand<Unit> _startCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        
        public ReadOnlyReactiveProperty<string> ProgressStatus => 
            _progressStatus.ToReadOnlyReactiveProperty();
        public ReadOnlyReactiveProperty<float> ExponentialProgress => 
            _exponentialProgress.ToReadOnlyReactiveProperty();
        public ReactiveCommand<Unit> StartCommand => _startCommand;
        
        public BootstrapPresenter(BootstrapModuleModel bootstrapModuleModel, BootstrapView bootstrapView)
        {
            _bootstrapModuleModel = bootstrapModuleModel;
            _bootstrapView = bootstrapView;

            SubscribeToUIUpdates();
        }
        
        private void SubscribeToUIUpdates()
        {
            _startCommand.Subscribe(_ => OnContinueButtonPressed())
                .AddTo(_cancellationTokenSource.Token);
        }
        
        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            SetVersionText(Application.version);
            SetApplicationFrameRate();
            InitializeUI();
    
            ShowTooltips().Forget();
            _bootstrapModuleModel.DoTweenInit();
            _bootstrapModuleModel.RegisterCommands();

            await _bootstrapView.Show();

            await InitializeServices();

            ShowAnimations();
        }
        
        private void InitializeUI()
        {
            _bootstrapView.HideInstantly();
            _bootstrapView.SetupEventListeners(StartCommand,
                ProgressStatus, ExponentialProgress); 
        }

        private async UniTask InitializeServices()
        {
            var timing = 1f / _bootstrapModuleModel.Commands.Count;
            var currentTiming = timing;
            
            foreach (var (serviceName, initFunction) in _bootstrapModuleModel.Commands)
            {
                _progressStatus.Value = $"Loading: {serviceName}";
                _exponentialProgress.Value = CalculateExponentialProgress(currentTiming);
                currentTiming += timing;
                
                await initFunction.Invoke().AsUniTask();
            }
        }

        private static float CalculateExponentialProgress(float progress)
        {
            var expValue = Math.Exp(progress);
            var minExp = Math.Exp(0);
            var maxExp = Math.Exp(1);
            return (float)((expValue - minExp) / (maxExp - minExp));
        }

        public async UniTask Exit()
        {
            _cancellationTokenSource?.Cancel();
            await _bootstrapView.Hide();
        }

        public void HideInstantly() => _bootstrapView.HideInstantly();

        private void SetApplicationFrameRate() => 
            Application.targetFrameRate = _bootstrapModuleModel.AppFrameRate;

        private void OnContinueButtonPressed() => _openNewModuleCommand.Execute(ModulesMap.MainMenu);

        private void SetVersionText(string appVersion) => _bootstrapView.SetVersionText(appVersion);

        private void ShowAnimations() => _bootstrapView.ShowAnimations(_cancellationTokenSource.Token);

        private async UniTaskVoid ShowTooltips()
        {
            var token = _cancellationTokenSource.Token;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var tooltip = _bootstrapModuleModel.GetNextTooltip();
                    _bootstrapView.SetTooltipText(tooltip);
                    await UniTask.Delay(_bootstrapModuleModel.TooltipDelay, cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Debug.LogError($"ShowTooltips Error: {ex.Message}"); }
        }

        public void Dispose()
        {
            if (_cancellationTokenSource is {IsCancellationRequested: false}) 
                _cancellationTokenSource.Cancel();
            _cancellationTokenSource?.Dispose();
            
            _disposables?.Dispose();
            _bootstrapView?.Dispose();
            _bootstrapModuleModel?.Dispose();
        }
    }
}
