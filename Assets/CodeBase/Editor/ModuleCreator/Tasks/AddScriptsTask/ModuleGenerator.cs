using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CodeBase.Editor.ModuleCreator.Base;
using CodeBase.Editor.ModuleCreator.Base.ConfigManagement;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Tasks.AddScriptsTask
{
    public static class ModuleGenerator
    {
        public static string TargetModuleFolderPath { get; private set; }

        public static void CreateModuleFiles(
            string moduleName,
            string selectedFolder,
            bool isInstallerRequired,
            bool isPresenterRequired,
            bool isViewRequired,
            bool isModelRequired,
            bool isAsmdefRequired)
        {
            string selectedFolderPath = GetSelectedFolderPath(selectedFolder);
            string targetFolderPath = PathManager.CombinePaths(selectedFolderPath, $"{moduleName}Module");
            TargetModuleFolderPath = targetFolderPath;
            EnsureModuleFolders(targetFolderPath);

            string scriptsFolderPath = PathManager.CombinePaths(targetFolderPath, ModulePathCache.ScriptsFolderName);

            if (isAsmdefRequired)
                CreateAsmdefFile(targetFolderPath, moduleName);

            CreateSelectedScripts(
                scriptsFolderPath,
                moduleName,
                selectedFolder,
                isInstallerRequired,
                isPresenterRequired,
                isViewRequired,
                isModelRequired);
        }

        public static string GetTargetModuleFolderPath(string moduleName, string selectedFolder)
        {
            string selectedFolderPath = GetSelectedFolderPath(selectedFolder);
            return PathManager.CombinePaths(selectedFolderPath, $"{moduleName}Module");
        }

        private static string GetSelectedFolderPath(string selectedFolder) =>
            selectedFolder switch
            {
                "Additional" => PathManager.AdditionalFolderPath,
                "Base" => PathManager.BaseFolderPath,
                "Test" => PathManager.TestFolderPath,
                _ => PathManager.BaseFolderPath
            };

        private static void EnsureModuleFolders(string targetFolderPath)
        {
            EnsureTargetFolderExists(targetFolderPath);
            EnsureTargetFolderExists(PathManager.CombinePaths(targetFolderPath, ModulePathCache.ScriptsFolderName));
            EnsureTargetFolderExists(PathManager.CombinePaths(targetFolderPath, ModulePathCache.ViewsFolderName));
        }

        public static void EnsureTargetFolderExists(string targetFolderPath)
        {
            targetFolderPath = targetFolderPath.Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(targetFolderPath))
            {
                string parentFolder = Path.GetDirectoryName(targetFolderPath)?.Replace("\\", "/");
                string newFolderName = Path.GetFileName(targetFolderPath);
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        private static void CreateAsmdefFile(string targetFolderPath, string moduleName)
        {
            string templateAsmdefPath = PathManager.
                CombinePaths(PathManager.TemplateModuleFolderPath, "Template.asmdef");
            string targetAsmdefPath = PathManager.
                CombinePaths(targetFolderPath, $"{moduleName}Module.asmdef");
            CopyAndAdjustAsmdef(templateAsmdefPath, targetAsmdefPath, moduleName);
            
            // Force asset database refresh after creating asmdef to ensure it is indexed
            UnityEditor.AssetDatabase.Refresh();
        }

        private static void CreateSelectedScripts(
            string folderPath,
            string moduleName,
            string selectedFolder,
            bool createInstaller,
            bool createPresenter,
            bool createView,
            bool createModel)
        {
            var scriptsToCreate = new List<(bool isRequired, string templateFile, string outputFile)>
            {
                (createInstaller, "TemplateModuleInstaller.cs", $"{moduleName}ModuleInstaller.cs"),
                (createPresenter, "TemplatePresenter.cs", $"{moduleName}Presenter.cs"),
                (createView, "TemplateView.cs", $"{moduleName}View.cs"),
                (createModel, "TemplateModuleModel.cs", $"{moduleName}ModuleModel.cs"),
                (true, "TemplateModuleController.cs", $"{moduleName}ModuleController.cs"),
            };

            foreach (var (shouldCreate, templateFile, outputFile) in scriptsToCreate)
            {
                if (shouldCreate)
                {
                    string content = GetTemplateContent(templateFile, moduleName, selectedFolder);
                    CreateScript(folderPath, outputFile, content);
                }
            }
            
            // Force asset database refresh after creating all scripts to ensure they are indexed
            UnityEditor.AssetDatabase.Refresh();
        }

        private static string GetTemplateContent(string templateFileName, string moduleName, string selectedFolder)
        {
            string templateFilePath = PathManager.CombinePaths(PathManager.TemplateScriptsFolderPath, templateFileName);
            string content = ReadTemplateFile(templateFilePath);
            if (content == null)
                return null;

            string moduleNameLower = char.ToLower(moduleName[0]) + moduleName.Substring(1);
            content = ReplaceNamespace(content, moduleName, selectedFolder);
            content = ReplaceTemplateOccurrences(content, moduleName, moduleNameLower);
            return content;
        }

        private static string ReadTemplateFile(string templateFilePath) =>
            File.Exists(templateFilePath) ? File.ReadAllText(templateFilePath) : null;

        private static string ReplaceNamespace(string content, string moduleName, string selectedFolder)
        {
            string namespaceReplacement = 
                $"namespace Modules.{selectedFolder}.{moduleName}Module.{ModulePathCache.ScriptsFolderName}";
            return Regex.Replace(content, @"namespace\s+[\w\.]+", namespaceReplacement);
        }

        private static string ReplaceTemplateOccurrences(string content, string moduleName, string moduleNameLower)
        {
            // Replace Template with ModuleName in class names and other identifiers
            content = Regex.Replace(content, @"TemplateModuleController", $"{moduleName}ModuleController");
            content = Regex.Replace(content, @"TemplateModuleInstaller", $"{moduleName}ModuleInstaller");
            content = Regex.Replace(content, @"TemplateModuleModel", $"{moduleName}ModuleModel");
            content = Regex.Replace(content, @"TemplatePresenter", $"{moduleName}Presenter");
            content = Regex.Replace(content, @"TemplateView", $"{moduleName}View");
            content = Regex.Replace(content, @"TemplateCommands", $"{moduleName}Commands");
            
            // Replace specific field names
            content = Regex.Replace(content, @"screenTitle", $"{moduleNameLower}ScreenTitle");
            
            // Replace template with moduleName in lowercase for variables and fields
            content = Regex.Replace(content, @"(_?)(template)", match =>
            {
                string prefix = match.Groups[1].Value;
                string templateWord = match.Groups[2].Value;
                return prefix + (char.IsUpper(templateWord[0]) ? moduleName : moduleNameLower);
            }, RegexOptions.IgnoreCase);
            
            return content;
        }

        private static void CreateScript(string folderPath, string fileName, string scriptContent)
        {
            if (string.IsNullOrEmpty(scriptContent))
                return;

            string filePath = PathManager.CombinePaths(folderPath, fileName);

            WriteToFile(filePath, scriptContent);
        }

        private static void WriteToFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error writing file {Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        private static void CopyAndAdjustAsmdef(string templateAsmdefPath, string targetAsmdefPath, string moduleName)
        {
            string content = ReadTemplateFile(templateAsmdefPath);
            if (content == null)
            {
                EditorUtility.DisplayDialog("Missing asmdef Template",
                    $"Template asmdef file not found at {templateAsmdefPath}.\n" +
                    $"\nCannot create asmdef file.", "OK");
                return;
            }
            content = AdjustAsmdefContent(content, moduleName);
            WriteToFile(targetAsmdefPath, content);
        }

        private static string AdjustAsmdefContent(string content, string moduleName) =>
            Regex.Replace(content, @"""name"":\s*""[^""]+""", $@"""name"": ""{moduleName}Module""");
    }
}