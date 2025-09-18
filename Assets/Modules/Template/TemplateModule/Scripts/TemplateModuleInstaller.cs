using CodeBase.Core;
using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Template.TemplateModule.Scripts
{
    /// <summary>
    /// Installer for Template module that registers all dependencies
    /// 
    /// IMPORTANT: This is a template file for ModuleCreator system.
    /// When creating a new module, this file will be copied and modified.
    /// 
    /// Key points for customization:
    /// 1. Change class name from TemplateModuleInstaller to YourModuleNameInstaller
    /// 2. Update namespace to match your module location
    /// 3. Register your specific dependencies
    /// 4. Update the View component reference
    /// 5. Add any additional services or systems your module needs
    /// </summary>
    public class TemplateModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private TemplateView templateView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            builder.AddMediatR(typeof(TemplateHandler).Assembly);
            
            // Register main module controller
            builder.Register<TemplateModuleController>(Lifetime.Singleton);
            
            // Register MVP components
            builder.Register<TemplateModuleModel>(Lifetime.Singleton);
            builder.Register<TemplatePresenter>(Lifetime.Singleton);
            builder.RegisterComponent(templateView).As<TemplateView>();
        }
    }
}