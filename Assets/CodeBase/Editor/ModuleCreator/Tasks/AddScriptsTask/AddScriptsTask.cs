using System;
using CodeBase.Editor.ModuleCreator.Base;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Tasks.AddScriptsTask
{
    [Serializable]
    public class AddScriptsTask : Task
    {
        [JsonProperty] private string _moduleName;
        [JsonProperty] private string _selectedFolder;
        [JsonProperty] private bool _isInstallerRequired;
        [JsonProperty] private bool _isPresenterRequired;
        [JsonProperty] private bool _isViewRequired;
        [JsonProperty] private bool _isModelRequired;
        [JsonProperty] private bool _isAsmdefRequired;

        public AddScriptsTask(string moduleName, string selectedFolder, bool isInstallerRequired,
            bool isPresenterRequired, bool isViewRequired, bool isModelRequired, bool isAsmdefRequired)
        {
            _moduleName = moduleName;
            _selectedFolder = selectedFolder;
            _isInstallerRequired = isInstallerRequired;
            _isPresenterRequired = isPresenterRequired;
            _isViewRequired = isViewRequired;
            _isModelRequired = isModelRequired;
            _isAsmdefRequired = isAsmdefRequired;
            WaitForCompilation = true;
        }

        public override void Execute()
        {
            PathManager.InitializePaths();

            if (TemplateValidator.AreTemplatesAvailable(_isAsmdefRequired))
            {
                ModuleGenerator.CreateModuleFiles(
                    _moduleName,
                    _selectedFolder,
                    _isInstallerRequired,
                    _isPresenterRequired,
                    _isViewRequired,
                    _isModelRequired,
                    _isAsmdefRequired);

                AssetDatabase.Refresh();
                Debug.Log($"Module {_moduleName} scripts created successfully.");
            }
            else
                Debug.LogError("Templates are not available.");
        }
    }
}