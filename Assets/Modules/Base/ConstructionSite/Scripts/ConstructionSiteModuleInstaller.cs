using CodeBase.Services.SceneInstallerService;
using Modules.Base.ConstructionSite.Scripts.Gameplay.Crane;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.ConstructionSite.Scripts
{
    /// <summary>
    /// Installer for ConstructionSite module that registers all dependencies
    /// </summary>
    public class ConstructionSiteModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private ConstructionSiteView constructionSiteView;
        [SerializeField] private Crane crane;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            // Register main module controller
            builder.Register<ConstructionSiteModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<ConstructionSiteModuleModel>(Lifetime.Singleton);
            builder.Register<ConstructionSitePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(constructionSiteView).As<ConstructionSiteView>();
            
            //Register Gameplay Components
            builder.RegisterComponent(crane).As<Crane>();
        }

        public override void InjectSceneViews(IObjectResolver resolver)
        {
            base.InjectSceneViews(resolver);
            
            // Manually inject dependencies into scene objects
            resolver.Inject(crane);
        }
    }
}