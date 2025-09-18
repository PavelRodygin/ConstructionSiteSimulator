namespace CodeBase.Core.Patterns.ObjectCreation.ObjectPool
{
    public interface IObjectPool<T> where T : class
    {
        T Spawn();
        void Despawn(T obj);
        void Prewarm(int count);
        int ActiveCount { get; }
        int InactiveCount { get; }
        int TotalCount { get; }
        void Clear();
        void SetMaxSize(int maxSize);
    }
}