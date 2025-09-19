using CodeBase.Services.SceneInstallerService;
using Modules.Base.ConstructionSite.Scripts.Gameplay.Crane;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.ConstructionSite.Scripts
{
    /// <summary>
    /// Installer for ThirdPersonMP module that registers all dependencies
    /// 
    /// IMPORTANT: This is a thirdPersonMP file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from ThirdPersonMPModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace Modules.Base.ThirdPersonMPModule.Scripts match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
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
    }
}