using CodeBase.Editor.ModuleCreator.Base.ConfigManagement;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Configs
{
    [CreateAssetMenu(fileName = "ModuleStructureConfig", menuName = "Module Creator/Structure Config", order = 1)]
    public class ModuleStructureConfig : ScriptableObject
    {
        [Header("Structure folders")]
        public string scriptsFolderName = "Scripts";
        public string viewsFolderName = "Views";
        public string scenesFolderName = ModulePathCache.ScenesFolderName;
    }
}