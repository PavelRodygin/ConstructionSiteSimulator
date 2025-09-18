#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Base.ConfigManagement
{
    public static class ModuleStructureConfigWatcher
    {
        public const string ConfigPath = "Assets/Editor/ModuleCreator/Base/ModuleStructureConfig.asset";

        [DidReloadScripts] // Invokes after recompilation of code
        private static void OnScriptsReloaded()
        {
            ModulePathCache.RefreshPaths();
        }
    }

    public class ModuleStructureConfigPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string asset in importedAssets)
            {
                if (asset != ModuleStructureConfigWatcher.ConfigPath) continue;
                Debug.Log("ModuleStructureConfig изменён! Перезагрузка путей...");
                ModulePathCache.RefreshPaths();
            }
        }
    }
#endif
}