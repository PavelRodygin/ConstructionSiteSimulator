using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Modules.Base.MainMenu.Scripts
{
    public readonly struct MainMenuCommands
    {
        public readonly ReactiveCommand<Unit> OpenConstructionSiteCommand;
        public readonly ReactiveCommand<Unit> SettingsPopupCommand;
        public readonly ReactiveCommand<Unit> SecondPopupCommand;
        public readonly ReactiveCommand<bool> SoundToggleCommand;

        public MainMenuCommands(
            ReactiveCommand<Unit> openConstructionSiteCommand,
            ReactiveCommand<Unit> settingsPopupCommand,
            ReactiveCommand<Unit> secondPopupCommand,
            ReactiveCommand<bool> soundToggleCommand)
        {
            OpenConstructionSiteCommand = openConstructionSiteCommand;
            SettingsPopupCommand = settingsPopupCommand;
            SecondPopupCommand = secondPopupCommand;
            SoundToggleCommand = soundToggleCommand;
        }
    }
    
    public class MainMenuView : BaseView
    {
        [Header("Open Module Buttons")]
        [SerializeField] private Button constructionSiteButton;
        
        [SerializeField] private Button settingsPopupButton;
        [SerializeField] private Button secondPopupButton;
        [SerializeField] private Toggle musicToggle;

        private InputSystemService _inputSystemService;
        
        [Inject]
        public void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;   
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
            ValidateUIElements();
            #endif
        }

        public void SetupEventListeners(MainMenuCommands commands)
        {
            _inputSystemService.SwitchToUI();
            
            constructionSiteButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.OpenConstructionSiteCommand.Execute(default))
                .AddTo(this);

            settingsPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SettingsPopupCommand.Execute(default))
                .AddTo(this);

            secondPopupButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SecondPopupCommand.Execute(default))
                .AddTo(this);

            musicToggle.OnValueChangedAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.SoundToggleCommand.Execute(musicToggle.isOn))
                .AddTo(this);
        }

        public override async UniTask Show()
        {
            await base.Show();
            
            _inputSystemService.SwitchToUI();
            OnScreenEnabled();
        }

        public void InitializeSoundToggle(bool isMusicOn) => musicToggle.SetIsOnWithoutNotify(isMusicOn);

        public void OnScreenEnabled()
        {
            _inputSystemService.SetFirstSelectedObject(constructionSiteButton);
        }

        private void ValidateUIElements()
        {
            if (settingsPopupButton == null) Debug.LogError($"{nameof(settingsPopupButton)} is not assigned in {nameof(MainMenuView)}");
            if (secondPopupButton == null) Debug.LogError($"{nameof(secondPopupButton)} is not assigned in {nameof(MainMenuView)}");
            if (constructionSiteButton == null) Debug.LogError($"{nameof(constructionSiteButton)} is not assigned in {nameof(MainMenuView)}");
            if (musicToggle == null) Debug.LogError($"{nameof(musicToggle)} is not assigned in {nameof(MainMenuView)}");
        }
    }
}