using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Tasks.CreateSceneTask
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<string, Type> TypeCache = new();
        private const int MaxRetries = 5;
        private const int RetryDelayMs = 100;

        public static Type FindType(string fullName)
        {
            if (TypeCache.TryGetValue(fullName, out Type cachedType))
                return cachedType;

            Type type = null;
            int retryCount = 0;

            while (type == null && retryCount < MaxRetries)
            {
                if (retryCount > 0)
                {
                    Debug.Log($"Retry {retryCount} for finding type '{fullName}'. Refreshing AssetDatabase...");
                    AssetDatabase.Refresh();
                    Thread.Sleep(RetryDelayMs);
                }

                type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == fullName);

                retryCount++;
            }

            if (type != null)
            {
                TypeCache[fullName] = type;
                Debug.Log($"Successfully found type '{fullName}' after {retryCount} attempts.");
            }
            else
            {
                Debug.LogError($"Type '{fullName}' not found after {MaxRetries} attempts with AssetDatabase refresh.");
                // Clear cache on failure to allow future attempts
                TypeCache.Remove(fullName);
            }

            return type;
        }

        public static void ClearTypeCache()
        {
            TypeCache.Clear();
            Debug.Log("Type cache cleared.");
        }

        public static void SetPrivateField<T>(object obj, string fieldName, T value)
        {
            if (obj == null)
            {
                Debug.LogError("Object is null when trying to set private field.");
                return;
            }

            FieldInfo field = FindFieldInHierarchy(obj.GetType(), fieldName);
            if (field != null)
                field.SetValue(obj, value);
            else
                Debug.LogError($"Field '{fieldName}' not found in {obj.GetType().Name} or its base classes.");
        }

        private static FieldInfo FindFieldInHierarchy(Type type, string fieldName)
        {
            // Search in current type and all base types
            Type currentType = type;
            while (currentType != null)
            {
                FieldInfo field = currentType.GetField(fieldName,
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field;
                
                currentType = currentType.BaseType;
            }
            return null;
        }

        public static Component GetComponentByName(GameObject gameObject, string componentName)
        {
            if (gameObject == null)
            {
                Debug.LogError("GameObject is null when trying to get component.");
                return null;
            }

            Component component = gameObject.GetComponent(componentName);
            if (component == null)
                Debug.LogError($"Component '{componentName}' not found on {gameObject.name}.");

            return component;
        }
    }
}