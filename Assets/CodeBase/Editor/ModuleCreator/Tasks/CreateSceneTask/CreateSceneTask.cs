using System;
using System.Linq;
using CodeBase.Core.UI;
using CodeBase.Editor.ModuleCreator.Base;
using CodeBase.Editor.ModuleCreator.Base.ConfigManagement;
using CodeBase.Editor.ModuleCreator.Tasks.AddScriptsTask;
using CodeBase.Implementation.UI;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CodeBase.Editor.ModuleCreator.Tasks.CreateSceneTask
{
    [Serializable]
    public class CreateSceneTask : Task
    {
        [JsonProperty] private string _moduleName;
        [JsonProperty] private string _targetModuleFolderPath;

        public CreateSceneTask(string moduleName, string targetModuleFolderPath)
        {
            _moduleName = moduleName;
            _targetModuleFolderPath = targetModuleFolderPath;
            WaitForCompilation = true;
        }

        public override void Execute()
        {
            string sceneFolderPath = PathManager.CombinePaths(_targetModuleFolderPath, ModulePathCache.ScenesFolderName);
            ModuleGenerator.EnsureTargetFolderExists(sceneFolderPath);
            string scenePath = PathManager.CombinePaths(sceneFolderPath, $"{_moduleName}.unity");
            CreateNewScene(scenePath);

            // Create template hierarchy structure that matches Template module scene layout
            // This ensures consistent organization across all modules
            GameObjectFactory.CreateTemplateHierarchy(_moduleName);
            
            // Get references to created objects at scene root level
            GameObject systemsObject = GameObject.Find("Systems");
            GameObject installerObject = systemsObject.transform.Find($"{_moduleName}ModuleInstaller").gameObject;
            GameObject canvasObject = GameObject.Find("ModuleCanvas");
            GameObject cameraObject = GameObject.Find("ModuleCamera");
            
            // Force asset database refresh after creating hierarchy to ensure it is indexed
            UnityEditor.AssetDatabase.Refresh();

            string viewPrefabPath = PathManager.CombinePaths(_targetModuleFolderPath, 
                ModulePathCache.ViewsFolderName, $"{_moduleName}ModuleView.prefab");
            GameObject viewInstance = GameObjectFactory.InstantiateViewPrefab(viewPrefabPath, canvasObject);
            if (viewInstance == null)
            {
                Debug.LogError("Failed to instantiate View prefab."); 
                return;
            }
            
            // Force asset database refresh after creating view instance to ensure it is indexed
            UnityEditor.AssetDatabase.Refresh();

            //TODO Ошибка появилась после замены "Scripts"
            //TODO Здесь проблема. Скрипт успевает создаваться, однако он не обнаруживается 55-ой строчкой.
            string installerName = $"{_moduleName}ModuleInstaller";
            string folderType = PathManager.GetFolderType(_targetModuleFolderPath);
            string installerFullName = 
                $"Modules.{folderType}.{_moduleName}Module.{ModulePathCache.ScriptsFolderName}.{installerName}";
            
            Debug.Log($"Looking for installer type: {installerFullName}");
            Type installerType = ReflectionHelper.FindType(installerFullName);
            if (installerType == null)
            {
                Debug.LogError($"Installer type '{installerName}' not found. Full name: {installerFullName}");
                Debug.LogError("This usually means the script compilation failed or the namespace/class name is incorrect.");
                return;
            }
            
            Debug.Log($"Successfully found installer type: {installerType.FullName}");

            // Add installer component to the existing installer object
            installerObject.AddComponent(installerType);
            
            // Force asset database refresh after creating installer to ensure it is indexed
            UnityEditor.AssetDatabase.Refresh();

            AssignInstallerFields(installerObject, viewInstance, canvasObject, cameraObject.GetComponent<Camera>());
            AssignScreensCanvasFields(canvasObject, cameraObject.GetComponent<Camera>());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Force asset database refresh after creating scene to ensure all assets are indexed
            UnityEditor.AssetDatabase.Refresh();
        }

        private void CreateNewScene(string scenePath)
        {
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (newScene.IsValid())
            {
                EditorSceneManager.SaveScene(newScene, scenePath);
                SceneManager.SetActiveScene(newScene);
                Debug.Log($"New scene created at: {scenePath}");
                
                // Force asset database refresh after creating scene to ensure it is indexed
                UnityEditor.AssetDatabase.Refresh();
            }
            else
                Debug.LogError("Failed to create a new scene.");
        }

        private void AssignInstallerFields(GameObject installerObject, GameObject viewInstance,
            GameObject canvas, Camera camera)
        {
            Component installerComponent = ReflectionHelper.
                GetComponentByName(installerObject, installerObject.name);
            if (installerComponent == null)
                return;

            string fieldPrefix = char.ToLower(_moduleName[0]) + _moduleName.Substring(1);
            string viewFieldName = $"{fieldPrefix}View";
            Component viewComponent = viewInstance.GetComponent($"{_moduleName}View");
            if (viewComponent != null)
            {
                ReflectionHelper.SetPrivateField(installerComponent, viewFieldName, viewComponent);
                Debug.Log($"Successfully assigned view component '{_moduleName}View' to installer field '{viewFieldName}'");
            }
            else
            {
                Debug.LogError($"View component '{_moduleName}View' not found on View prefab.");
                Debug.LogError($"Available components on view instance: {string.Join(", ", viewInstance.GetComponents<Component>().Select(c => c.GetType().Name))}");
            }

            var screenCanvas = canvas.GetComponent<BaseModuleCanvas>();
            if (screenCanvas != null)
                ReflectionHelper.SetPrivateField(installerComponent, "moduleCanvas", screenCanvas);
            else
                Debug.LogError("ModuleCanvas component not found on Canvas.");

            if (camera != null)
                ReflectionHelper.SetPrivateField(installerComponent, "mainCamera", camera);
            else
                Debug.LogError("Main Camera is null.");
        }

        private void AssignScreensCanvasFields(GameObject canvas, Camera camera)
        {
            var screenCanvas = canvas.GetComponent<ModuleCanvas>();
            if (screenCanvas == null)
            {
                Debug.LogError("ModuleCanvas component not found on Canvas.");
                return;
            }

            CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
                ReflectionHelper.SetPrivateField(screenCanvas, "canvasScaler", canvasScaler);
            else
                Debug.LogError("CanvasScaler component not found on Canvas.");

            if (camera != null)
                ReflectionHelper.SetPrivateField(screenCanvas, "uiCamera", camera);
            else
                Debug.LogError("UI Camera is null.");
        }
    }
}