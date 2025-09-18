using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Services.SceneInstallerService
{
    public class SceneInstallerService
    {
        [Inject] private SceneService _sceneService;
        private List<ISceneInstaller> _currentScenesInstallers;
        
        private List<ISceneInstaller> FindAllSceneInstallers()
        {
            int sceneCount = SceneManager.sceneCount;
            Scene[] activeScenes = new Scene[sceneCount];

            for (int i = 0; i < sceneCount; i++) 
                activeScenes[i] = SceneManager.GetSceneAt(i);

            return FindSceneInstallersInScenes(activeScenes);
        }

        private List<ISceneInstaller> FindActiveModulesSceneInstallers()
        {
            Scene[] activeScenes = _sceneService.GetActiveModulesScenes();
            if (activeScenes.Length == 0)
                return FindAllSceneInstallers();
            return FindSceneInstallersInScenes(activeScenes);
        }

        private List<ISceneInstaller> FindSceneInstallersInScenes(Scene[] scenes)
        {
            List<ISceneInstaller> sceneInstallers = new List<ISceneInstaller>();
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].isLoaded)
                {
                    // Infrastructure GameObjects are the top-level objects in the scene hierarchy (direct children of the scene itself).
                    GameObject[] rootObjects = scenes[i].GetRootGameObjects();
                    foreach (GameObject rootObject in rootObjects)
                    {
                        ISceneInstaller[] installersInRoot = rootObject.
                            GetComponentsInChildren<ISceneInstaller>(true);
                        sceneInstallers.AddRange(installersInRoot);
                    }
                }
            }

            return sceneInstallers;
        }
        
        public LifetimeScope CombineScenes(LifetimeScope parentScope, bool removeObjectsToDelete)
        {
            _currentScenesInstallers = FindActiveModulesSceneInstallers();

            if (removeObjectsToDelete)
            {
                foreach (var installer in _currentScenesInstallers) 
                    installer.RemoveObjectsToDelete();
            }
            
            var sceneScope = parentScope.CreateChild(builder =>
            {
                foreach (var installer in _currentScenesInstallers)
                    installer.RegisterSceneDependencies(builder);
            });
            
            foreach (var installer in _currentScenesInstallers) 
                installer.InjectSceneViews(sceneScope.Container);

            return sceneScope;
        }
    }
}