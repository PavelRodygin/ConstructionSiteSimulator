using CodeBase.Core.UI.Views;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Unit = R3.Unit;

namespace Modules.Base.ConstructionSite.Scripts
{
    public readonly struct ConstructionSiteCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;

        public ConstructionSiteCommands(ReactiveCommand<Unit> openMainMenuCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
        }
    }
    
    public class ConstructionSiteView : BaseView
    {
        [Header("Navigation")]
        [SerializeField] private Button mainMenuButton;
        
        [Header("Sound")]
        [SerializeField] private Toggle soundToggle;

        private ConstructionSiteCommands _commands;

        public void SetupEventListeners(ConstructionSiteCommands commands)
        {
            _commands = commands;
            
            if (mainMenuButton != null)
            {
                mainMenuButton.OnClickAsObservable()
                    .Subscribe(_ => _commands.OpenMainMenuCommand.Execute(Unit.Default))
                    .AddTo(this);
            }
            
            // if (settingsButton != null)
            // {
            //     settingsButton.OnClickAsObservable()
            //         .Subscribe(_ => _commands.SettingsPopupCommand.Execute(Unit.Default))
            //         .AddTo(this);
            // }
            //
            // if (soundToggle != null)
            // {
            //     soundToggle.OnValueChangedAsObservable()
            //         .Subscribe(isOn => _commands.ToggleSoundCommand.Execute(isOn))
            //         .AddTo(this);
            // }
        }
    }
}
