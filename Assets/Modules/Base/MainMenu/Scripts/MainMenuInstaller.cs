using CodeBase.Core;
using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private MainMenuView mainMenuView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            builder.AddMediatR(typeof(MainMenuHandler).Assembly);
            
            builder.Register<MainMenuModuleController>(Lifetime.Singleton);
            
            builder.Register<MainMenuModuleModel>(Lifetime.Singleton);
            builder.Register<MainMenuPresenter>(Lifetime.Singleton);
            builder.RegisterComponent(mainMenuView).As<MainMenuView>();
        }
    }
}