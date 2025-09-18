using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor.ModuleCreator.Tasks
{
    [InitializeOnLoad]
    public static class TaskQueue
    {
        public static UniTaskCompletionSource UniTaskCompletionSource;
        private static readonly Queue<Task> Tasks = new();
        private static bool _isExecuting;


        static TaskQueue()
        {
            LoadState();
            EditorApplication.update += OnEditorUpdate;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            Init();
            AwaitTaskCompleted();
        }

        public static void Init() => UniTaskCompletionSource = new UniTaskCompletionSource();


        private static void AwaitTaskCompleted()
        {
            // UniTask.ToCoroutine(async () =>
            // {
            //     await UniTaskCompletionSource.Task;
            //     Debug.Log("Tasks Completed");
            // });
        }

        public static void EnqueueTask(Task task)
        {
            Tasks.Enqueue(task);
            SaveState();
            ExecuteNextTask();
        }

        public static void OnEditorUpdate()
        {
            if (!_isExecuting && Tasks.Count > 0)
                ExecuteNextTask();
        }

        private static void ExecuteNextTask()
        {
            if (_isExecuting || Tasks.Count == 0)
                return;

            _isExecuting = true;
            var task = Tasks.Peek();
            task.Execute();
            HandleTaskCompletion(task);
        }

        private static void HandleTaskCompletion(Task task)
        {
            if (task.WaitForCompilation)
            {
                if (EditorApplication.isCompiling)
                    EditorApplication.update += WaitForCompilation;
                else
                {
                    CompleteTask();
                    ExecuteNextTask();
                }
            }
            else
            {
                CompleteTask();
                ExecuteNextTask();
            }
        }

        private static void WaitForCompilation()
        {
            if (!EditorApplication.isCompiling)
            {
                EditorApplication.update -= WaitForCompilation;
                
                // Force asset database refresh after compilation to ensure new scripts are indexed
                AssetDatabase.Refresh();
                
                // Clear type cache to ensure fresh lookups after compilation
                CodeBase.Editor.ModuleCreator.Tasks.CreateSceneTask.ReflectionHelper.ClearTypeCache();
                
                // Small delay to allow Unity to fully process the refreshed assets
                EditorApplication.delayCall += () =>
                {
                    CompleteTask();
                    ExecuteNextTask();
                };
            }
        }

        private static void CompleteTask()
        {
            Tasks.Dequeue();
            _isExecuting = false;
            SaveState();
            ClearCompletedTasks();
            if (Tasks.Count == 0) 
                UniTaskCompletionSource.TrySetResult();
        }

        private static void OnAfterAssemblyReload()
        {
            _isExecuting = false;
            LoadState();
        }

        private static void SaveState()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string tasksJson = JsonConvert.SerializeObject(Tasks.ToList(), settings);
            SessionState.SetString("TaskQueueState", tasksJson);
        }

        private static void LoadState()
        {
            string tasksJson = SessionState.GetString("TaskQueueState", "");
            if (!string.IsNullOrEmpty(tasksJson))
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                try
                {
                    var tasksList = JsonConvert.DeserializeObject<List<Task>>(tasksJson, settings);
                    Tasks.Clear();
                    if (tasksList != null)
                        foreach (var task in tasksList)
                            Tasks.Enqueue(task);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error deserializing tasks: " + ex.Message);
                }
            }
        }

        public static void ClearCompletedTasks()
        {
            if (Tasks.Count == 0)
            {
                SessionState.EraseString("TaskQueueState");
                _isExecuting = false;
            }
        }
    }
}