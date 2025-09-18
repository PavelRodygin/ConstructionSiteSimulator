using CodeBase.Core.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Services.SceneInstallerService
{
    public class BaseModuleSceneInstaller : SceneInstaller
    {
        [SerializeField] private BaseModuleCanvas moduleCanvas;
        [SerializeField] private Camera mainCamera;
        
        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            builder.RegisterComponent(moduleCanvas);
            builder.RegisterInstance(mainCamera);
        }
    }
}