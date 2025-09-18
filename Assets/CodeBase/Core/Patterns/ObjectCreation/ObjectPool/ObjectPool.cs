using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Core.Patterns.ObjectCreation.ObjectPool
{
    public abstract class ObjectPool<T> : IObjectPool<T> where T : class
    {
        protected readonly IFactory<T> Factory;
        private readonly Stack<T> _inactiveObjects;
        private readonly HashSet<T> _activeObjects;
        private readonly Transform _inactiveParent;
        private int _maxSize = -1;

        protected ObjectPool(
            IFactory<T> factory,
            Transform inactiveObjectsParent = null,
            int initialSize = 0,
            int maxSize = -1)
        {
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _inactiveObjects = new Stack<T>(initialSize);
            _activeObjects = new HashSet<T>();
            _inactiveParent = inactiveObjectsParent ?? new GameObject($"Pool_{typeof(T).Name}").transform;
            _maxSize = maxSize;

            Prewarm(initialSize);
        }

        public virtual T Spawn()
        {
            T obj;
            while (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Pop();
                if (IsValid(obj))
                {
                    _activeObjects.Add(obj);
                    OnSpawned(obj);
                    return obj;
                }
            }

            if (_maxSize < 0 || TotalCount < _maxSize)
            {
                obj = Factory.Create();
                if (IsValid(obj))
                {
                    _activeObjects.Add(obj);
                    OnSpawned(obj);
                    return obj;
                }
            }

            Debug.LogWarning($"No valid objects available in pool for {typeof(T).Name} and max size reached.");
            return null;
        }

        public virtual void Despawn(T obj)
        {
            if (obj == null || !_activeObjects.Contains(obj))
            {
                Debug.LogWarning(obj == null
                    ? "Attempted to despawn a null object."
                    : $"Object of type {typeof(T).Name} is not managed by this pool.");
                return;
            }

            _activeObjects.Remove(obj);
            if (IsValid(obj))
            {
                _inactiveObjects.Push(obj);
                OnDespawned(obj);
                if (obj is MonoBehaviour mono && mono != null)
                {
                    mono.transform.SetParent(_inactiveParent, false);
                }
            }
        }

        public void Prewarm(int count)
        {
            if (_maxSize >= 0 && TotalCount + count > _maxSize)
            {
                count = _maxSize - TotalCount;
            }

            for (int i = 0; i < count; i++)
            {
                T obj = Factory.Create();
                if (IsValid(obj))
                {
                    _inactiveObjects.Push(obj);
                    OnDespawned(obj);
                    if (obj is MonoBehaviour mono && mono != null)
                    {
                        mono.transform.SetParent(_inactiveParent, false);
                    }
                }
            }
        }

        public void SetMaxSize(int maxSize)
        {
            _maxSize = maxSize;
            TrimExcess();
        }

        public int ActiveCount => _activeObjects.Count;
        public int InactiveCount => _inactiveObjects.Count;
        public int TotalCount => ActiveCount + InactiveCount;

        public void Clear()
        {
            foreach (var obj in _activeObjects) DestroyIfUnityObject(obj);
            foreach (var obj in _inactiveObjects) DestroyIfUnityObject(obj);
            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        protected abstract void OnSpawned(T tooltipView);
        protected abstract void OnDespawned(T obj);

        private void TrimExcess()
        {
            while (_maxSize >= 0 && TotalCount > _maxSize && _inactiveObjects.Count > 0)
            {
                T obj = _inactiveObjects.Pop();
                DestroyIfUnityObject(obj);
            }
        }

        private bool IsValid(T obj)
        {
            return obj != null && (obj is not UnityEngine.Object unityObj || unityObj != null);
        }

        private void DestroyIfUnityObject(T obj)
        {
            if (obj is UnityEngine.Object unityObj && unityObj != null)
            {
                UnityEngine.Object.Destroy(unityObj);
            }
        }
    }
}