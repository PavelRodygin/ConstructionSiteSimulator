using System.Collections;
using System.Collections.Generic;
using System.IO;
using CodeBase.Editor.ModuleCreator;
using CodeBase.Editor.ModuleCreator.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace CodeBase.Tests.Editor.ModuleCreator
{
    public class ModuleCreatorWindowTests
    {
        private const string ModuleName = "NewModule";
        private const string TrackingFilePath = "Assets/Scripts/Editor/ModuleCreator/CreatedModules.json";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(TrackingFilePath))
            {
                File.Delete(TrackingFilePath);
                const string metaPath = TrackingFilePath + ".meta";
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(TrackingFilePath))
            {
                File.Delete(TrackingFilePath);
                const string metaPath = TrackingFilePath + ".meta";
                if (File.Exists(metaPath))
                    File.Delete(metaPath);
            }
            AssetDatabase.Refresh();
        }

        [UnityTest]
        public IEnumerator CreateModule_ShouldCreateModuleFolder() => UniTask.ToCoroutine(async () =>
        {
            // arrange 
            var moduleCreatorWindow = ScriptableObject.CreateInstance<ModuleCreatorWindow>();
            moduleCreatorWindow.ShowUtility();
            moduleCreatorWindow.CreateModule();
            
            // act
            await TaskQueue.UniTaskCompletionSource.Task;

            // assert
            Assert.IsTrue(File.Exists(TrackingFilePath), "Tracking file was not created.");

            string json = await File.ReadAllTextAsync(TrackingFilePath);
            List<string> createdModules = JsonConvert.DeserializeObject<List<string>>(json);
            Assert.IsNotNull(createdModules, "CreatedModules list is null.");
            Assert.IsTrue(createdModules.Count > 0, "CreatedModules list is empty.");

            string targetModuleFolderPath = createdModules.Find(path => path.Contains(ModuleName));
            Assert.IsFalse(string.IsNullOrEmpty(targetModuleFolderPath),
                "Module path not found in tracking file.");

            Debug.Log($"Created module path: {targetModuleFolderPath}");

            Assert.IsTrue(AssetDatabase.IsValidFolder(targetModuleFolderPath),
                "Module folder was not created.");
        });

        /*[UnityTest]
        public IEnumerator DeleteLastModule_ShouldDeleteModuleFolder() => UniTask.ToCoroutine(async () =>
        {
            // arrange 
            var moduleCreatorWindow = ScriptableObject.CreateInstance<ModuleCreatorWindow>();
            moduleCreatorWindow.ShowUtility();
            moduleCreatorWindow.CreateModule();

            await TaskQueue.UniTaskCompletionSource.Task;

            string json = await File.ReadAllTextAsync(TrackingFilePath);
            List<string> createdModules = JsonConvert.DeserializeObject<List<string>>(json);
            string targetModuleFolderPath = createdModules.Find(path => path.Contains(ModuleName));

            // act
            moduleCreatorWindow.DeleteLastModule();
            AssetDatabase.Refresh();

            // assert
            Assert.IsFalse(AssetDatabase.IsValidFolder(targetModuleFolderPath),
                "Module folder was not deleted.");

            string updatedJson = await File.ReadAllTextAsync(TrackingFilePath);
            List<string> updatedCreatedModules = JsonConvert.DeserializeObject<List<string>>(updatedJson);
            Assert.IsFalse(updatedCreatedModules.Contains(targetModuleFolderPath),
                "Module path still exists in tracking file.");
        });*/
    }
}