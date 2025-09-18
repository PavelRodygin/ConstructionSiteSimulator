using CodeBase.Editor.ModuleCreator.Base.ConfigManagement;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Base
{
    public static class PathManager
    {
        public static string AdditionalFolderPath { get; private set; }
        public static string BaseFolderPath { get; private set; }
        public static string TestFolderPath { get; private set; }
        public static string TemplateScriptsFolderPath { get; private set; }
        public static string TemplateModuleFolderPath { get; private set; }
        public static string TemplateViewsFolderPath { get; private set; }
        public static string TemplateViewPrefabPath { get; private set; }

        private const string BasePath = "Assets/Modules";

        public static void InitializePaths()
        {
            AdditionalFolderPath = CombinePaths(BasePath, "Additional");
            BaseFolderPath = CombinePaths(BasePath, "Base");
            TestFolderPath = CombinePaths(BasePath, "Test");
            
            TemplateModuleFolderPath = CombinePaths(BasePath, "Template", "TemplateModule");
            TemplateViewsFolderPath = CombinePaths(TemplateModuleFolderPath, ModulePathCache.ViewsFolderName);
            TemplateScriptsFolderPath = CombinePaths(TemplateModuleFolderPath, ModulePathCache.ScriptsFolderName);
            TemplateViewPrefabPath = CombinePaths(TemplateViewsFolderPath, "TemplateModuleView.prefab");

            EnsureSubfoldersExist();
        }

        private static void EnsureSubfoldersExist()
        {
            CreateFolderIfNotExists(AdditionalFolderPath);
            CreateFolderIfNotExists(BaseFolderPath);
            CreateFolderIfNotExists(TestFolderPath);
        }

        public static string CombinePaths(params string[] paths) =>
            string.Join("/", paths).Replace("\\", "/");

        private static void CreateFolderIfNotExists(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
                string newFolderName = System.IO.Path.GetFileName(folderPath);
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
            }
        }

        public static string GetFolderType(string path)
        {
            string[] pathParts = path.Split('/');
            int modulesIndex = System.Array.IndexOf(pathParts, "Modules");
            if (modulesIndex >= 0 && modulesIndex + 1 < pathParts.Length)
                return pathParts[modulesIndex + 1];
            Debug.LogError("Folder type not found in path: " + path);
            return "";
        }
    }
}
