using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeBase.Editor.ModuleCreator.Tasks;
using CodeBase.Editor.ModuleCreator.Tasks.AddPrefabTask;
using CodeBase.Editor.ModuleCreator.Tasks.AddScriptsTask;
using CodeBase.Editor.ModuleCreator.Tasks.CreateSceneTask;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator
{
    public class ModuleCreatorWindow : EditorWindow
    {
        private const float Spacing = 10f;
        private FolderType _selectedFolder = FolderType.Base;
        private bool _createPrefab = true;
        private bool _createScene = true;
        private string _moduleName = "NewModule";
        private bool _createInstaller = true;
        private bool _createPresenter = true;
        private bool _createView = true;
        private bool _createModel = true;
        private bool _createAsmdef = true;

        public enum FolderType { Additional, Base, Test }

        private static readonly string TrackingFilePath = "Assets/CodeBase/Editor/ModuleCreator/CreatedModules.json";
        private List<string> _createdModules = new List<string>();

        [MenuItem("Tools/Create Module")]
        public static void ShowWindow() => GetWindow<ModuleCreatorWindow>("Create Module");

        private static bool IsValidModuleName(string moduleName) =>
            !string.IsNullOrWhiteSpace(moduleName) && !moduleName.Contains(" ");

        private void OnEnable() => LoadCreatedModules();

        private void OnGUI()
        {
            GUILayout.Label("Module Creator", EditorStyles.boldLabel);
            _moduleName = EditorGUILayout.TextField("Module Name", _moduleName);
            GUILayout.Space(Spacing);
            _selectedFolder = (FolderType)EditorGUILayout.EnumPopup("Select Folder", _selectedFolder);
            GUILayout.Space(Spacing);
            GUILayout.Label("Select Scripts to Create", EditorStyles.boldLabel);
            _createInstaller = EditorGUILayout.Toggle("Installer", _createInstaller);
            _createPresenter = EditorGUILayout.Toggle("Presenter", _createPresenter);
            _createView = EditorGUILayout.Toggle("View", _createView);
            _createModel = EditorGUILayout.Toggle("Model", _createModel);
            GUILayout.Space(Spacing);
            _createAsmdef = EditorGUILayout.Toggle("Create asmdef", _createAsmdef);
            GUILayout.Space(Spacing);
            _createPrefab = EditorGUILayout.Toggle("Create Prefab", _createPrefab);
            _createScene = EditorGUILayout.Toggle("Create Scene", _createScene);
            GUILayout.Space(Spacing);
            if (GUILayout.Button("Create Module"))
            {
                if (IsValidModuleName(_moduleName))
                    CreateModule();
                else
                    EditorUtility.DisplayDialog("Invalid Name",
                        "Module name cannot be empty or contain spaces.", "OK");
            }
            GUILayout.Space(Spacing);
            if (HasLastModule())
            {
                if (GUILayout.Button("Delete Last Module"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Deletion",
                        "Are you sure you want to delete the last created module?", "Yes", "No"))
                        DeleteLastModule();
                }
            }
        }

        public IEnumerator CreateModule() => UniTask.ToCoroutine(async () =>
        {
            TaskQueue.Init();
            Debug.Log("Creating module with name: " + _moduleName);
            var addScriptsTask = new AddScriptsTask(
                _moduleName,
                _selectedFolder.ToString(),
                _createInstaller,
                _createPresenter,
                _createView,
                _createModel,
                _createAsmdef);
            TaskQueue.EnqueueTask(addScriptsTask);
            string targetModuleFolderPath = ModuleGenerator.GetTargetModuleFolderPath(_moduleName,
                _selectedFolder.ToString());
            if (_createPrefab)
            {
                var addPrefabTask = new AddPrefabTask(_moduleName, targetModuleFolderPath);
                TaskQueue.EnqueueTask(addPrefabTask);
            }
        
            if (_createScene)
            {
                var createSceneTask = new CreateSceneTask(_moduleName, targetModuleFolderPath);
                TaskQueue.EnqueueTask(createSceneTask);
            }
            
            _createdModules.Add(targetModuleFolderPath);
            SaveCreatedModules();
        
            await TaskQueue.UniTaskCompletionSource.Task;
        });

        private bool HasLastModule() =>
            _createdModules.Count > 0 && Directory.Exists(_createdModules[^1]);

        private string GetLastModulePath() =>
            _createdModules.Count > 0 ? _createdModules[^1] : null;

        public void DeleteLastModule()
        {
            string lastModulePath = GetLastModulePath();
            if (string.IsNullOrEmpty(lastModulePath))
            {
                Debug.LogError("No modules found to delete.");
                return;
            }

            string moduleName = Path.GetFileName(lastModulePath);
            Debug.Log("Starting delete module with name: " + moduleName);

            bool deleteResult = AssetDatabase.DeleteAsset(lastModulePath);
            AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                if (deleteResult)
                {
                    Debug.Log($"Module '{moduleName}' at '{lastModulePath}' deleted successfully.");
                    _createdModules.RemoveAt(_createdModules.Count - 1);
                    SaveCreatedModules();
                }
                else
                    Debug.LogError($"Failed to delete module '{moduleName}' at '{lastModulePath}'.");
            };
        }

        private void LoadCreatedModules()
        {
            if (File.Exists(TrackingFilePath))
            {
                string json = File.ReadAllText(TrackingFilePath);
                _createdModules = JsonConvert.
                    DeserializeObject<List<string>>(json) ?? new List<string>();
            }
        }

        private void SaveCreatedModules()
        {
            string directoryPath = Path.GetDirectoryName(TrackingFilePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            string json = JsonConvert.SerializeObject(_createdModules, Formatting.Indented);
            File.WriteAllText(TrackingFilePath, json);
        }
    }
}
