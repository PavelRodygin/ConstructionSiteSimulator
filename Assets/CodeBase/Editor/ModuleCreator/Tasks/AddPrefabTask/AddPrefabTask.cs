using System;
using CodeBase.Editor.ModuleCreator.Base;
using CodeBase.Editor.ModuleCreator.Base.ConfigManagement;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Tasks.AddPrefabTask
{
    [Serializable]
    public class AddPrefabTask : Task
    {
        [JsonProperty] private string _moduleName;
        [JsonProperty] private string _targetModuleFolderPath;

        public AddPrefabTask(string moduleName, string targetModuleFolderPath)
        {
            _moduleName = moduleName;
            _targetModuleFolderPath = targetModuleFolderPath;
            WaitForCompilation = true; 
        }

        public override void Execute()
        {
            string targetPrefabPath = PrefabHelper.CopyTemplatePrefab(_moduleName, _targetModuleFolderPath);
            if (string.IsNullOrEmpty(targetPrefabPath))
                return;

            GameObject prefabContents = PrefabHelper.LoadPrefab(targetPrefabPath);
            if (prefabContents == null)
                return;

            string folderType = PathManager.GetFolderType(_targetModuleFolderPath);
            if (string.IsNullOrEmpty(folderType))
                return;

            PrefabHelper.ReplaceScriptProperty(prefabContents, _moduleName, folderType);

            MonoScript newMonoScript = PrefabHelper.
                FindMonoScript($"Modules.{folderType}.{_moduleName}Module.{ModulePathCache.ScriptsFolderName}.{_moduleName}View");
            if (newMonoScript == null)
            {
                Debug.LogError($"Failed to find MonoScript for '{_moduleName}View' in namespace 'Modules.{folderType}.{_moduleName}Module.{ModulePathCache.ScriptsFolderName}'");
                Debug.LogError("This usually means the script compilation failed or the namespace/class name is incorrect.");
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }
            
            Debug.Log($"Successfully found MonoScript for {_moduleName}View: {newMonoScript.name}");

            Type newViewType = PrefabHelper.GetViewType(newMonoScript);
            if (newViewType == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return;
            }

            Component newViewComponent = PrefabHelper.GetViewComponent(prefabContents, newViewType);
            if (newViewComponent != null)
            {
                PrefabHelper.AssignTemplateScreenTitle(prefabContents, _moduleName, newViewComponent, newViewType);
                PrefabHelper.InvokeSetTitle(newViewComponent, _moduleName, newViewType);
                PrefabHelper.LogTitleSet(_moduleName, newViewType.Name);
            }
            else
                PrefabHelper.LogComponentNotFound(newViewType.Name);

            PrefabHelper.SaveAndUnloadPrefab(prefabContents, targetPrefabPath, _moduleName);
            PrefabHelper.LogPrefabCreated(_moduleName);
        }
    }
}
