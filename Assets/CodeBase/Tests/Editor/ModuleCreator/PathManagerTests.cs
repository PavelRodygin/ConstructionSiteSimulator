using CodeBase.Editor.ModuleCreator.Base;
using NUnit.Framework;

namespace CodeBase.Tests.Editor.ModuleCreator
{
    public class PathManagerTests
    {
        [SetUp]
        public void Setup() => PathManager.InitializePaths();

        [Test]
        public void InitializePaths_ShouldSetCorrectPaths()
        {
            Assert.AreEqual("Assets/Modules/Additional", PathManager.AdditionalFolderPath);
            Assert.AreEqual("Assets/Modules/Base", PathManager.BaseFolderPath);
            Assert.AreEqual("Assets/Modules/Test", PathManager.TestFolderPath);
            Assert.AreEqual("Assets/Modules/Template/TemplateModule", 
                PathManager.TemplateModuleFolderPath);
            Assert.AreEqual("Assets/Modules/Template/TemplateModule/Views",
                PathManager.TemplateViewsFolderPath);
            Assert.AreEqual("Assets/Modules/Template/TemplateModule/Scripts",
                PathManager.TemplateScriptsFolderPath);
            Assert.AreEqual("Assets/Modules/Template/TemplateModule/Views/TemplateModuleView.prefab",
                PathManager.TemplateViewPrefabPath);
        }

        [Test]
        public void GetFolderType_ShouldReturnCorrectFolderType()
        {
            string path = "Assets/Modules/Base/SomeModule";
            string folderType = PathManager.GetFolderType(path);
            Assert.AreEqual("Base", folderType);
        }
    }
}