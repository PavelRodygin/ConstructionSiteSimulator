using CodeBase.Core.Systems.PopupHub.Popups;
using CodeBase.Core.UI;
using CodeBase.Services.EventMediator;
using CodeBase.Services.SceneInstallerService;
using CodeBase.Systems.PopupHub;
using CodeBase.Systems.PopupHub.Popups.FirstPopup;
using CodeBase.Systems.PopupHub.Popups.SecondPopup;
using CodeBase.Systems.PopupHub.Popups.SettingsPopup;
using CodeBase.Systems.PopupHub.Popups.ThirdPopup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Additional.PopupsManager.Scripts
{
    public class PopupsManagerInstaller : SceneInstaller
    {
        [SerializeField] private BasePopupCanvas popupCanvas;
        [SerializeField] private FirstPopup firstPopupPrefab;
        [SerializeField] private SecondPopup secondPopup;
        [SerializeField] private ThirdPopup thirdPopup;
        [SerializeField] private SettingsPopup settingsPopupPrefab;
        [SerializeField] private ThirdPopup rebindingPopupPrefab;
        
        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            builder.RegisterComponent(popupCanvas)
                .As<BasePopupCanvas>();
            
            builder.Register<EventMediator>(Lifetime.Singleton);

            RegisterPopupFactories(builder);
            
            builder.Register<PopupHub>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
        
        private void RegisterPopupFactories(IContainerBuilder builder)
        {
            builder.Register<BasePopupFactory<FirstPopup>>(Lifetime.Transient)
                .WithParameter(firstPopupPrefab)
                .AsImplementedInterfaces();
            builder.Register<BasePopupFactory<SecondPopup>>(Lifetime.Transient)
                .WithParameter(secondPopup)
                .AsImplementedInterfaces();             
            builder.Register<BasePopupFactory<ThirdPopup>>(Lifetime.Transient)
                .WithParameter(thirdPopup)
                .AsImplementedInterfaces();
            builder.Register<BasePopupFactory<SettingsPopup>>(Lifetime.Transient)
                .WithParameter(settingsPopupPrefab)
                .AsImplementedInterfaces();
            // builder.Register<BasePopupFactory<RebindingPopup>>(Lifetime.Transient)
            //     .WithParameter(rebindingPopupPrefab)
            //     .AsImplementedInterfaces();
        }
    }
}