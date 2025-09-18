using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.Bootstrap.Scripts
{
    public class BootstrapInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private BootstrapView bootstrapView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            // Register main module controller
            builder.Register<BootstrapModuleController>(Lifetime.Singleton);

            // Register MVP components
            builder.Register<BootstrapModuleModel>(Lifetime.Singleton);
            builder.Register<BootstrapPresenter>(Lifetime.Singleton);
            builder.RegisterComponent(bootstrapView).As<BootstrapView>();
        }
    }
}
