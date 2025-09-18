using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace CodeBase.Services.SceneInstallerService
{
    public abstract class SceneInstaller : MonoBehaviour, ISceneInstaller
    {
        [SerializeField] private List<GameObject> objectsToDelete;
        
        public abstract void RegisterSceneDependencies(IContainerBuilder builder);

        public void RemoveObjectsToDelete()
        {
            foreach (var objectToDelete in objectsToDelete) 
                Destroy(objectToDelete);
        }

        public virtual void InjectSceneViews(IObjectResolver resolver) { }
    }
}