using System;
using CodeBase.Core.UI;
using CodeBase.Implementation.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.Editor.ModuleCreator.Tasks.CreateSceneTask
{
    public static class GameObjectFactory
    {
        public static GameObject CreateCanvas()
        {
            var canvas = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvas, "Create Canvas");
            var canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            canvas.AddComponent<ModuleCanvas>();
            return canvas;
        }

        public static GameObject InstantiateViewPrefab(string prefabPath, GameObject parent)
        {
            var viewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (viewPrefab == null)
            {
                Debug.LogError($"View prefab not found at {prefabPath}");
                return null;
            }

            GameObject viewInstance = PrefabUtility.InstantiatePrefab(viewPrefab) as GameObject;
            if (viewInstance == null)
            {
                Debug.LogError($"Failed to instantiate View prefab at {prefabPath}");
                return null;
            }

            viewInstance.transform.SetParent(parent.transform, false);
            Undo.RegisterCreatedObjectUndo(viewInstance, "Instantiate View Prefab");
            return viewInstance;
        }

        public static GameObject InstantiateInstaller(string installerName, Type installerType)
        {
            if (installerType == null)
            {
                Debug.LogError("Installer type is null.");
                return null;
            }

            GameObject installerObject = new GameObject(installerName);
            Undo.RegisterCreatedObjectUndo(installerObject, "Instantiate Installer");
            installerObject.AddComponent(installerType);
            return installerObject;
        }

        public static Camera CreateModuleCamera()
        {
            GameObject cameraObject = new GameObject("ModuleCamera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Module Camera");
            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.Skybox;
            cameraComponent.cullingMask = LayerMask.GetMask("Default");
            return cameraComponent;
        }

        /// <summary>
        /// Creates a template hierarchy structure that matches the Template module scene layout:
        /// 
        /// All objects are created at scene root level (no parent):
        /// ├── Systems
        /// │   └── ModuleNameModuleInstaller
        /// ├── ModuleCanvas (with Canvas, CanvasScaler, GraphicRaycaster, ModuleCanvas components)
        /// └── ModuleCamera (with Camera component, orthographic, positioned at 0,0,-10)
        /// 
        /// This ensures all new modules have the same clean, organized structure as Template.unity.
        /// </summary>
        public static void CreateTemplateHierarchy(string moduleName)
        {
            // Create Systems container at scene root
            GameObject systemsObject = new GameObject("Systems");
            Undo.RegisterCreatedObjectUndo(systemsObject, "Create Systems Container");

            // Create ModuleInstaller under Systems
            GameObject installerObject = new GameObject($"{moduleName}ModuleInstaller");
            Undo.RegisterCreatedObjectUndo(installerObject, $"Create {moduleName}ModuleInstaller");
            installerObject.transform.SetParent(systemsObject.transform, false);

            // Create ModuleCanvas at scene root (no parent)
            GameObject canvasObject = new GameObject("ModuleCanvas");
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create ModuleCanvas");
            
            var canvasComponent = canvasObject.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<ModuleCanvas>();

            // Create ModuleCamera at scene root (no parent)
            GameObject cameraObject = new GameObject("ModuleCamera");
            Undo.RegisterCreatedObjectUndo(cameraObject, "Create ModuleCamera");
            Camera cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.Skybox;
            cameraComponent.cullingMask = LayerMask.GetMask("Default");
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5f;
            cameraComponent.depth = -1;
            cameraObject.transform.position = new Vector3(0, 0, -10);
        }
    }
}
