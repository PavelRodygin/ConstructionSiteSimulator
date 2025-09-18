using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace CodeBase.Editor.ModuleCreator.Base
{
    public static class TemplateValidator
    {
        private static readonly List<string> RequiredTemplates = new()
        {
            "TemplateModuleInstaller.cs",
            "TemplateModuleController.cs",
            "TemplatePresenter.cs",
            "TemplateView.cs",
            "TemplateModuleModel.cs"
        };

        public static bool AreTemplatesAvailable(bool createAsmdef)
        {
            if (!AssetDatabase.IsValidFolder(PathManager.TemplateScriptsFolderPath))
            {
                ShowDialog("Missing Template Folder",
                    $"Template folder not found at {PathManager.TemplateScriptsFolderPath}.\n" +
                    $"\nModule creation aborted.");
                return false;
            }

            if (MissingTemplateFiles())
                return false;

            if (createAsmdef && !AsmdefTemplateExists())
                return false;

            if (!PrefabTemplateExists())
                return false;

            return true;
        }

        private static bool MissingTemplateFiles()
        {
            var missingTemplates = RequiredTemplates.Where(template =>
                !File.Exists(PathManager.CombinePaths(PathManager.TemplateScriptsFolderPath, template))).ToList();
            if (missingTemplates.Any())
            {
                string missing = string.Join("\n", missingTemplates);
                ShowDialog("Missing Templates", $"The following template files are missing:\n" +
                                                $"{missing}\n\nModule creation aborted.");
                return true;
            }
            return false;
        }

        private static bool AsmdefTemplateExists()
        {
            string templateAsmdefPath = PathManager.CombinePaths(PathManager.TemplateModuleFolderPath,
                "Template.asmdef");
            if (!File.Exists(templateAsmdefPath))
            {
                ShowDialog("Missing asmdef Template",
                    $"Template asmdef file not found at {templateAsmdefPath}.\n\nModule creation aborted.");
                return false;
            }
            return true;
        }

        private static bool PrefabTemplateExists()
        {
            if (!File.Exists(PathManager.TemplateViewPrefabPath))
            {
                ShowDialog("Missing Prefab Template",
                    $"Template prefab not found at {PathManager.TemplateViewPrefabPath}.\n" +
                    $"\nModule creation aborted.");
                return false;
            }
            return true;
        }

        private static void ShowDialog(string title, string message) =>
            EditorUtility.DisplayDialog(title, message, "OK");
    }
}
