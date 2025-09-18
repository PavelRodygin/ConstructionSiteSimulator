using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Services
{
    public enum AdditiveScenesMap
    {
        PopupsManager,
        DynamicBackground,
        SplashScreen,
    }

    public enum TestScenesMap  //TODO 
    {
        PopupTester
    }
    
    public class SceneService
    {
        private readonly List<string> _staticModuleScenes = new();
        private List<string> _activeModuleScenes = new();
        private List<string> _loadedModuleScenes = new();
        private CancellationTokenSource _cts;

        
        public SceneService() // loading of scenes that have to exist during the whole project and adding the additive static scene in the constructor
        {
            AddStaticAdditiveScene(AdditiveScenesMap.PopupsManager);
            LoadStaticScenes().Forget();
        }

        public Scene[] GetActiveModulesScenes()
        {
            //concat combines the two collections into a single sequence, preserving their order.
            return _activeModuleScenes.Concat(_staticModuleScenes) 
                .Select(SceneManager.GetSceneByName)
                .ToArray();
        }
        
        public async UniTask LoadStaticScenes() => await LoadScenesAsync(_staticModuleScenes);

        public void AddStaticAdditiveScene(AdditiveScenesMap sceneName) =>
            _staticModuleScenes.Add(sceneName.ToString());

        public void AddModuleActiveScene(string sceneName) => _activeModuleScenes.Add(sceneName);

        public async UniTask LoadScenesForModule(ModulesMap modulesMap)
        {
            List<string> scenes = new List<string> { modulesMap.ToString() };
            IEnumerable<AdditiveScenesMap> additionalScenes = GetAdditionalScenes(modulesMap);
            if (additionalScenes != null)
            {
                var sceneNames = additionalScenes.Select(scene => scene.ToString());
                scenes.InsertRange(0, sceneNames);
            }

            Debug.Log("Loading scenes: " + string.Join(", ", scenes));
            await LoadScenesAsync(scenes); //loading of all the needed scenes
            _activeModuleScenes = scenes;
        }

        private static IEnumerable<AdditiveScenesMap> GetAdditionalScenes(ModulesMap modulesMap)
        {
            return modulesMap switch
            {
                ModulesMap.Bootstrap => new List<AdditiveScenesMap>(),
                ModulesMap.Converter => new List<AdditiveScenesMap> {AdditiveScenesMap.DynamicBackground},
                ModulesMap.MainMenu => new List<AdditiveScenesMap>(),
                ModulesMap.TicTac => new List<AdditiveScenesMap>(),
                ModulesMap.DeliveryTycoon => new List<AdditiveScenesMap>() { AdditiveScenesMap.SplashScreen},
                _ => null
            };
            
            //ScreenPresenterMap.StartGame: Returns an empty list (no additional scenes).
            //ScreenPresenterMap.Converter: Returns a list containing one item: AdditiveScenesMap.DynamicBackground.
            //Other cases (MainMenu, TicTac): Return empty lists as well.
            //default case returns null if no matching case is found.
        }

        //checks if the current a scene already exists
        private bool IsCurrentScene(string sceneName) =>
            SceneManager.GetActiveScene().name == sceneName; 

        private async UniTask LoadScenesAsync(List<string> scenes)
        {
            List<UniTask> loadTasks = new List<UniTask>();

            foreach (var scene in scenes)
            {
                if (!IsCurrentScene(scene))
                    loadTasks.Add(LoadSceneAsync(scene, true)); //if the scene is not a current scene we add it
                                                                      //to the task list and load it as an additive one
                else
                {
                    if (!_loadedModuleScenes.Contains(scene)) //if the scene is not in the list, we add it, but we don't load it
                        _loadedModuleScenes.Add(scene);
                }
            }

            await UniTask.WhenAll(loadTasks); //wait until all the additive scenes have been loaded
        }

        private async UniTask LoadSceneAsync(string sceneName, bool additive)
        {
            //takes SceneManager.LoadSceneAsync as a delegate and in the method we enter a name of a scene and check if it's additive or not
            // Single mode loads a standard Unity Scene which then appears on its own in the Hierarchy window.
            // Additive loads a Scene which appears in the Hierarchy window while another is active (Adds the Scene to the current loaded Scenes).
            await LoadSceneAsyncInternal(() =>
                SceneManager.LoadSceneAsync(sceneName,
                    additive ? LoadSceneMode.Additive : LoadSceneMode.Single));

            if (!_loadedModuleScenes.Contains(sceneName))
                _loadedModuleScenes.Add(sceneName);
        }

        //func - Encapsulates a method that has no parameters and returns a value of the type specified by the
        //TResult parameter. It's essentially a delegate
        
        //allowSceneActivation Allow Scenes to be activated as soon as it is ready. If a
        //LoadSceneAsync.allowSceneActivation is set to false, and another AsyncOperation
        //(e.g. SceneManager.UnloadSceneAsync ) initializes, Unity does not call the second operation until the
        //first AsyncOperation.allowSceneActivation is set to true.
        private async UniTask LoadSceneAsyncInternal(Func<AsyncOperation> loadSceneFunc)  
        {
            try
            {
                var asyncOperation = loadSceneFunc();
                asyncOperation.allowSceneActivation = false; 

                while (asyncOperation.progress < 0.9f)  // Check if the load has finished
                    await UniTask.Yield();

                asyncOperation.allowSceneActivation = true;
                await asyncOperation; //awaits loading of a scene
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading scene: {ex.Message}");
            }
        }

        public async UniTask UnloadUnusedScenesAsync()
        {
            if (_activeModuleScenes == null || _activeModuleScenes.Count == 0)
            {
                Debug.LogWarning("No scenes to load.");
                return;
            }

            var scenesToUnload = _loadedModuleScenes
                .Except(_activeModuleScenes) // Исключение активных сцен (exclusion of the active scenes)
                .Except(_staticModuleScenes) // Исключение постоянных сцен (exclusion of the static scenes that we need for the whole project)
                .ToList();

            List<UniTask> unloadTasks = new List<UniTask>();

            foreach (var scene in scenesToUnload)
            {
                var sceneToUnload = SceneManager.GetSceneByName(scene);
                if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
                    unloadTasks.Add(SceneManager.UnloadSceneAsync(scene).ToUniTask());
                else
                    Debug.LogWarning($"Scene {scene} is not valid or not loaded.");
            }

            await UniTask.WhenAll(unloadTasks);

            _loadedModuleScenes = _loadedModuleScenes.Except(scenesToUnload).ToList();
        }
    }
}
